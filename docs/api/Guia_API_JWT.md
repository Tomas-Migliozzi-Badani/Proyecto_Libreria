# API y JWT: guia practica para la libreria

## De donde sale cada parte

Se uso el material del curso:

- Autenticacion_con_JWT_1.pdf, diapositivas 2-5: autenticacion/autorizacion, estructura header.payload.signature y flujo Bearer.
- Mismo PDF, diapositivas 6-10: AddJwtBearer, validaciones, middleware, emision, Authorize/AllowAnonymous y claims.
- Web_API_1.pdf, diapositivas 3, 5-10: recursos REST, GET/POST/PUT/DELETE, codigos HTTP, ControllerBase, ApiController, binding y DTOs.

La documentacion oficial solo se consulto para comprobar la compatibilidad del paquete con .NET 10 y detalles de configuracion actuales.

## Recorrido completo

1. Postman envia email y password por JSON a POST /api/auth/login.
2. AuthApiController busca el usuario activo y verifica el hash con PasswordHasher. No devuelve la password ni crea una cookie.
3. JwtService crea claims de ID (sub), nombre, rol, identificador del token (jti), version y vencimiento. Firma con HMAC SHA-256 usando una clave privada del servidor.
4. La respuesta contiene accessToken, tokenType=Bearer y expiresAt.
5. Postman envia Authorization: Bearer <accessToken> al consultar /api/productos.
6. JwtBearer valida firma, algoritmo permitido, emisor, audiencia y vencimiento. Luego JwtUsuarioEvents comprueba estado, rol y version actuales en MySQL.
7. Authorize aplica permisos. Si todo es valido, el controlador consulta EF Core y devuelve JSON.

## Tres partes del JWT

- Header: tipo de token y algoritmo.
- Payload: claims, como sub, role, iss, aud y exp.
- Signature: permite detectar cambios utilizando la clave secreta.

El payload esta codificado, no cifrado. No colocar passwords, hashes ni la clave secreta en los claims. La firma tampoco convierte el contenido en secreto. El token es una credencial: no compartirlo ni guardarlo en Git.

## Cookie MVC y JWT API

Las pantallas existentes siguen usando Cookies. Los controladores de Controllers/Api exigen explicitamente el esquema Bearer. Tener la cookie del sitio no habilita la API. Enviar un JWT tampoco inicia la sesion MVC.

No se agrego un frontend separado: Postman consume la API y la web sigue funcionando como antes. CORS no es necesario para Postman o solicitudes del mismo origen; si despues se agrega otro frontend con distinto origen, se debe permitir ese origen concreto siguiendo la diapositiva de CORS.

## Adaptaciones al proyecto

El ejemplo del apunte usa ocho horas. Aqui los access tokens duran 30 minutos y admiten 30 segundos de tolerancia de reloj. Se valida tiempo UTC.

El JWT permite validar firma y claims sin consultar la base. Como control adicional de este proyecto, se consulta tambien el estado de la cuenta en cada peticion para aplicar cambios de rol/desactivacion inmediatamente. VersionToken permite revocar JWT al cambiar la password o cerrar sesiones API. Esto agrega una consulta y evita mantener sesiones o una lista completa de tokens en memoria; no es un requisito del ejemplo basico del PDF.

POST /api/auth/logout incrementa la version y revoca todos los JWT anteriores de esa cuenta. No cierra la cookie del sitio. El logout MVC cierra su cookie. No se implementaron refresh tokens: al vencer o ser revocado, se vuelve a pedir un JWT con el login API.

## Endpoints

| Metodo y ruta | Acceso | Respuesta normal |
|---|---|---|
| POST /api/auth/login | Publico, credenciales validas | 200 + token |
| GET /api/auth/me | JWT valido | 200, datos propios |
| POST /api/auth/logout | JWT valido | 204, revoca JWT anteriores |
| GET /api/categorias | JWT valido | 200, paginado; Cliente solo activas |
| GET /api/clientes | Administrador/Empleado | 200, paginado |
| GET /api/productos | JWT valido | 200, paginado; Cliente solo catalogo activo |
| GET /api/productos/{id} | JWT valido | 200 o 404 |
| POST /api/productos | Administrador/Empleado | 201 + Location |
| PUT /api/productos/{id} | Administrador/Empleado | 204 |
| DELETE /api/productos/{id} | Administrador/Empleado | 204, baja logica |
| GET /api/reservas | JWT valido | 200; Cliente solo propias |
| GET /api/reservas/{id} | JWT valido | 200; ajena para Cliente devuelve 404 |
| POST /api/reservas | JWT valido | 201 + Location |
| PUT /api/reservas/{id} | Administrador/Empleado | 204, solo pendientes |
| DELETE /api/reservas/{id} | JWT valido | 204, cancela y devuelve stock; Cliente solo propias |

