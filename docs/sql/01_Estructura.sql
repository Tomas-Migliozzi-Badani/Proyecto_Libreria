CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
);

START TRANSACTION;
CREATE TABLE `Categorias` (
    `IdCategoria` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(100) NOT NULL,
    `Estado` tinyint(1) NOT NULL,
    PRIMARY KEY (`IdCategoria`)
);

CREATE TABLE `Clientes` (
    `IdCliente` int NOT NULL AUTO_INCREMENT,
    `Nombre` longtext NOT NULL,
    `Apellido` longtext NOT NULL,
    `Dni` longtext NOT NULL,
    `Telefono` longtext NOT NULL,
    `Email` longtext NOT NULL,
    PRIMARY KEY (`IdCliente`)
);

CREATE TABLE `Usuarios` (
    `IdUsuario` int NOT NULL AUTO_INCREMENT,
    `Nombre` longtext NOT NULL,
    `Apellido` longtext NOT NULL,
    `Email` varchar(254) NOT NULL,
    `Password` longtext NOT NULL,
    `Rol` varchar(30) NOT NULL,
    `Estado` tinyint(1) NOT NULL,
    `Avatar` longtext NOT NULL,
    PRIMARY KEY (`IdUsuario`)
);

CREATE TABLE `Productos` (
    `IdProducto` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(150) NOT NULL,
    `Marca` varchar(100) NOT NULL,
    `Precio` decimal(18,2) NOT NULL,
    `Stock` int NOT NULL,
    `Imagen` longtext NOT NULL,
    `Estado` tinyint(1) NOT NULL,
    `IdCategoria` int NOT NULL,
    PRIMARY KEY (`IdProducto`),
    CONSTRAINT `FK_Productos_Categorias_IdCategoria` FOREIGN KEY (`IdCategoria`) REFERENCES `Categorias` (`IdCategoria`) ON DELETE RESTRICT
);

CREATE TABLE `Reservas` (
    `IdReserva` int NOT NULL AUTO_INCREMENT,
    `FechaReserva` datetime(6) NOT NULL,
    `FechaVencimiento` datetime(6) NOT NULL,
    `EstadoReserva` varchar(30) NOT NULL,
    `IdCliente` int NOT NULL,
    `IdUsuario` int NOT NULL,
    PRIMARY KEY (`IdReserva`),
    CONSTRAINT `FK_Reservas_Clientes_IdCliente` FOREIGN KEY (`IdCliente`) REFERENCES `Clientes` (`IdCliente`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reservas_Usuarios_IdUsuario` FOREIGN KEY (`IdUsuario`) REFERENCES `Usuarios` (`IdUsuario`) ON DELETE RESTRICT
);

CREATE TABLE `DetallesReserva` (
    `IdDetalleReserva` int NOT NULL AUTO_INCREMENT,
    `Cantidad` int NOT NULL,
    `PrecioUnitario` decimal(18,2) NOT NULL,
    `IdReserva` int NOT NULL,
    `IdProducto` int NOT NULL,
    PRIMARY KEY (`IdDetalleReserva`),
    CONSTRAINT `FK_DetallesReserva_Productos_IdProducto` FOREIGN KEY (`IdProducto`) REFERENCES `Productos` (`IdProducto`) ON DELETE RESTRICT,
    CONSTRAINT `FK_DetallesReserva_Reservas_IdReserva` FOREIGN KEY (`IdReserva`) REFERENCES `Reservas` (`IdReserva`) ON DELETE RESTRICT
);

CREATE INDEX `IX_DetallesReserva_IdProducto` ON `DetallesReserva` (`IdProducto`);

CREATE INDEX `IX_DetallesReserva_IdReserva` ON `DetallesReserva` (`IdReserva`);

CREATE INDEX `IX_Productos_IdCategoria` ON `Productos` (`IdCategoria`);

CREATE INDEX `IX_Reservas_IdCliente` ON `Reservas` (`IdCliente`);

CREATE INDEX `IX_Reservas_IdUsuario` ON `Reservas` (`IdUsuario`);

CREATE UNIQUE INDEX `IX_Usuarios_Email` ON `Usuarios` (`Email`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261102115940_Inicial', '10.0.13');

ALTER TABLE `Usuarios` ADD `IdCliente` int NULL;

ALTER TABLE `Clientes` MODIFY `Dni` varchar(20) NOT NULL;

CREATE UNIQUE INDEX `IX_Usuarios_IdCliente` ON `Usuarios` (`IdCliente`);

CREATE UNIQUE INDEX `IX_Clientes_Dni` ON `Clientes` (`Dni`);

ALTER TABLE `Usuarios` ADD CONSTRAINT `FK_Usuarios_Clientes_IdCliente` FOREIGN KEY (`IdCliente`) REFERENCES `Clientes` (`IdCliente`) ON DELETE RESTRICT;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261102163613_VincularUsuarioCliente', '10.0.13');

ALTER TABLE `Usuarios` ADD `VersionToken` int NOT NULL DEFAULT 0;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261103143950_VersionarTokensJwt', '10.0.13');

COMMIT;

