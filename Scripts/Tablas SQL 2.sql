CREATE TABLE `archivos` (
  `ArchivoId` int NOT NULL AUTO_INCREMENT,
  `NombreOriginal` varchar(255) DEFAULT NULL,
  `RutaAlmacenamiento` varchar(500) DEFAULT NULL,
  `FechaSubida` datetime DEFAULT CURRENT_TIMESTAMP,
  `UsuarioId` int NOT NULL,
  `EmpresaId` int NOT NULL,
  `CategoriaId` int NOT NULL,
  PRIMARY KEY (`ArchivoId`),
  KEY `UsuarioId` (`UsuarioId`),
  KEY `EmpresaId` (`EmpresaId`),
  KEY `CategoriaId` (`CategoriaId`),
  CONSTRAINT `archivos_ibfk_1` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`UsuarioId`),
  CONSTRAINT `archivos_ibfk_2` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`EmpresaId`),
  CONSTRAINT `archivos_ibfk_3` FOREIGN KEY (`CategoriaId`) REFERENCES `categoriasarchivo` (`CategoriaId`)
) ENGINE=InnoDB AUTO_INCREMENT=54 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `archivosmapping` (
  `MappingId` int NOT NULL AUTO_INCREMENT,
  `CategoriaId` int NOT NULL,
  `CsvColumnName` varchar(100) DEFAULT NULL,
  `AuxColumnName` varchar(100) DEFAULT NULL,
  `IndexCsvColumn` int DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MappingId`),
  KEY `CategoriaId` (`CategoriaId`),
  CONSTRAINT `archivosmapping_ibfk_1` FOREIGN KEY (`CategoriaId`) REFERENCES `categoriasarchivo` (`CategoriaId`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `categoriasarchivo` (
  `CategoriaId` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Descripcion` text,
  `EmpresaId` int NOT NULL,
  `UsuarioId` int NOT NULL,
  `FechaCreacion` datetime DEFAULT CURRENT_TIMESTAMP,
  `delimiter` varchar(5) DEFAULT NULL,
  PRIMARY KEY (`CategoriaId`),
  UNIQUE KEY `Nombre` (`Nombre`,`EmpresaId`),
  KEY `EmpresaId` (`EmpresaId`),
  KEY `UsuarioId` (`UsuarioId`),
  CONSTRAINT `categoriasarchivo_ibfk_1` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`EmpresaId`),
  CONSTRAINT `categoriasarchivo_ibfk_2` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`UsuarioId`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `datos` (
  `DatoId` int NOT NULL AUTO_INCREMENT,
  `ArchivoId` int NOT NULL,
  `AuxString1` text,
  `AuxString2` text,
  `AuxString3` text,
  `AuxString4` text,
  `AuxString5` text,
  `AuxDecimal1` decimal(18,4) DEFAULT NULL,
  `AuxDecimal2` decimal(18,4) DEFAULT NULL,
  `AuxDecimal3` decimal(18,4) DEFAULT NULL,
  `AuxDecimal4` decimal(18,4) DEFAULT NULL,
  `AuxDecimal5` decimal(18,4) DEFAULT NULL,
  `AuxDateTime1` datetime DEFAULT NULL,
  `AuxDateTime2` datetime DEFAULT NULL,
  `AuxDateTime3` datetime DEFAULT NULL,
  `AuxDateTime4` datetime DEFAULT NULL,
  `AuxDateTime5` datetime DEFAULT NULL,
  `AuxString6` text,
  `AuxString7` text,
  `AuxString8` text,
  `AuxString9` text,
  `AuxString10` text,
  `AuxString11` text,
  `AuxString12` text,
  `AuxString13` text,
  `AuxString14` text,
  `AuxString15` text,
  `AuxDecimal6` text,
  `AuxDecimal7` text,
  `AuxDecimal8` text,
  `AuxDecimal9` text,
  `AuxDecimal10` text,
  `AuxDecimal11` text,
  `AuxDecimal12` text,
  `AuxDecimal13` text,
  `AuxDecimal14` text,
  `AuxDecimal15` text,
  `AuxDateTime6` text,
  `AuxDateTime7` text,
  `AuxDateTime8` text,
  `AuxDateTime9` text,
  `AuxDateTime10` text,
  `AuxDateTime11` text,
  `AuxDateTime12` text,
  `AuxDateTime13` text,
  `AuxDateTime14` text,
  `AuxDateTime15` text,
  PRIMARY KEY (`DatoId`),
  KEY `ArchivoId` (`ArchivoId`),
  CONSTRAINT `datos_ibfk_1` FOREIGN KEY (`ArchivoId`) REFERENCES `archivos` (`ArchivoId`)
) ENGINE=InnoDB AUTO_INCREMENT=3324 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `empresas` (
  `EmpresaId` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `RazonSocial` varchar(150) DEFAULT NULL,
  `FechaRegistro` datetime DEFAULT CURRENT_TIMESTAMP,
  `Activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`EmpresaId`),
  UNIQUE KEY `Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `logs` (
  `LogId` int NOT NULL AUTO_INCREMENT,
  `UsuarioId` int NOT NULL,
  `EmpresaId` int NOT NULL,
  `Accion` varchar(100) DEFAULT NULL,
  `Detalle` text,
  `Fecha` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LogId`),
  KEY `UsuarioId` (`UsuarioId`),
  KEY `EmpresaId` (`EmpresaId`),
  CONSTRAINT `logs_ibfk_1` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`UsuarioId`),
  CONSTRAINT `logs_ibfk_2` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`EmpresaId`)
) ENGINE=InnoDB AUTO_INCREMENT=70 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `roles` (
  `RolId` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) NOT NULL,
  `Activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`RolId`),
  UNIQUE KEY `Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `usuarios` (
  `UsuarioId` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `EmpresaId` int NOT NULL,
  `RolId` int NOT NULL,
  `Activo` tinyint(1) DEFAULT '1',
  `FechaRegistro` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`UsuarioId`),
  UNIQUE KEY `Email` (`Email`),
  KEY `EmpresaId` (`EmpresaId`),
  KEY `RolId` (`RolId`),
  CONSTRAINT `usuarios_ibfk_1` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`EmpresaId`),
  CONSTRAINT `usuarios_ibfk_2` FOREIGN KEY (`RolId`) REFERENCES `roles` (`RolId`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `widgets` (
  `id` int NOT NULL AUTO_INCREMENT,
  `url` varchar(2083) COLLATE utf8mb4_unicode_ci NOT NULL,
  `nombre` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `UsuarioId` int NOT NULL,
  `EmpresaId` int NOT NULL,
  `fechaCreacion` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_usuario` (`UsuarioId`),
  KEY `idx_empresa` (`EmpresaId`),
  CONSTRAINT `fk_widget_empresa` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`EmpresaId`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_widget_usuario` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`UsuarioId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
