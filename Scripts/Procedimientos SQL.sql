DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ActualizarEmpresa`(
    IN p_EmpresaId INT,
    IN p_Nombre VARCHAR(100),
    IN p_RazonSocial VARCHAR(150),
    IN p_Activo BOOL,
    OUT p_Mensaje VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Empresas WHERE Nombre = p_Nombre AND EmpresaId <> p_EmpresaId) THEN
        SET p_Mensaje = 'Ya existe una empresa con ese nombre.';
    ELSE
        UPDATE Empresas
        SET Nombre = p_Nombre,
            RazonSocial = p_RazonSocial,
            Activo = p_Activo
        WHERE EmpresaId = p_EmpresaId;
        SET p_Mensaje = 'Empresa actualizada correctamente.';
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ActualizarRol`(
    IN p_RolId INT,
    IN p_Nombre VARCHAR(50),
    IN p_Activo BOOL,
    OUT p_Mensaje VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Roles WHERE Nombre = p_Nombre AND RolId <> p_RolId) THEN
        SET p_Mensaje = 'Ya existe un rol con ese nombre.';
    ELSE
        UPDATE Roles
        SET Nombre = p_Nombre,
         Activo = p_Activo
        WHERE RolId = p_RolId;
        SET p_Mensaje = 'Rol actualizado correctamente.';
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ActualizarUsuario`(
    IN p_UsuarioId INT,
    IN p_Nombre VARCHAR(100),
    IN p_Email VARCHAR(100),
    IN p_PasswordHash VARCHAR(255),
    IN p_EmpresaId INT,
    IN p_RolId INT,
    IN p_Activo BOOLEAN,
    OUT p_Mensaje VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Usuarios WHERE Email = p_Email AND UsuarioId <> p_UsuarioId) THEN
        SET p_Mensaje = 'El correo ya está en uso por otro usuario.';
    ELSE
        UPDATE Usuarios
        SET Nombre = IFNULL(p_Nombre, Nombre),
            Email = IFNULL(p_Email, Email),
            PasswordHash = IFNULL(p_PasswordHash, PasswordHash),
            EmpresaId = IFNULL(p_EmpresaId, EmpresaId),
            RolId = IFNULL(p_RolId, RolId),
            Activo = IFNULL(p_Activo, Activo)
        WHERE UsuarioId = p_UsuarioId;
        SET p_Mensaje = 'Usuario actualizado correctamente.';
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_CrearEmpresa`(
    IN p_Nombre VARCHAR(100),
    IN p_RazonSocial VARCHAR(150),
    OUT p_Mensaje VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Empresas WHERE Nombre = p_Nombre) THEN
        SET p_Mensaje = 'La empresa ya está registrada con ese nombre.';
    ELSE
        INSERT INTO Empresas (Nombre, RazonSocial)
        VALUES (p_Nombre, p_RazonSocial);
        SET p_Mensaje = 'Empresa creada correctamente.';
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_CrearRol`(
    IN p_Nombre VARCHAR(50),
    OUT p_Mensaje VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Roles WHERE Nombre = p_Nombre) THEN
        SET p_Mensaje = 'El rol ya está registrado con ese nombre.';
    ELSE
        INSERT INTO Roles (Nombre)
        VALUES (p_Nombre);
        SET p_Mensaje = 'Rol creado correctamente.';
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_CrearUsuario`(
    IN p_Nombre VARCHAR(100),
    IN p_Email VARCHAR(100),
    IN p_PasswordHash VARCHAR(255),
    IN p_EmpresaId INT,
    IN p_RolId INT,
    IN p_Activo BOOLEAN,
    OUT p_Mensaje VARCHAR(255)
)
BEGIN
    IF EXISTS (SELECT 1 FROM Usuarios WHERE Email = p_Email) THEN
        SET p_Mensaje = 'El correo ya está registrado.';
    ELSE
        INSERT INTO Usuarios (Nombre, Email, PasswordHash, EmpresaId, RolId, Activo)
        VALUES (p_Nombre, p_Email, p_PasswordHash, p_EmpresaId, p_RolId, p_Activo);
        SET p_Mensaje = 'Usuario creado correctamente.';
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DesactivarEmpresa`(
    IN p_EmpresaId INT,
    IN p_Activar bool
)
BEGIN
	UPDATE Empresas SET Activo = p_Activar WHERE EmpresaId = p_EmpresaId;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DesactivarRol`(
    IN p_RolId INT,
    IN p_Activar BOOL
)
BEGIN
    UPDATE Roles SET Activo = p_Activar WHERE RolId = p_RolId;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_DesactivarUsuario`(
    IN p_UsuarioId INT,
    IN p_Activar bool
)
BEGIN

	UPDATE Usuarios SET Activo = p_Activar WHERE UsuarioId = p_UsuarioId;
   
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GenerarVistaPorCategoria`(IN categoriaNombre text)
BEGIN
	 -- Variables
    DECLARE done INT DEFAULT 0;
    DECLARE colCsv VARCHAR(100);
    DECLARE colAux VARCHAR(100);
    DECLARE selectList TEXT DEFAULT '';
    DECLARE catId INT;
    DECLARE table_schema_name VARCHAR(100);

    -- Cursor (debe ir después de variables)
    DECLARE cur CURSOR FOR 
        SELECT CsvColumnName, AuxColumnName
        FROM tmp_columnas_validas;

    -- Handler (debe ir después del cursor)
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;
    
    DROP TEMPORARY TABLE IF EXISTS tmp_columnas_validas;

    -- Determinar el esquema actual
    SET table_schema_name = DATABASE();

    -- Obtener ID de categoría
    SELECT CategoriaId INTO catId 
    FROM categoriasarchivo 
    WHERE Nombre = categoriaNombre 
    LIMIT 1;

    IF catId IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'Categoría no encontrada';
    END IF;

    -- Crear tabla temporal para columnas válidas
    CREATE TEMPORARY TABLE tmp_columnas_validas (
        CsvColumnName VARCHAR(100),
        AuxColumnName VARCHAR(100)
    );

    -- Insertar solo columnas mapeadas que sí existen en la tabla datos
    INSERT INTO tmp_columnas_validas (CsvColumnName, AuxColumnName)
    SELECT am.CsvColumnName, am.AuxColumnName
    FROM archivosmapping am
    WHERE am.CategoriaId = catId
      AND EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS c
        WHERE c.TABLE_NAME = 'datos'
          AND c.TABLE_SCHEMA = table_schema_name
          AND c.COLUMN_NAME = am.AuxColumnName
      );

    -- Usar el cursor para construir el SELECT dinámico
    OPEN cur;

    read_loop: LOOP
        FETCH cur INTO colCsv, colAux;
        IF done THEN
            LEAVE read_loop;
        END IF;

        -- SET selectList = CONCAT_WS(', ', selectList, CONCAT('datos.`', colAux, '` AS `', colCsv, '`'));
        IF selectList = '' THEN
			SET selectList = CONCAT('datos.`', colAux, '` AS `', colCsv, '`');
		ELSE
			SET selectList = CONCAT(selectList, ', datos.`', colAux, '` AS `', colCsv, '`');
		END IF;
    END LOOP;

    CLOSE cur;

    -- Validación final
    IF selectList IS NULL OR selectList = '' THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'No hay columnas válidas para construir la vista.';
    END IF;

    -- Armar SQL dinámico
    SET @sql = CONCAT(
        'CREATE OR REPLACE VIEW vista_', REPLACE(categoriaNombre, ' ', '_'), ' AS ',
        'SELECT ', selectList, ',
                datos.ArchivoId,
                archivos.NombreOriginal,
                categoriasarchivo.Nombre AS CategoriaNombre ',
        'FROM datos ',
        'INNER JOIN archivos ON archivos.ArchivoId = datos.ArchivoId ',
        'INNER JOIN categoriasarchivo ON categoriasarchivo.CategoriaId = archivos.CategoriaId ',
        'WHERE categoriasarchivo.Nombre = \'', categoriaNombre, '\''
    );

    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;

    -- Limpiar tabla temporal
    DROP TEMPORARY TABLE IF EXISTS tmp_columnas_validas;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_getFileMapping`(IN p_CategoryId int)
BEGIN
	-- DECLARE id_categoria INT;
    -- set id_categoria = (select CategoriaId  from categoriasarchivo where Nombre = fileName limit 1);

	select A.CategoriaId, A.CsvColumnName, A.AuxColumnName, A.IndexCsvColumn, A.CreatedAt, B.Delimiter from ArchivosMapping A
    LEFT JOIN LATERAL (
		select delimiter from categoriasarchivo where categoriaId = p_CategoryId limit 1
    ) B ON true
    where A.CategoriaId = p_CategoryId
    group by A.CategoriaId, A.CsvColumnName, A.AuxColumnName, A.IndexCsvColumn, A.CreatedAt, B.Delimiter
    having A.CreatedAt = max(A.CreatedAt);
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetRolByUserId`(IN p_userId int)
BEGIN
	select roles.RolId, roles.Nombre from proyectotitulo.roles
	inner join proyectotitulo.usuarios on usuarios.RolId = roles.RolId
	where usuarios.UsuarioId = p_userId limit 1;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_getWidgets`(IN p_userId INT)
BEGIN
	DECLARE v_empresaId INT;
     SET v_empresaId = (
            SELECT EmpresaId
            FROM usuarios
            WHERE UsuarioId = p_userId
            LIMIT 1
        );
        
        select 
        widgets.Url
        ,widgets.Nombre
        ,empresas.Nombre as Empresa
        from widgets
        inner join empresas on empresas.EmpresaId = widgets.EmpresaId
        where widgets.EmpresaId = v_empresaId;
        
        
        
         -- Insertar log
        CALL sp_InsertarLog(
            p_userId,
            v_empresaId,
            'Obtener Widgets',
            CONCAT('El usuario Id ', p_userId, ' solicitó listado de widgets.')
        );
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertarLog`(
	IN p_UsuarioId INT,
    IN p_EmpresaId INT,
    IN p_Accion VARCHAR(100),
    IN p_Detalle TEXT)