Los listados aceptan pagina. Productos/categorias/clientes tambien aceptan buscar. El servidor devuelve elementos, pagina, paginas y total, con diez filas como maximo por pagina.

## HTTP: como leer el resultado

- 200: consulta o login correcto.
- 201: se creo un recurso; Location permite consultarlo.
- 204: operacion completada sin cuerpo JSON.
- 400: datos invalidos, precio negativo, falta de stock o regla de negocio.
- 401: falta token o es invalido, vencido o revocado.
- 403: token valido, pero rol sin permiso.
- 404: registro inexistente o reserva ajena.
- 409: conflicto al guardar o por operacion concurrente.
- 429: demasiados intentos de login; esperar un minuto.

ApiController valida los DTOs automaticamente y devuelve 400 cuando fallan. Los endpoints JSON exigen content-type application/json. No aceptan cookies como autenticacion ni usan el token antiforgery MVC, porque requieren Authorization Bearer explicito.

## Ejemplo para probar

Con el sitio iniciado, enviar:

POST http://localhost:5063/api/auth/login
Content-Type: application/json

{"email":"administrador@libreria.test","password":"123"}

Copiar accessToken y usarlo en Authorization -> Bearer Token para:

GET http://localhost:5063/api/productos?pagina=1

Crear una reserva de cliente:

POST http://localhost:5063/api/reservas
Authorization: Bearer <token de cliente>
Content-Type: application/json

{"detalles":[{"idProducto":ID_REAL,"cantidad":2}]}

El cliente no puede elegir propietario, autor, precio, estado ni vencimiento. El servidor usa su cuenta vinculada y confirma disponibilidad, con la misma transaccion de stock de la web. El personal indica ademas idCliente; puede indicar fechaVencimiento, y el autor sigue siendo el usuario del token.

## Coleccion Postman

Importar Libreria_API.postman_collection.json (en docs/api/). Ajustar baseUrl si tu terminal anuncia otro puerto. Usa las cuentas demo documentadas. Ejecutar carpetas en orden: login, consultas, ABM de producto de prueba, reserva del cliente, comprobaciones negativas, cancelacion y revocacion.

La coleccion guarda automaticamente los tokens y los IDs como variables locales de Postman. Los tokens iniciales estan vacios y no se exporto ningun token real. Requiere al menos una categoria activa entre los resultados de la primera pagina: si falta, crearla desde Categorias. Crea su propio producto de prueba y lo desactiva despues; conserva la reserva cancelada como historial. La ultima peticion comprueba que el token revocado ya devuelve 401.

## Configuracion en otro equipo

Copiar appsettings.Local.example.json a appsettings.Local.json. Completar MySQL y reemplazar TokenAuthentication:SecretKey por una clave aleatoria de al menos 32 bytes. Por ejemplo, generar una en PowerShell:

[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))

Guardar el resultado solo en appsettings.Local.json, no en appsettings.json ni en el README. Tambien se puede usar TokenAuthentication__SecretKey como variable de entorno. Issuer=Libreria y Audience=Libreria.Api estan en la configuracion publica, no son secretos.

Ejecutar dotnet restore, dotnet tool restore y dotnet ef database update antes de dotnet run. No se rotaron las claves ni passwords de las cuentas existentes. La nueva migracion agrega VersionToken con valor cero.

## Que se probo

Firma alterada, token vencido, algoritmo distinto, ausencia de exp, emisor/audiencia incorrectos, rol/version manipulados, token ausente, credenciales incorrectas; cookies rechazadas en la API y JWT ignorado por MVC; roles; CRUD de productos; reservas, stock e identidad del cliente; reservas ajenas 404; cambios de password/desactivacion/logout revocan tokens. Se ejecutaron las 21 peticiones y sus tests de la coleccion contra MySQL desde un ejecutor local compatible con los scripts de comprobacion.
