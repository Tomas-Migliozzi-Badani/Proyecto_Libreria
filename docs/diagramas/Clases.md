# Diagrama de clases del modelo

![Diagrama de clases](Clases.png)

```mermaid
classDiagram
    class Cliente {
        +int IdCliente
        +string Nombre
        +string Apellido
        +string Dni
        +string Telefono
        +string Email
    }
    class Usuario {
        +int IdUsuario
        +string Nombre
        +string Apellido
        +string Email
        +string Password
        +string Rol
        +bool Estado
        +int VersionToken
        +string Avatar
        +int? IdCliente
        +Cliente? Cliente
    }
    class Categoria {
        +int IdCategoria
        +string Nombre
        +bool Estado
    }
    class Reserva {
        +int IdReserva
        +DateTime FechaReserva
        +DateTime FechaVencimiento
        +string EstadoReserva
        +int IdCliente
        +int IdUsuario
        +Cliente Cliente
        +Usuario Usuario
    }
    class DetalleReserva {
        +int IdDetalleReserva
        +int Cantidad
        +decimal PrecioUnitario
        +int IdReserva
        +int IdProducto
        +Reserva Reserva
        +Producto Producto
    }
    class Producto {
        +int IdProducto
        +string Nombre
        +string Marca
        +decimal Precio
        +int Stock
        +string Imagen
        +bool Estado
        +int IdCategoria
        +Categoria Categoria
    }
    Cliente "0..1" <-- "0..1" Usuario : Cliente opcional
    Cliente "1" <-- "0..*" Reserva : Cliente
    Usuario "1" <-- "0..*" Reserva : autor
    Categoria "1" <-- "0..*" Producto : Categoria
    Reserva "1" <-- "0..*" DetalleReserva : Reserva
    Producto "1" <-- "0..*" DetalleReserva : Producto
```

Se representan las seis clases persistentes de Models, con sus propiedades y navegaciones. No se inventan colecciones de navegacion que no existen en el codigo. Cliente? en Usuario es opcional; las restantes navegaciones estan configuradas como obligatorias.

LibreriaContext hereda de DbContext y expone los seis DbSet. ReservasService opera sobre Reserva/DetalleReserva/Producto; ImagenesService administra los archivos de Avatar e Imagen. Los controladores coordinan entradas y respuestas, sin ser entidades persistentes.
