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
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_generateCsvView`(
IN fileNameParam VARCHAR(100)
)
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE csvColumn VARCHAR(100);
    DECLARE auxColumn VARCHAR(100);
    DECLARE columnList TEXT DEFAULT '';


    DECLARE cur CURSOR FOR 
    SELECT CsvColumnName, AuxColumnName 
    FROM CsvMapping 
    WHERE FileName = fileNameParam;

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    OPEN cur;

    column_loop: LOOP
        FETCH cur INTO csvColumn, auxColumn;
        IF done THEN
            LEAVE column_loop;
        END IF;

        -- Concatenar cada columna al query dinámico
        SET columnList = CONCAT(columnList, 
            IF(LENGTH(columnList) = 0, '', ', '), 
            auxColumn, ' AS `', csvColumn, '`');
    END LOOP;

    CLOSE cur;

    -- Crear la vista dinámica
    SET @sql = CONCAT('CREATE OR REPLACE VIEW vw_',SUBSTRING_INDEX(fileNameParam, '.', 1),' AS SELECT ', columnList, ' FROM CsvData WHERE FileName = ''', fileNameParam, ''';');

    -- Ejecutar el SQL dinámico
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
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
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetUploadedFiles`()
BEGIN
    SELECT FileName, UploadDate FROM UploadedFiles ORDER BY UploadDate DESC;
END$$
DELIMITER ;
DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insertFileCategory`(IN userId int, IN filename VARCHAR(255),IN fileDesc VARCHAR(255),IN fileDelimiter VARCHAR(10), IN data JSON)
BEGIN
    DECLARE newCategoriaId INT;
    DECLARE i INT DEFAULT 0;
    DECLARE total INT;
    DECLARE empresaId INT;
    
    SET empresaId = (SELECT EmpresaId from usuarios where UsuarioId = userId limit 1);
    
	INSERT INTO categoriasarchivo (Nombre, Descripcion, EmpresaId, UsuarioId, FechaCreacion, Delimiter)
	VALUES (filename, fileDesc, empresaId, userId, now(), fileDelimiter);
    
    -- Capturar el ID del registro insertado
	SET newCategoriaId = LAST_INSERT_ID();
    
    -- Obtén la cantidad de registros en el JSON
    SET total = JSON_LENGTH(data);

    -- Bucle para recorrer cada registro del JSON
    WHILE i < total DO
        
		INSERT INTO archivosmapping
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
    
END$$
DELIMITER ;
DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insertFileData`(IN p_categoryId int, IN data JSON)
BEGIN
    DECLARE i INT DEFAULT 0;
    DECLARE total INT;
    DECLARE nombre_archivo varchar(255);
    DECLARE newArchivoId INT;
    
    -- Obtén la cantidad de registros en el JSON
    SET total = JSON_LENGTH(data);
	
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
        1,
        1,
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
            AuxDecimal1,
            AuxDecimal2,
            AuxDecimal3,
            AuxDecimal4,
            AuxDecimal5,
            AuxDateTime1,
            AuxDateTime2,
            AuxDateTime3,
            AuxDateTime4,
            AuxDateTime5
        )
        VALUES (
            newArchivoId,
            JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString1'))),
            JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString2'))),
            JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString3'))),
            JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString4'))),
            JSON_UNQUOTE(JSON_EXTRACT(data, CONCAT('$[', i, '].AuxString5'))),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal1')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal2')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal3')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal4')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDecimal5')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime1')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime2')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime3')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime4')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDateTime5'))
        );
        
        SET i = i + 1; -- Avanzar al siguiente registro
    END WHILE;
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
    OUT p_EmpresaId INT,
    OUT p_RolId INT,
    OUT p_Activo BOOL
)
BEGIN
    SELECT 
        UsuarioId,
        Nombre,
        PasswordHash,
        EmpresaId,
        RolId,
        Activo
    INTO 
        p_UsuarioId,
        p_Nombre,
        p_PasswordHash,
        p_EmpresaId,
        p_RolId,
        p_Activo
    FROM Usuarios
    WHERE Email = p_Email
    LIMIT 1;
END$$
DELIMITER ;
DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_SaveRecord`(IN Name VARCHAR(100), IN Age INT)
BEGIN
    INSERT INTO Records (Name, Age)
    VALUES (Name, Age);
END$$
DELIMITER ;
DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_SaveUploadedFile`(IN FileName VARCHAR(255), IN UploadDate DATETIME)
BEGIN
    INSERT INTO UploadedFiles (FileName, UploadDate)
    VALUES (FileName, UploadDate);
END$$
DELIMITER ;
