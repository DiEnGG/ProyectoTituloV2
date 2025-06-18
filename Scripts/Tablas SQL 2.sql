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
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

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
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

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
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

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
  PRIMARY KEY (`DatoId`),
  KEY `ArchivoId` (`ArchivoId`),
  CONSTRAINT `datos_ibfk_1` FOREIGN KEY (`ArchivoId`) REFERENCES `archivos` (`ArchivoId`)
) ENGINE=InnoDB AUTO_INCREMENT=30 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


CREATE TABLE `empresas` (
  `EmpresaId` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `RazonSocial` varchar(150) DEFAULT NULL,
  `FechaRegistro` datetime DEFAULT CURRENT_TIMESTAMP,
  `Activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`EmpresaId`),
  UNIQUE KEY `Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE `roles` (
  `RolId` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) NOT NULL,
  `Activo` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`RolId`),
  UNIQUE KEY `Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;