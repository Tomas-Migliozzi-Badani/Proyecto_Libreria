# Librería - Proyecto integrador MVC

Sistema para consultar productos de librería y reservarlos para retirar en el local. El personal administra categorías, productos, clientes y reservas; el administrador también gestiona usuarios. Cada cliente puede registrarse y consultar o cancelar sus propias reservas.

**Tecnologías:** ASP.NET Core MVC (.NET 10), C#, Entity Framework Core, MySQL, Razor, Vue 3 y fetch. Las vistas usan cookies; la API utiliza JWT. Las reservas y los movimientos de stock se procesan juntos mediante transacciones.

## Inicio rápido para evaluar

Requisitos: SDK .NET 10 y servidor MySQL 8.0 o superior iniciado. Workbench es un cliente: abrirlo no inicia por sí solo el servidor.

1. Descargar/clonar el repositorio y abrir la carpeta que contiene `Proyecto_Libreria.csproj`.
2. Ejecutar:

   ```powershell
   dotnet restore
   dotnet tool restore
   Copy-Item appsettings.Local.example.json appsettings.Local.json
   ```

3. En MySQL Workbench abrir y ejecutar completo [docs/sql/Libreria_Demo.sql](docs/sql/Libreria_Demo.sql). Crea `Libreria_Demo`, las tablas, las migraciones aplicadas y los datos ficticios. Debe ejecutarse **una sola vez sobre una base nueva**. Si ese nombre ya está ocupado con tablas, elegir otro en las instrucciones CREATE DATABASE y USE. No contiene DROP DATABASE ni exporta información personal del estudiante.
4. En `appsettings.Local.json`, completar usuario/contraseña de MySQL y usar `database=Libreria_Demo` (o el nombre elegido). Establecer `TokenAuthentication:SecretKey` con una clave aleatoria propia de al menos 32 bytes. Para generar una en PowerShell:

   ```powershell
   $bytesClave = New-Object byte[] 48
   $generadorClave = [System.Security.Cryptography.RandomNumberGenerator]::Create()
   $generadorClave.GetBytes($bytesClave)
   $generadorClave.Dispose()
   [Convert]::ToBase64String($bytesClave)
   ```

   Copiar el resultado como SecretKey del archivo local. No publicarlo en Git. La plantilla tiene un marcador que debe reemplazarse: la aplicación rechaza claves ausentes, cortas o sin configurar.
