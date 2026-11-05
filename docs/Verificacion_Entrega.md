# Verificación de entrega - 05/11/2026

## Entorno y método

Se utilizó una copia limpia de los archivos versionados, con los documentos nuevos, sin bin, obj, .git ni imágenes personales. Se configuraron conexión y clave JWT propias de prueba. Se creó una base temporal distinta de Libreria; no se borró ni se exportó la base de trabajo del estudiante. La aplicación de esa copia se ejecutó en http://127.0.0.1:5100.

Se verificó esta instalación en Windows con SDK .NET 10 y MySQL 8. No equivale a una prueba ejecutada en todos los sistemas operativos; los paquetes nativos de SkiaSharp para otras plataformas se distribuyen con el proyecto.

## Resultados

| Comprobación | Resultado |
|---|---|
| dotnet restore y dotnet tool restore en la copia limpia | Correcto |
| dotnet build --no-restore tras restauración con acceso a NuGet | 0 advertencias, 0 errores |
| Importación de Libreria_Demo.sql, cambiando solo el nombre de la base temporal | Correcto |
| Categorias / Productos / Clientes / Usuarios | 4 / 16 / 12 / 3 registros iniciales |
| Reservas / DetallesReserva | 12 / 16 registros iniciales |
| Historial de migraciones | 3, correspondientes al proyecto |
| dotnet ef database update | Sin migraciones pendientes |
| dotnet ef migrations has-pending-model-changes | Sin cambios pendientes de modelo |
| Colección API: 21 solicitudes y comprobaciones | Aprobadas |
| Instalación: 13 comprobaciones HTTP sobre datos, roles y archivos | Aprobadas |
| Navegador: login, perfil/avatar, catálogo, paginado, alta, edición, baja y búsqueda | Capturas guardadas |

La primera restauración restringida no pudo consultar el servicio de auditoría de NuGet y emitió NU1900. Se repitió con acceso normal y la compilación final quedó sin advertencias. No se deshabilitó la auditoría para ocultar ese aviso.

## Qué se comprobó

El [informe JSON](evidencias/Verificacion_instalacion.json) registra los tres logins usando los hashes importados por SQL, rechazo 403 de permisos incorrectos, páginas siguientes de catálogo/clientes, cinco detalles en la reserva #1, reserva ajena rechazada, búsqueda acotada, stock inicial esperado y carga real de las dos imágenes demo.

El [registro API](evidencias/API_coleccion_resultados.txt) incluye creación/lectura/edición/baja lógica, reserva y cancelación repetida, 401/403/404 y revocación del token tras logout. Se ejecutaron solicitudes y los scripts de la colección mediante un runner de desarrollo; no se afirma haber usado la interfaz de Postman. La [colección estándar](api/Libreria_API.postman_collection.json) se importa para repetirlos en esa aplicación.

Las pruebas de la colección crean un producto y una reserva adicionales, que quedan inactivos/cancelados. El recorrido de capturas agrega una categoría propia y la desactiva. Por eso las capturas posteriores pueden mostrar 17 productos, 13 reservas o 5 categorías, mientras el SQL inicial incluye 16, 12 y 4. No son discrepancias del importador.

## Cómo repetirlo

1. Seguir el inicio rápido del README, usando una base nueva y el SQL completo.
2. Comprobar los tres logins de la tabla de credenciales.
3. Entrar al catálogo: comprobar la segunda página y reservar con el cliente.
4. Consultar reserva #1: contiene cinco productos y total de 30900.
5. Probar una categoría: crear, editar y desactivar sin recargar el módulo.
6. En Reservas buscar Persona: se devuelven hasta diez clientes.
7. Subir los PNG de docs/datos-demo al perfil y a un producto; verificar catálogo.
8. Importar la colección Postman; ajustar baseUrl; ejecutar sus carpetas en orden.
9. Detener la instancia con Ctrl+C.

Las credenciales son públicas de evaluación. No compartir cookies ni tokens activos. No ejecutar el SQL inicial sobre la base de trabajo ni repetirlo sobre tablas pobladas.

## Límites del resultado

Estas pruebas cubren escenarios concretos, no garantizan ausencia absoluta de errores ni sustituyen revisión de código. No existe un proyecto xUnit ni se declara cobertura unitaria/CI. El sistema sigue siendo de reservas: pagos, envío, retiro completado y cancelación automática por vencimiento están fuera del alcance implementado.
