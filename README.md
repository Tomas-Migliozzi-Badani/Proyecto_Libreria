# Libreria - Proyecto integrador MVC

Sistema de gestion de productos, clientes y reservas. ASP.NET Core MVC (.NET 10), EF Core, MySQL y Vue 3 con AJAX. El codigo se organiza para separar entidades, formularios, controladores y reglas de negocio.

## Ejecutar en otro equipo

Requisitos: SDK .NET 10 y servidor MySQL 8.0 o superior iniciado (Workbench por si solo no reemplaza al servidor).

Desde la carpeta que contiene `Proyecto_Libreria.csproj`:

1. Ejecutar `dotnet restore` y `dotnet tool restore`.
2. Copiar `appsettings.Local.example.json` a `appsettings.Local.json`.
3. Editar la conexion del archivo local con servidor, puerto, usuario y password propios. Configurar tambien TokenAuthentication:SecretKey con una clave aleatoria de al menos 32 bytes; ver docs/api/Guia_API_JWT.md. Usar una base nueva dedicada, sugerida: `Libreria`. Este archivo esta excluido de Git.
4. Ejecutar `dotnet ef database update`. Se requieren permisos para crear la base y tablas, o una base vacia creada previamente.
5. Ejecutar `dotnet run` y abrir la direccion indicada en la terminal.
6. Entrar en Iniciar sesion. En Development se crean las cuentas de demostracion indicadas abajo.

Detener con Ctrl+C antes de recompilar o ejecutar otra instancia. Una instancia abierta puede bloquear el DLL en bin. Si estas en la carpeta superior, usar `dotnet run --project Proyecto_Libreria/Proyecto_Libreria.csproj`.

La variable `ConnectionStrings__Libreria` tiene prioridad sobre el archivo local. Las migraciones describen las tablas, no contienen los datos de otra computadora. No usar EnsureCreated junto con migraciones.

## Cuentas de demostracion (solo desarrollo)

| Rol | Email | Contrasena |
|---|---|---|
| Administrador | administrador@libreria.test | 123 |
| Empleado | empleado@test | 123 |
| Cliente | cliente@libreria.test | Libreria2026! |

Son cuentas publicas de prueba para la evaluacion, no credenciales de produccion. Se crean solo si el entorno es Development y `DatosPrueba:Habilitar` es true (configurado en appsettings.Development.json). En cada inicio solo se agregan cuentas que no existen: nunca se cambian passwords ni se reactivan cuentas existentes. Deshabilitar esa opcion fuera del uso de demostracion. En Production no se ejecuta la creacion aunque la opcion sea true.

## Portal de clientes

El visitante puede abrir `/Catalogo` sin iniciar sesion. Para reservar usa `/Cuenta/Registro`: nombre, apellido, DNI, telefono, email y password de al menos ocho caracteres con confirmacion. El servidor crea Cliente y Usuario en una transaccion y asigna siempre rol Cliente. Ningun dato enviado por el navegador puede otorgar rol de empleado/administrador.

`Usuario.IdCliente` es una relacion opcional y unica: las cuentas de clientes se vinculan a una persona, mientras que el personal no necesita ese vinculo. La migracion `VincularUsuarioCliente` agrega esa relacion y un indice unico por DNI. Se verificaron los datos existentes antes de aplicarla.

Si ya existe el DNI o el email, el registro se rechaza. No se apropia ni vincula automaticamente un cliente existente. Para cuentas antiguas, el administrador selecciona expresamente el cliente desde Usuarios mediante una busqueda por AJAX. Un cliente no puede estar vinculado a dos cuentas. La cuenta conocida de demostracion `cliente@libreria.test` fue asociada expresamente a sus datos demo; ninguna otra cuenta previa fue vinculada automaticamente.

El catalogo publica solo productos y categorias activos, con busqueda y nueve productos por pagina desde MySQL. La seleccion se conserva en sessionStorage de la pestaña durante el login; solo contiene informacion publica de productos, no credenciales ni datos personales.

La confirmacion toma el cliente y el autor desde la identidad autenticada y el precio desde MySQL. El navegador envia solo IDs de productos y cantidades. Se asigna un vencimiento de 48 horas. No hay pagos ni envios. El vencimiento es informativo: por ahora no hay una tarea automatica que cancele al vencer; el cliente o el personal cancelan las reservas pendientes.

`/MisReservas` muestra solo registros del cliente vinculado a la sesion, con paginado, detalle y cancelacion. Las reservas que el personal registra para ese cliente tambien aparecen. Consultar o cancelar una reserva ajena devuelve 404; enviar otro IdCliente o IdUsuario no permite cambiar el propietario. Un cliente cancela pendientes pero no modifica cantidades ya confirmadas: puede cancelar y crear una nueva seleccion.

## Roles y sesion

| Funcionalidad | Administrador | Empleado | Cliente |
|---|---|---|---|
| Categorias, Productos, Clientes y Reservas | Si | Si | No |
| Gestion de Usuarios | Si | No | No |
| Perfil propio y cierre de sesion | Si | Si | Si |
| Catalogo publico | Si | Si | Si (tambien visitantes) |
| Confirmar reserva desde catalogo / Mis reservas | No (usa administracion) | No (usa administracion) | Si, con cliente vinculado |

