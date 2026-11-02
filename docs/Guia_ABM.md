# Guia para explicar los ABM

## Recorrido de una operacion

1. La vista Razor entrega el HTML y el token de proteccion del formulario.
2. Vue mantiene los valores en pantalla. fetch envia JSON sin recargar la pagina.
3. El controlador recibe un ViewModel que permite editar solo los campos previstos.
4. Se validan los datos en el servidor. Los atributos del formulario ayudan al usuario, pero no reemplazan esa validacion.
5. EF Core consulta o actualiza MySQL. SaveChanges confirma los cambios de las entidades.
6. El controlador responde JSON y Vue actualiza el listado.

## Responsabilidad de cada carpeta

- Models: entidades que se guardan en tablas.
- ViewModels: datos aceptados por cada formulario; no son tablas.
- Controllers: reciben solicitudes y devuelven vistas o resultados JSON.
- Services: reglas que abarcan varias entidades. ReservasService coordina reserva, detalles y stock.
- Views: HTML Razor que se ve en el navegador.
- wwwroot/js: comportamiento de Vue y llamadas AJAX.
- Data: contexto y configuracion del modelo relacional.
- Migrations: historial de cambios del esquema.
- bin y obj: archivos generados por .NET, no codigo escrito por el estudiante.

## Ejemplo de reserva

Un cliente reserva 3 cuadernos y 2 lapiceras. Se crea UNA reserva y DOS detalles, con cantidades 3 y 2. El stock se descuenta por esas cantidades. El precio se guarda en cada detalle para conservar el valor acordado. Si falla una linea, la transaccion revierte toda la operacion. Al cancelar se devuelve el stock y se conserva la reserva como Cancelada.

## Baja fisica y baja logica

Cliente sin reservas: puede eliminarse fisicamente. Cliente con reservas: se rechaza para conservar el historial. Categoria, producto y usuario: se desactivan mediante Estado. Reserva: se cancela, manteniendo sus detalles.

## Paginado y busqueda asincrona

Skip y Take limitan la consulta en MySQL. Solo se reciben las diez filas de la pagina solicitada. Las relaciones de la reserva se buscan por texto y llegan en grupos de hasta diez coincidencias, sin cargar todo el catalogo.

## Contrasenas

PasswordHasher genera un hash con sal; no se guarda el texto original ni se devuelve el hash al navegador. El login usa VerifyHashedPassword para comparar la clave ingresada con el hash guardado.

## Login: autenticacion y autorizacion

Autenticacion responde quien es la persona: CuentaController valida email, estado y hash. SignInAsync crea una cookie cifrada con claims de ID, nombre, email y rol. UseAuthentication interpreta esa cookie antes de que UseAuthorization compruebe los atributos Authorize.

Autorizacion responde que puede hacer: Administrador gestiona Usuarios; Administrador y Empleado gestionan los otros ABM. Cliente consulta su perfil, usa el catalogo y administra sus propias reservas. La comprobacion se hace en el servidor, incluso si alguien escribe una URL o envia AJAX manualmente.

La reserva usa el ID de la identidad autenticada al darla de alta, sea cliente u operador. No confia en un usuario enviado por el navegador. El cierre de sesion elimina la cookie y exige POST con un token antiforgery. Cambiar rol/password o desactivar el usuario invalida la sesion en la siguiente peticion.

JWT se incorporara despues para la API; este login MVC usa cookies, como en los apuntes de autenticacion.
