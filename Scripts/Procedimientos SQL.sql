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
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insertFileCategory`(IN filename VARCHAR(255), IN data JSON)
BEGIN
    DECLARE newCategoriaId INT;
    DECLARE i INT DEFAULT 0;
    DECLARE total INT;
    
    
    INSERT INTO categoriasarchivo (Nombre, Descripcion, EmpresaId, UsuarioId, FechaCreacion)
	VALUES (filename, '', 1,1,now());
    
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
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_insertFileData`(IN filename VARCHAR(255), IN data JSON)
BEGIN
    DECLARE i INT DEFAULT 0;
    DECLARE total INT;
	DECLARE id_category INT;
    DECLARE newArchivoId INT;
    
    -- Obtén la cantidad de registros en el JSON
    SET total = JSON_LENGTH(data);
	
    SET id_category = (select CategoriaId from categoriasarchivo where Nombre = filename limit 1);
    
    INSERT INTO archivos
    (
		NombreOriginal,
        FechaSubida,
        UsuarioId,
        EmpresaId,
        CategoriaId
    )
    VALUES 
    (	filename,
		NOW(),
        1,
        1,
		id_category
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
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDate1')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDate2')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDate3')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDate4')),
            JSON_EXTRACT(data, CONCAT('$[', i, '].AuxDate5'))
        );
        
        SET i = i + 1; -- Avanzar al siguiente registro
    END WHILE;
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
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_getFileMapping`(IN fileName varchar(255))
BEGIN
	
    DECLARE id_categoria INT;
    set id_categoria = (select CategoriaId  from categoriasarchivo where Nombre = fileName limit 1);

	select CategoriaId, CsvColumnName, AuxColumnName, IndexCsvColumn, CreatedAt from ArchivosMapping
    where CategoriaId = id_categoria
    group by CategoriaId, CsvColumnName, AuxColumnName, IndexCsvColumn, CreatedAt
    having CreatedAt = max(CreatedAt);
END$$
DELIMITER ;

DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_GetUploadedFiles`()
BEGIN
    SELECT FileName, UploadDate FROM UploadedFiles ORDER BY UploadDate DESC;
END$$
DELIMITER ;