BEGIN
	INSERT INTO logs (UsuarioId, EmpresaId, Accion, Detalle)
    VALUES (p_UsuarioId, p_EmpresaId, p_Accion, p_Detalle);
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insertFileCategory`(IN userId int, IN filename VARCHAR(255),IN fileDesc VARCHAR(255),IN fileDelimiter VARCHAR(10), IN data JSON)
BEGIN
    DECLARE newCategoriaId INT;
    DECLARE i INT DEFAULT 0;
    DECLARE total INT;
    DECLARE empIdEncontrada INT;
    
    SET empIdEncontrada = (SELECT EmpresaId from proyectotitulo.usuarios where UsuarioId = userId limit 1);
    
    IF empIdEncontrada IS NULL THEN
		SIGNAL SQLSTATE '45000'
		SET MESSAGE_TEXT = 'Error: empresaId no se pudo obtener para el usuario';
	END IF;
    
	INSERT INTO proyectotitulo.categoriasarchivo (Nombre, Descripcion, EmpresaId, UsuarioId, FechaCreacion, Delimiter)
	VALUES (filename, fileDesc, empIdEncontrada, userId, now(), fileDelimiter);
    
    -- Capturar el ID del registro insertado
	SET newCategoriaId = LAST_INSERT_ID();
    
    -- Obtén la cantidad de registros en el JSON
    SET total = JSON_LENGTH(data);

    -- Bucle para recorrer cada registro del JSON
    WHILE i < total DO
        
		INSERT INTO proyectotitulo.archivosmapping
				(
					CategoriaId,
					CsvColumnName,
					AuxColumnName,
                    IndexCsvColumn,
					CreatedAt
				)
				VALUES (
					newCategoriaId,
					JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].CsvColumnName'))),
					JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxColumnName'))),
                    JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].IndexCsvColumn'))),
					NOW()
				);
		SET i = i + 1; -- Avanzar al siguiente registro
    END WHILE;
    
    CALL sp_InsertarLog(userId, empIdEncontrada, 'Generar Categoria', concat('El usuario creó la categoría ', filename, '.'));
    
    CALL sp_GenerarVistaPorCategoria(filename);
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insertFileData`(IN p_userid int, IN p_categoryId int, IN data JSON)
BEGIN
    DECLARE i INT DEFAULT 0;
    DECLARE total INT;
    DECLARE nombre_archivo varchar(255);
    DECLARE newArchivoId INT;
    DECLARE v_empresaid INT;
    
    
    -- Obtén la cantidad de registros en el JSON
    SET total = JSON_LENGTH(data);
	set v_empresaid = (select empresaId from usuarios where usuarioId = p_userid limit 1);
	SET nombre_archivo = "nombre temporal";
    
    INSERT INTO archivos
    (
		NombreOriginal,
        FechaSubida,
        UsuarioId,
        EmpresaId,
        CategoriaId
    )
    VALUES 
    (	nombre_archivo,
		NOW(),
        p_userid,
        v_empresaid,
		p_categoryId
    );
    
    -- Capturar el ID del registro insertado
    SET newArchivoId = LAST_INSERT_ID();

    -- Bucle para recorrer cada registro del JSON
    WHILE i < total DO
        INSERT INTO datos
        (
            ArchivoId,
            AuxString1,
            AuxString2,
            AuxString3,
            AuxString4,
            AuxString5,
            AuxString6,
            AuxString7,
            AuxString8,
            AuxString9,
            AuxString10,
            AuxString11,
            AuxString12,
            AuxString13,
            AuxString14,
            AuxString15,
            AuxDecimal1,
            AuxDecimal2,
            AuxDecimal3,
            AuxDecimal4,
            AuxDecimal5,
            AuxDecimal6,
            AuxDecimal7,
            AuxDecimal8,
            AuxDecimal9,
            AuxDecimal10,
            AuxDecimal11,
            AuxDecimal12,
            AuxDecimal13,
            AuxDecimal14,
            AuxDecimal15,
            AuxDateTime1,
            AuxDateTime2,
            AuxDateTime3,
            AuxDateTime4,
            AuxDateTime5,
			AuxDateTime6,
            AuxDateTime7,
            AuxDateTime8,
            AuxDateTime9,
            AuxDateTime10,
            AuxDateTime11,
            AuxDateTime12,
            AuxDateTime13,
            AuxDateTime14,
            AuxDateTime15
        )
        VALUES (
            newArchivoId,
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString1'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString2'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString3'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString4'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString5'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString6'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString7'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString8'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString9'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString10'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString11'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString12'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString13'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString14'))), '"', ''),
            REPLACE(JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString15'))), '"', ''),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal1')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal2')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal3')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal4')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal5')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal6')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal7')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal8')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal9')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal10')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal11')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal12')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal13')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal14')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal15')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime1')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime2')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime3')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime4')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime5')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime6')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime7')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime8')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime9')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime10')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime11')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime12')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime13')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime14')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime15'))
        );
        
        SET i = i + 1; -- Avanzar al siguiente registro
    END WHILE;
    
    CALL sp_InsertarLog(p_userid, v_empresaid, 'Subir Archivo', concat('El usuario Id ', p_userid, ' subio el archivo ', nombre_archivo, '.'));
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insertWidget`(
    IN p_Url varchar(255),
    IN p_Nombre varchar(255),
    IN p_UserId INT
)
BEGIN
    DECLARE v_empresaId INT;
    DECLARE v_exists INT DEFAULT 0;

    -- Verificar si ya existe un widget con misma url y nombre
    SELECT COUNT(*) INTO v_exists
    FROM widgets
   WHERE url COLLATE utf8mb4_unicode_ci = p_Url COLLATE utf8mb4_unicode_ci
	AND nombre COLLATE utf8mb4_unicode_ci = p_Nombre COLLATE utf8mb4_unicode_ci;

    -- Si ya existe, retornar false
    IF v_exists > 0 THEN
        SELECT FALSE AS resultado;
    ELSE
        -- Obtener empresa asociada al usuario
        SET v_empresaId = (
            SELECT EmpresaId
            FROM usuarios
            WHERE UsuarioId = p_UserId
            LIMIT 1
        );

        -- Insertar widget
        INSERT INTO proyectotitulo.widgets (
            url,
            nombre,
            UsuarioId,
            EmpresaId,
            fechaCreacion
        ) VALUES (
            p_Url,
            p_Nombre,
            p_UserId,
            v_empresaId,
            NOW()
        );

        -- Insertar log
        CALL sp_InsertarLog(
            p_UserId,
            v_empresaId,
            'Crear Widget',
            CONCAT('El usuario Id ', p_UserId, ' creó el Widget ', p_Nombre, '.')
        );

        -- Retornar true
        SELECT TRUE AS resultado;
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListarArchivosPorEmpresa`(IN p_userId int)
BEGIN
	declare v_empresaId int;
    
    set v_empresaId = (select EmpresaId from usuarios where usuarioId = p_userId limit 1);

	select 
	archivos.ArchivoId 
	,archivos.NombreOriginal 
	,archivos.RutaAlmacenamiento 
	,archivos.FechaSubida 
	,empresas.Nombre as NombreEmpresa
	,usuarios.Email as NombreUsuario 
	,categoriasarchivo.Nombre as NombreCategoria 
	from archivos
	inner join usuarios on usuarios.UsuarioId = archivos.UsuarioId
	inner join empresas on empresas.EmpresaId = archivos.EmpresaId
	inner join categoriasarchivo on categoriasarchivo.CategoriaId = archivos.CategoriaId
    where archivos.EmpresaId = v_empresaId;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListarCategoriasArchivos`(
   IN p_UserId int
)
BEGIN
	DECLARE EmpId int;
    
    set EmpId = (select EmpresaId from proyectotitulo.usuarios where UsuarioId = p_UserId limit 1);

    SELECT A.CategoriaId, A.Nombre, A.Descripcion, A.EmpresaId, B.Nombre as NombreEmpresa
    FROM categoriasarchivo A
    LEFT JOIN LATERAL (
		SELECT bb.Nombre FROM proyectotitulo.empresas bb
        WHERE (EmpId IS NULL OR bb.EmpresaId = EmpId)
        limit 1
    )B ON true
    WHERE (EmpId IS NULL OR A.EmpresaId = EmpId);
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListarEmpresas`(
    IN p_EmpresaId VARCHAR(100)
)
BEGIN
    SELECT * FROM Empresas
    WHERE (p_EmpresaId IS NULL OR EmpresaId = p_EmpresaId);
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListarRoles`(
    IN p_RolId VARCHAR(50)
)
BEGIN
    SELECT * FROM Roles
    WHERE (p_RolId IS NULL OR RolId = p_RolId);
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListarUsuarios`(
	IN p_UsuarioId INT,
    IN p_Nombre VARCHAR(100),
    IN p_Email VARCHAR(100),
    IN p_Activo BOOLEAN,
    IN p_EmpresaId INT
)
BEGIN
	if (p_UsuarioId IS NOT NULL) THEN
		SELECT U.*, E.Nombre AS EmpresaNombre, R.Nombre AS RolNombre
		FROM Usuarios U
		INNER JOIN Empresas E ON U.EmpresaId = E.EmpresaId
		INNER JOIN Roles R ON U.RolId = R.RolId
		WHERE U.UsuarioId = p_UsuarioId;
        
	ELSE
        SELECT U.*, E.Nombre AS EmpresaNombre, R.Nombre AS RolNombre
		FROM Usuarios U
		INNER JOIN Empresas E ON U.EmpresaId = E.EmpresaId
		INNER JOIN Roles R ON U.RolId = R.RolId
		WHERE
        (p_Nombre IS NULL OR U.Nombre LIKE CONCAT('%', p_Nombre, '%')) AND
        (p_Email IS NULL OR U.Email = p_Email) AND
        (p_Activo IS NULL OR U.Activo = p_Activo) AND
        (p_EmpresaId IS NULL OR U.EmpresaId = p_EmpresaId);
    END IF;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_ListarView`(
IN pview_name VARCHAR(100)
)
BEGIN
	-- Crear la vista dinámica
    SET @sql = CONCAT('select * from  vw_',pview_name);
    
    -- Ejecutar el SQL dinámico
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_LoginUsuario`(
    IN p_Email VARCHAR(255),
    OUT p_UsuarioId INT,
    OUT p_Nombre VARCHAR(255),
    OUT p_PasswordHash VARCHAR(255),
    OUT p_EmpresaNombre varchar(255),
    OUT p_RolNombre varchar(255),
    OUT p_Activo BOOL
)
BEGIN
    declare v_EmpresaId int;
    declare usuarioID int;
    declare empresaid int;

    SELECT 
        Usuarios.UsuarioId
        into 
        p_UsuarioId
    FROM Usuarios
    WHERE Email = p_Email
    AND  Usuarios.Activo = 1
    LIMIT 1;

    IF p_UsuarioId is not null then
         SELECT 
            Usuarios.UsuarioId,
            Usuarios.Nombre,
            Usuarios.PasswordHash,
            Empresas.Nombre,
            Roles.Nombre,
            Usuarios.Activo,
            Empresas.EmpresaId
        INTO 
            p_UsuarioId,
            p_Nombre,
            p_PasswordHash,
            p_EmpresaNombre,
            p_RolNombre,
            p_Activo,
            v_EmpresaId
        FROM Usuarios
        inner join Roles on Roles.RolId = Usuarios.RolId
        inner join Empresas on Empresas.EmpresaId = Usuarios.EmpresaId
        WHERE Email = p_Email
        AND  Usuarios.Activo = 1
        LIMIT 1;
         CALL sp_InsertarLog(p_UsuarioId, v_EmpresaId, 'Login Usuario', CONCAT('El usuario ', p_Email, ' inició sesión.'));
    ELSE

        set empresaId = (select EmpresaId from empresas where nombre = "AutoReport" limit 1);
        SET usuarioId = (select UsuarioId from usuarios where nombre = "Sistema AutoReport" limit 1);
        SET p_Nombre ="";
        SET p_PasswordHash ="";
        SET p_EmpresaNombre ="";
        SET p_RolNombre ="";
        SET p_Activo =FALSE;
        set p_UsuarioId = -1;
        set v_EmpresaId = -1;
        CALL sp_InsertarLog(usuarioId, empresaId, 'Login Usuario', concat('El usuario ', p_Email, ' intento iniciar sesión.'));
    END IF;

END$$
DELIMITER ;