- Los permisos se verifican en el servidor con Authorize; ocultar el menu no reemplaza la autorizacion.
- El login verifica el hash con PasswordHasher y genera una cookie HttpOnly de ocho horas con renovacion por actividad. Recordarme permite persistirla al cerrar el navegador.
- Las cookies se validan contra MySQL en cada peticion. Desactivar una cuenta o cambiar su rol, email o hash de password invalida sus sesiones anteriores.
- El logout es POST con antiforgery. Una redireccion posterior al login solo admite direcciones locales.
- El login admite diez intentos por minuto y direccion IP. Ante 429 esperar un minuto.
- Las llamadas AJAX reciben 401/403; una sesion vencida redirige la pantalla al login.
- En produccion se exige cookie Secure (HTTPS); en Development acompana el protocolo usado para permitir pruebas HTTP locales.
- El administrador no puede desactivarse ni quitarse su propio rol.
- El Cliente dispone de catalogo, perfil y Mis reservas. No accede a los ABM internos.

## ABM implementados

- `/Categorias`: alta, edicion, busqueda, baja logica y reactivacion.
- `/Productos`: nombre, marca, precio positivo, stock entero no negativo y categoria. Busqueda de categoria por AJAX, con hasta diez coincidencias activas. Se puede conservar una categoria historica inactiva al editar. Permite cargar, reemplazar y quitar una imagen JPG/PNG mediante AJAX.
- `/Clientes`: datos personales y contacto; DNI de 7 a 9 digitos sin puntos. Rechaza DNI repetido. Un cliente con reservas no puede eliminarse.
- `/Usuarios`: nombre, email unico, password, rol y estado. En edicion, password vacio conserva el anterior. El hash nunca se devuelve al navegador.
- `/Reservas`: alta, consulta, modificacion de pendientes y cancelacion. Un producto por detalle, con cantidad y precio unitario. Clientes y productos se buscan por AJAX sin descargar todo el catalogo.

Todos los listados principales consultan diez filas por pagina desde MySQL mediante Skip/Take y orden estable. Vue y fetch actualizan la pantalla sin recargar. Vue se incluye localmente con licencia MIT.

### Reservas y stock

ReservasService agrupa reserva, detalles y stock en una transaccion: se confirman o revierten juntos. Bloquea las filas afectadas y descuenta stock de forma condicional en SQL para impedir sobreventa.

El autor de una nueva reserva se obtiene de la sesion: puede ser un operador interno o el propio cliente. IdCliente identifica al comprador e IdUsuario al autor. Cualquier IdUsuario enviado por el navegador se ignora. Una edicion interna conserva el autor original. El precio viene del servidor y se conserva en las lineas existentes. El total se calcula desde los detalles guardados.

Cancelar conserva el historial y devuelve el stock una sola vez. Una reserva cancelada queda en modo consulta. Todavia no hay retiro/venta ni vencimiento automatico: una reserva pendiente mantiene el stock comprometido hasta cancelarla.

## Estructura para la defensa

- Models/: entidades persistentes.
- ViewModels/: campos permitidos y validaciones de formularios.
- Controllers/: acciones MVC y endpoints AJAX.
- Services/: transacciones de reservas, validacion de sesion y datos de demostracion.
- Data/ y Migrations/: contexto, relaciones y estructura versionada de MySQL.
- Views/ y wwwroot/js/: pantallas y comportamiento Vue.
- docs/Guia_ABM.md: explicacion teorica y ejemplos.
- bin/ y obj/: archivos generados, ignorados por Git y ocultos en VS Code.

## Verificacion

Compilacion sin errores ni advertencias. Pruebas HTTP contra MySQL para CRUD, validaciones, relaciones, baja logica, antiforgery y paginado. Reservas: totales, modificacion de stock, rollback por falta de stock, cancelacion repetida y dos solicitudes simultaneas sin sobreventa.

Login: credenciales correctas/incorrectas, perfiles de tres roles, 401/403, redireccion externa rechazada, logout protegido, autodesactivacion rechazada, invalidacion por cambio de rol/password o desactivacion y autor de reserva no manipulable. Revision visual de la pantalla de login y de los ABM. Los fixtures estan identificados como Prueba; las reservas de prueba quedan canceladas y los usuarios/productos/categorias de prueba inactivos.

### Pruebas adicionales del portal

Registro de dos clientes, rechazo de DNI/email repetidos y passwords que no coinciden; rol Cliente fijo pese a manipulacion del formulario; vinculos unicos; catalogo con dos paginas; exclusion de productos/categorias inactivos; reserva con precios e identidad determinados por el servidor; rechazo de cantidades invalidas y stock insuficiente; consulta/cancelacion ajenas devuelven 404; cancelacion repetida no duplica stock; reservas internas visibles al cliente correcto. Revision del navegador: seleccion, login, regreso al catalogo conservando carrito, Mis reservas y pantalla de registro. No se modificaron productos del usuario para estas pruebas; los fixtures propios quedaron inactivos y sus reservas canceladas.