5. Ejecutar `dotnet ef database update`: con el SQL completo no aplicará nada porque incluye el historial de las tres migraciones; también comprueba que el esquema esté actualizado.
6. Ejecutar `dotnet run`. Abrir la dirección que indique la terminal (perfil HTTP predeterminado: http://localhost:5063).
7. Iniciar sesión con una de las cuentas de la siguiente tabla. Para probar la API, importar la colección y ajustar su variable baseUrl si cambia el puerto.

Para detener la aplicación usar Ctrl+C antes de recompilar o iniciar otra instancia. Si se está en la carpeta superior, ejecutar `dotnet run --project Proyecto_Libreria/Proyecto_Libreria.csproj`.

### Cuentas de demostración

| Rol | Email | Contraseña |
|---|---|---|
| Administrador | administrador@libreria.test | 123 |
| Empleado | empleado@test | 123 |
| Cliente | cliente@libreria.test | Libreria2026! |

Son credenciales públicas de evaluación. El registro y las altas habituales exigen contraseñas de al menos ocho caracteres; 123 es una excepción de las cuentas demo solicitadas para probar. Las contraseñas del SQL están derivadas con el formato de PasswordHasher, no almacenadas como texto plano en la columna Password.

En Development, `DatosPrueba:Habilitar=true` crea solo cuentas demo faltantes y no modifica las existentes. Para un uso fuera de la evaluación se deshabilita esa opción y no se importan estas cuentas públicas.

### Qué incluye el SQL

4 categorías, 16 productos activos, 12 clientes ficticios, 3 usuarios, 12 reservas y 16 detalles. La reserva #1 tiene cinco productos y está pendiente; su stock **ya está descontado** en los valores iniciales. Las restantes están canceladas y no comprometen stock. El vencimiento se calcula respecto de NOW() al importar; los históricos son ejemplos.

La instalación no incluye fotos personales. Para demostrar la carga se proporcionan [avatar.png](docs/datos-demo/avatar.png) y [producto.png](docs/datos-demo/producto.png): cargarlos desde Mi perfil y Productos. Las capturas muestran esos mismos archivos subidos a la aplicación.

### Alternativa: base vacía con migraciones

Si se prefiere comenzar sin datos, configurar una base nueva y ejecutar `dotnet ef database update` sin importar el SQL completo. En desarrollo se crearán las tres cuentas demo, pero no productos ni reservas. Para cargar después los datos SQL, hacerlo antes del primer inicio de la aplicación y sobre tablas vacías, usando [02_Datos_Demo.sql](docs/sql/02_Datos_Demo.sql). No mezclar ese script con datos ya creados.

La estructura separada está en [01_Estructura.sql](docs/sql/01_Estructura.sql). Es la salida de las migraciones, no un esquema mantenido manualmente. No usar EnsureCreated junto con migraciones.

## Matriz de requisitos y evidencias

Los enlaces llevan al código concreto y a evidencia guardada en este repositorio. Las capturas se tomaron el 05/11/2026 en una copia limpia y una base de pruebas independiente, con datos ficticios. Se utilizó el puerto 5100 para no interferir con una instancia del estudiante.

| Requisito | Estado | Implementación | Evidencia |
|---|---|---|---|
| Seis tablas, relaciones 1:N y N:M | Completo | [Contexto y relaciones](Data/LibreriaContext.cs#L20) | [DER](docs/diagramas/DER.png) · [Reserva con cinco detalles](docs/evidencias/10_Reserva_cinco_detalles.png) |
| Login/logout, roles y acceso restringido | Completo | [Login](Controllers/CuentaController.cs#L31) · [Rol administrador](Controllers/UsuariosController.cs#L12) | [Login](docs/evidencias/01_Login.png) · [Pruebas de permisos](docs/evidencias/Verificacion_instalacion.json) |
| Avatar personalizado | Completo | [Avatar propio](Controllers/CuentaController.cs#L123) | [Perfil con avatar](docs/evidencias/02_Perfil_avatar.png) |
| Manejo de archivos adicional al avatar | Completo | [Subida de producto](Controllers/ProductosController.cs#L89) · [Validación](Services/ImagenesService.cs#L21) | [Imagen en catálogo](docs/evidencias/03_Catalogo_imagen.png) |
| ABM completo con Vue/AJAX sin recarga | Completo | [Vue y fetch](wwwroot/js/categorias.js#L43) · [Controlador](Controllers/CategoriasController.cs#L43) | [Antes](docs/evidencias/05_ABM_antes.png) · [Alta](docs/evidencias/06_ABM_alta.png) · [Edición](docs/evidencias/07_ABM_edicion.png) · [Baja](docs/evidencias/08_ABM_baja.png) |
| Paginado del servidor en listados principales | Completo | [Skip/Take y conteo](Controllers/ProductosController.cs#L23) · [Reservas propias](Controllers/MisReservasController.cs#L23) | [Página 2](docs/evidencias/04_Productos_pagina2.png) · [Respuesta real JSON](docs/evidencias/Paginado_respuesta.json) |
| Selección asincrónica de entidades relacionadas | Completo | [Búsqueda acotada](Controllers/ReservasController.cs#L49) · [Solicitudes del frontend](wwwroot/js/reservas.js#L38) | [Búsqueda de clientes](docs/evidencias/09_Busqueda_clientes.png) |
| Web API protegida con JWT | Completo | [Login API](Controllers/Api/AuthApiController.cs#L27) · [Firma y claims](Services/JwtService.cs#L15) · [Revocación](Services/JwtUsuarioEvents.cs#L11) | [21 comprobaciones HTTP](docs/evidencias/API_coleccion_resultados.txt) · [Guía y ejemplo Bearer](docs/api/Guia_API_JWT.md) |
| Colección Postman/Bruno para API | Completo | [Colección Postman](docs/api/Libreria_API.postman_collection.json) | [Resultados ejecutados](docs/evidencias/API_coleccion_resultados.txt) |
| Diagrama DER y diagrama de clases | Completo | [DER editable](docs/diagramas/DER.md) · [Clases editable](docs/diagramas/Clases.md) | [DER PNG](docs/diagramas/DER.png) · [Clases PNG](docs/diagramas/Clases.png) |
| SQL con datos de prueba y credenciales | Completo | [Instalación completa](docs/sql/Libreria_Demo.sql) · cuentas documentadas arriba | [Instalación verificada](docs/Verificacion_Entrega.md) |
| Gitignore y ejecución documentada | Completo | [.gitignore](.gitignore) · inicio rápido de este README | [Copia limpia y pruebas](docs/Verificacion_Entrega.md) |

## Cómo está organizado

```text
Models/                  Entidades persistentes
ViewModels/              Formularios y validaciones de entrada
Controllers/             Vistas MVC y acciones JSON para Vue
Controllers/Api/         Endpoints JSON con esquema Bearer explícito
Services/                Reglas de reservas, sesión, JWT e imágenes
Data/ y Migrations/      Contexto, relaciones y evolución de MySQL
Views/ y wwwroot/js/     Razor y comportamiento de los módulos Vue
docs/                    Diagramas, SQL, colección, guías y evidencias
```

Bin y obj son archivos generados: están ignorados por Git. Vue está incluido localmente con su licencia MIT; la aplicación no necesita Node/Vite para funcionar.

### Roles y seguridad

- El catálogo es público. Para reservar, un visitante crea cuenta con rol Cliente fijado en el servidor.
- Empleado administra categorías, productos, clientes y reservas; Administrador además administra usuarios.
- Cada cliente ve/cancela sus reservas mediante su vínculo Usuario.IdCliente, opcional y único. No se acepta un propietario o autor arbitrario del navegador.
- Authorize controla permisos en el backend; ocultar menús no reemplaza esa validación.
- PasswordHasher verifica contraseñas. Las consultas de login usan valores parametrizados mediante LINQ.
- La cookie web es HttpOnly, de ocho horas con renovación por actividad. Los cambios relevantes de cuenta invalidan la sesión; los intentos de login están limitados por IP.
- Las escrituras web requieren antiforgery. Las redirecciones del login solo aceptan destinos locales.
- La API exige JWT Bearer: HS256, 30 minutos, firma/emisor/audiencia/vigencia y estado/rol/versión de la cuenta. El payload no está cifrado. Logout API revoca todos los JWT anteriores de la cuenta; no cierra su cookie web. No hay refresh tokens.
- `appsettings.Local.json` se ignora y no se copia al publicar. `ConnectionStrings__Libreria` y `TokenAuthentication__SecretKey` permiten configurar cada equipo mediante entorno.

### Reservas y alcance

Una reserva agrupa varios detalles. ReservasService obtiene precios desde la base, mantiene el precio histórico de líneas existentes y coordina reserva/stock con transacciones, bloqueos y descuento condicionado por disponibilidad. Cancelar devuelve stock una sola vez y conserva el historial. La baja de productos/categorías/usuarios es lógica.

El cliente puede cancelar una pendiente y crear otra selección; el personal puede editar pendientes. El sistema no implementa pagos, envío ni un flujo de venta/retiro completado. El vencimiento es informativo: no existe una tarea automática que cancele y devuelva stock al vencer. La reserva mantiene stock comprometido hasta cancelarla.

### Archivos y respaldo

JPG/PNG de hasta 2 MB, 4096 píxeles por lado y 12 megapíxeles. Se valida/decodifica con SkiaSharp y se genera un PNG con nombre GUID. Las fotos de productos y avatares son públicas y se sirven por una ruta controlada.

Los archivos quedan en `App_Data/imagenes`, fuera de wwwroot y Git; MySQL guarda su nombre. Para trasladar datos completos se respalda la base **y** esa carpeta. La aplicación necesita permiso de escritura y la publicación no copia automáticamente las fotos personales. Al desplegar, conservar o restaurar la carpeta. El SQL demo no referencia archivos externos: se pueden subir las dos imágenes de ejemplo incluidas.

## Verificación y guías

Se restauró y compiló una copia sin bin/obj, se importó el SQL en una base nueva y se verificó el historial de migraciones. Resultado: **0 errores y 0 advertencias**, 21 comprobaciones de colección API y 13 comprobaciones de instalación/roles/datos/archivos. Detalle y límites en [Verificación de entrega](docs/Verificacion_Entrega.md).

Las capturas muestran operaciones reales en navegador. El registro de la API corresponde a solicitudes HTTP y scripts de la colección ejecutados mediante un runner de desarrollo; no se presenta como captura de Postman ni como cobertura unitaria completa. La colección importable queda disponible para repetir en Postman.

- [Guía de ABM](docs/Guia_ABM.md).
- [Portal de clientes](docs/Portal_Clientes.md).
- [API y JWT](docs/api/Guia_API_JWT.md).
- [Imágenes](docs/Imagenes.md).
- [Índice de evidencias](docs/evidencias/README.md).
- [DER](docs/diagramas/DER.md) y [clases](docs/diagramas/Clases.md).

Para generar nuevamente el esquema: `dotnet ef migrations script --output docs/sql/01_Estructura.sql`. Si cambia el modelo o la estructura, actualizar también el SQL completo y volver a verificar la importación.
