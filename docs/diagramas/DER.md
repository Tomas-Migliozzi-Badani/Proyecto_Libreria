# Diagrama entidad-relacion

![Diagrama entidad-relacion](DER.png)

```mermaid
erDiagram
    Cliente o|--o| Usuario : "vinculo opcional"
    Cliente ||--o{ Reserva : "realiza"
    Usuario ||--o{ Reserva : "registra"
    Categoria ||--o{ Producto : "clasifica"
    Reserva ||--o{ DetalleReserva : "contiene"
    Producto ||--o{ DetalleReserva : "aparece en"
    Cliente {
        int IdCliente PK
        string Nombre
        string Apellido
        string Dni UK
        string Telefono
        string Email
    }
    Usuario {
        int IdUsuario PK
        string Nombre
        string Apellido
        string Email UK
        string Password
        string Rol
        boolean Estado
        int VersionToken
        string Avatar
        int IdCliente FK,UK
    }
    Categoria {
        int IdCategoria PK
        string Nombre
        boolean Estado
    }
    Reserva {
        int IdReserva PK
        datetime FechaReserva
        datetime FechaVencimiento
        string EstadoReserva
        int IdCliente FK
        int IdUsuario FK
    }
    DetalleReserva {
        int IdDetalleReserva PK
        int Cantidad
        decimal PrecioUnitario
        int IdReserva FK
        int IdProducto FK
    }
    Producto {
        int IdProducto PK
        string Nombre
        string Marca
        decimal Precio
        int Stock
        string Imagen
        boolean Estado
        int IdCategoria FK
    }
```

PK: clave primaria. FK: clave foranea. UQ/UK: unica. Todas las relaciones se conservan con DeleteBehavior.Restrict.

Usuario.IdCliente puede ser null; un cliente puede tener cero o una cuenta vinculada. Las reservas y productos forman una relacion N:M a traves de DetalleReserva, que agrega cantidad y precio unitario historico.

El dibujo expresa las relaciones que admite la base. En la aplicacion se exige al menos un detalle al crear una reserva; el servicio rechaza productos repetidos en una misma reserva. Esa regla no se representa como restriccion de cardinalidad fisica en MySQL.