## Matriz de entrega - estado real

| Requisito | Estado | Codigo / evidencia |
|---|---|---|
| Seis tablas, 1:N y N:M con tabla intermedia | Implementado | Models/, Migrations/; DetalleReserva une Reserva y Producto |
| Login/logout y roles | Implementado | CuentaController, SesionCookieEvents, Program.cs y Authorize |
| Avatar personalizado | Implementado | Cuenta/Perfil, CuentaController y Services/ImagenesService.cs |
| Archivo adicional al avatar | Implementado | Productos/SubirImagen, Catalogo y Services/ImagenesService.cs |
| ABM Vue/AJAX sin recargar | Implementado | Controllers/, Views/ y wwwroot/js/ |
| Paginado de servidor | Implementado | Listar en los controladores |
| Seleccion asincrona | Implementado | Reservas/BuscarClientes y BuscarProductos |
| API JWT | Implementado | Controllers/Api/, Services/JwtService.cs, docs/api/Guia_API_JWT.md |
| Diagramas DER/clases | Pendiente | docs/ |
| SQL y datos para promocionar | Parcial | Migracion inicial y cuentas demo; falta exportacion completa |
| Evidencias finales con enlaces/capturas | Pendiente | Agregar tras cerrar funcionalidades |
| Coleccion de pruebas API | Implementado | docs/api/Libreria_API.postman_collection.json |

No se hicieron commits ni push de las etapas recientes: los realiza el estudiante.

## Herramientas y entorno restringido

`dotnet ef migrations has-pending-model-changes` verifica consistencia del modelo. `dotnet ef migrations script` genera SQL sin aplicarlo.

Solo para pruebas en entornos restringidos, `DataProtection__KeysPath` permite configurar una carpeta privada escribible para las claves en Development. Sin esa opcion se mantiene el almacenamiento predeterminado de ASP.NET. No versionar ni compartir las claves.


## API JWT

Implementada siguiendo los apuntes Web_API_1.pdf y Autenticacion_con_JWT_1.pdf. Login API: POST /api/auth/login. Enviar el accessToken en Authorization: Bearer para consumir productos, categorias, clientes y reservas segun el rol. La web conserva su login con cookies; la API exige JWT.

Guia teorica/practica: docs/api/Guia_API_JWT.md. Coleccion importable: docs/api/Libreria_API.postman_collection.json, con 21 peticiones y tests de resultado. No contiene tokens reales ni clave de firma.

La clave se guarda en appsettings.Local.json (ignorado y excluido de la publicacion), o en TokenAuthentication__SecretKey. No publicar ni compartir esta clave. Las cuentas demo conservan las credenciales documentadas arriba.

Access tokens de 30 minutos, algoritmo HS256, validacion de firma/emisor/audiencia/vencimiento y estado actual de cuenta. VersionToken permite revocar JWT por cambio de password o logout API. Requiere aplicar la migracion VersionarTokensJwt al instalar. No se agregaron refresh tokens ni un frontend separado.

## Avatares e imagenes de productos

Desde Mi perfil, cada usuario carga/reemplaza o quita su avatar. Desde Productos, el administrador o empleado selecciona una imagen al crear o editar: los datos y luego la foto se envian sin recargar la pagina. Si falla solamente la foto, los datos quedan guardados y el formulario permite reintentar sin duplicar el producto. El catalogo muestra la imagen guardada.

Se aceptan JPG/PNG de hasta 2 MB, 4096 pixeles por lado y 12 megapixeles. SkiaSharp valida y decodifica el contenido, y lo recodifica a PNG; no basta con cambiar la extension. Los nombres son GUID generados en el servidor. Los POST/DELETE web requieren antiforgery; el avatar pertenece siempre a la cuenta de la sesion y las fotos de productos solo las cambia el personal.

Las fotos son publicas y se sirven mediante `/imagenes/{nombre}` como image/png, con nosniff. Se almacenan en `App_Data/imagenes`, fuera de wwwroot y de Git. MySQL guarda solo el nombre. No requieren migracion nueva porque Avatar e Imagen ya existian. La API devuelve Imagen como nombre; su URL es `/imagenes/` seguida de ese nombre. La carga se realiza desde la web MVC.

**Respaldo:** copiar `App_Data/imagenes` junto con la base de datos. En otro equipo la carpeta se crea con la primera carga y la cuenta que ejecuta la app debe tener permiso de escritura. Publicar la app no copia imagenes personales ni las elimina; conservar/restaurar esa carpeta en el despliegue. No se deben borrar fotos desde el explorador mientras la base las referencia. Para una entrega con fotos precargadas se debe proporcionar tambien su carpeta de respaldo.

Guia practica y explicacion: [docs/Imagenes.md](docs/Imagenes.md). Se verificaron carga JPG/PNG, reemplazo y eliminacion, catalogo, identidad del avatar, acceso por roles, antiforgery, formato falso, tamano excesivo, dimensiones excesivas y archivos dañados mediante pruebas HTTP contra MySQL.
