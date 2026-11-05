-- Libreria: instalacion completa de demostracion para MySQL 8.
-- Ejecutar UNA vez en una base nueva. No borra ni sobrescribe bases existentes.
-- Si Libreria_Demo ya tiene tablas, elegir otro nombre en CREATE DATABASE y USE.
CREATE DATABASE IF NOT EXISTS `Libreria_Demo` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `Libreria_Demo`;
SET NAMES utf8mb4;

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


-- Solo para una base NUEVA con la estructura de 01_Estructura.sql.
-- Datos ficticios de evaluacion; no exporta informacion de la base del estudiante.
-- Cuentas: administrador@libreria.test / 123; empleado@test / 123; cliente@libreria.test / Libreria2026!
SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO `Categorias` (`IdCategoria`, `Nombre`, `Estado`) VALUES
(1, 'Cuadernos y papel', 1),
(2, 'Escritura', 1),
(3, 'Arte y dibujo', 1),
(4, 'Organizacion', 1);
INSERT INTO `Clientes` (`IdCliente`, `Nombre`, `Apellido`, `Dni`, `Telefono`, `Email`) VALUES
(1, 'Cliente', 'Demostracion', '99999991', '0000000000', 'cliente@libreria.test'),
(2, 'Persona', 'Demo 02', '99999992', '0000000000', 'persona02@example.test'),
(3, 'Persona', 'Demo 03', '99999993', '0000000000', 'persona03@example.test'),
(4, 'Persona', 'Demo 04', '99999994', '0000000000', 'persona04@example.test'),
(5, 'Persona', 'Demo 05', '99999995', '0000000000', 'persona05@example.test'),
(6, 'Persona', 'Demo 06', '99999996', '0000000000', 'persona06@example.test'),
(7, 'Persona', 'Demo 07', '99999997', '0000000000', 'persona07@example.test'),
(8, 'Persona', 'Demo 08', '99999998', '0000000000', 'persona08@example.test'),
(9, 'Persona', 'Demo 09', '99999999', '0000000000', 'persona09@example.test'),
(10, 'Persona', 'Demo 10', '100000000', '0000000000', 'persona10@example.test'),
(11, 'Persona', 'Demo 11', '100000001', '0000000000', 'persona11@example.test'),
(12, 'Persona', 'Demo 12', '100000002', '0000000000', 'persona12@example.test');
INSERT INTO `Usuarios` (`IdUsuario`, `Nombre`, `Apellido`, `Email`, `Password`, `Rol`, `Estado`, `Avatar`, `IdCliente`, `VersionToken`) VALUES
(1, 'Administrador', 'Demostracion', 'administrador@libreria.test', 'AQAAAAIAAYagAAAAEF+0YbXdFwWTVg390O85croN6mNpqsFY3+oWaIr9e41bVWWdnQr4KBeN2JtuPJYpaQ==', 'Administrador', 1, '', NULL, 0),
(2, 'Empleado', 'Demostracion', 'empleado@test', 'AQAAAAIAAYagAAAAEEy3cgZjMTJS1vAtwrVSWuEVbIOWqY2ODdbC+BHJYqJtbacFRiYA9vNZfUv11XeevQ==', 'Empleado', 1, '', NULL, 0),
(3, 'Cliente', 'Demostracion', 'cliente@libreria.test', 'AQAAAAIAAYagAAAAEFDswBcQPMQe0XqDNiB+9urGBttPEa4p+e2wX0mv03ickVDod9TrIDr8xXz9UWmwMg==', 'Cliente', 1, '', 1, 0);
INSERT INTO `Productos` (`IdProducto`, `Nombre`, `Marca`, `Precio`, `Stock`, `Imagen`, `Estado`, `IdCategoria`) VALUES
(1, 'Cuaderno A4 rayado', 'Escolar', 3500, 18, '', 1, 1),
(2, 'Cuaderno A5 cuadriculado', 'Escolar', 2200, 19, '', 1, 1),
(3, 'Resma A4 80 gramos', 'Papel', 6500, 17, '', 1, 1),
(4, 'Block de notas', 'Papel', 1500, 19, '', 1, 1),
(5, 'Lapicera azul', 'Escritura', 700, 29, '', 1, 2),
(6, 'Lapicera negra', 'Escritura', 700, 30, '', 1, 2),
(7, 'Lapiz negro HB', 'Escritura', 400, 40, '', 1, 2),
(8, 'Marcador permanente', 'Escritura', 1200, 20, '', 1, 2),
(9, 'Caja de 12 lapices de colores', 'Arte', 4200, 15, '', 1, 3),
(10, 'Acuarelas escolares', 'Arte', 3600, 12, '', 1, 3),
(11, 'Pincel numero 6', 'Arte', 1100, 25, '', 1, 3),
(12, 'Block de dibujo A4', 'Arte', 2800, 20, '', 1, 3),
(13, 'Carpeta A4 con elastico', 'Orden', 1800, 25, '', 1, 4),
(14, 'Separadores de colores', 'Orden', 1600, 20, '', 1, 4),
(15, 'Cartuchera simple', 'Orden', 3200, 15, '', 1, 4),
(16, 'Agenda escolar', 'Orden', 4800, 10, '', 1, 4);

-- La reserva 1 tiene 5 productos. Su stock ya esta descontado en los valores anteriores.
INSERT INTO `Reservas` (`IdReserva`,`FechaReserva`,`FechaVencimiento`,`EstadoReserva`,`IdCliente`,`IdUsuario`) VALUES
(1,NOW(),DATE_ADD(NOW(),INTERVAL 2 DAY),'Pendiente',1,3),
(2,DATE_SUB(NOW(),INTERVAL 2 DAY),DATE_SUB(NOW(),INTERVAL 1 DAY),'Cancelada',2,2),
(3,DATE_SUB(NOW(),INTERVAL 3 DAY),DATE_SUB(NOW(),INTERVAL 2 DAY),'Cancelada',3,2),
(4,DATE_SUB(NOW(),INTERVAL 4 DAY),DATE_SUB(NOW(),INTERVAL 3 DAY),'Cancelada',4,2),
(5,DATE_SUB(NOW(),INTERVAL 5 DAY),DATE_SUB(NOW(),INTERVAL 4 DAY),'Cancelada',5,2),
(6,DATE_SUB(NOW(),INTERVAL 6 DAY),DATE_SUB(NOW(),INTERVAL 5 DAY),'Cancelada',6,2),
(7,DATE_SUB(NOW(),INTERVAL 7 DAY),DATE_SUB(NOW(),INTERVAL 6 DAY),'Cancelada',7,2),
(8,DATE_SUB(NOW(),INTERVAL 8 DAY),DATE_SUB(NOW(),INTERVAL 7 DAY),'Cancelada',8,2),
(9,DATE_SUB(NOW(),INTERVAL 9 DAY),DATE_SUB(NOW(),INTERVAL 8 DAY),'Cancelada',9,2),
(10,DATE_SUB(NOW(),INTERVAL 10 DAY),DATE_SUB(NOW(),INTERVAL 9 DAY),'Cancelada',10,2),
(11,DATE_SUB(NOW(),INTERVAL 11 DAY),DATE_SUB(NOW(),INTERVAL 10 DAY),'Cancelada',11,2),
(12,DATE_SUB(NOW(),INTERVAL 12 DAY),DATE_SUB(NOW(),INTERVAL 11 DAY),'Cancelada',12,2);
INSERT INTO `DetallesReserva` (`IdDetalleReserva`, `Cantidad`, `PrecioUnitario`, `IdReserva`, `IdProducto`) VALUES
(1, 2, 3500, 1, 1),
(2, 1, 2200, 1, 2),
(3, 3, 6500, 1, 3),
(4, 1, 1500, 1, 4),
(5, 1, 700, 1, 5),
(6, 1, 700, 2, 6),
(7, 1, 400, 3, 7),
(8, 1, 1200, 4, 8),
(9, 1, 4200, 5, 9),
(10, 1, 3600, 6, 10),
(11, 1, 1100, 7, 11),
(12, 1, 2800, 8, 12),
(13, 1, 1800, 9, 13),
(14, 1, 1600, 10, 14),
(15, 1, 3200, 11, 15),
(16, 1, 4800, 12, 16);

COMMIT;
-- No ejecutar nuevamente sobre la misma base: los IDs son de demostracion.
