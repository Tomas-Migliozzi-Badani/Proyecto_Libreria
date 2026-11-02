# Proyecto Libreria

ASP.NET Core MVC, .NET 10, Entity Framework Core y MySQL. Estado: modelos y persistencia inicial; las pantallas de gestion y el login todavia estan pendientes.

## Ejecutar en otro equipo

Requisitos: SDK .NET 10 y servidor MySQL 8.0 o superior en ejecucion. Workbench por si solo no reemplaza al servidor.

Desde la carpeta que contiene Proyecto_Libreria.csproj:

1. Ejecutar `dotnet restore` y `dotnet tool restore`.
2. Copiar `appsettings.Local.example.json` a `appsettings.Local.json`.
3. Cambiar servidor, puerto, usuario y password en el archivo local. No subirlo a Git. El nombre sugerido para una base nueva y dedicada es `Libreria`.
4. Ejecutar `dotnet ef database update`. El usuario debe tener permisos para crear la base y las tablas, o usar una base vacia creada previamente.
5. Ejecutar `dotnet run` y abrir la direccion indicada en la terminal.

Tambien se puede configurar la variable de entorno `ConnectionStrings__Libreria`, que tiene prioridad sobre el archivo local.

## Migraciones

Las migraciones describen la estructura; no contienen los registros de tu equipo. Se guardan en Git. Para cambiar el esquema: modificar el modelo, ejecutar `dotnet ef migrations add NombreDelCambio`, revisar la migracion y luego aplicar con `dotnet ef database update`.

No utilizar EnsureCreated junto con migraciones. No se actualiza la base automaticamente al iniciar el sitio.

Comprobaciones sin servidor: `dotnet build`, `dotnet ef migrations has-pending-model-changes` y `dotnet ef migrations script`.

## Siguientes etapas

Validaciones de entrada; CRUD de categorias, productos y clientes; login y roles; reservas con control transaccional de stock; pruebas y datos de demostracion. La consigna exige un ABM completo en Vue/AJAX, paginado en servidor, busquedas asincronas y Web API con JWT. SignalR y pruebas aparecen en los temas de clase, pero no en los requisitos minimos de las capturas.


## Matriz de requisitos de entrega (revision 2 de noviembre)

Estado real: Pendiente significa que todavia no se ha implementado ni demostrado el requisito completo.

| Requisito | Estado | Ubicacion / evidencia |
|---|---|---|
| Al menos seis tablas, 1:N y N:M con tabla intermedia | Estructura generada; falta aplicar en MySQL | Models/, Data/, Migrations/ |
| Login, logout, roles y funcionalidad restringida | Pendiente | Se implementara con cookies para MVC |
| Avatar personalizado | Pendiente | Perfil de usuario |
| Archivos adicionales al avatar | Pendiente | Imagen de producto |
| ABM completo Vue/AJAX sin recargar | Pendiente | Categorias o productos |
| Paginado desde backend | Pendiente | Listados principales |
| Seleccion asincrona de entidades | Pendiente | Clientes/productos al reservar |
| API autenticada con JWT | Pendiente | API de productos y reservas |
| Diagrama DER / clases | Pendiente | docs/ |
| SQL y datos de prueba para promocionar | Esquema generado; faltan datos | Pendiente exportar entrega final |
| Credenciales de prueba por rol | Pendiente | Pendiente inicializacion segura |
| Evidencias con enlaces y capturas | Pendiente | Se agregaran al verificar cada funcion |
| Coleccion Postman/Bruno si no hay cliente nativo | Pendiente | Se incluira para facilitar evaluacion |

No considerar los ejemplos de la consigna como requisitos ya cumplidos. La entrega es el 5 de noviembre de 2026 a las 23:59 (Argentina).


## ABM de Categorias

Abrir `/Categorias`. La pantalla usa Vue 3.5.13 (incluido en wwwroot/lib/vue con licencia MIT) y fetch. Permite alta, edicion, busqueda y baja logica con reactivacion desde Editar. El listado usa pagina de 10 filas resuelta en MySQL con Skip/Take y orden estable.

- Controlador: Controllers/CategoriasController.cs.
- Datos de formulario: ViewModels/CategoriaFormulario.cs (evita recibir propiedades no editables).
- Vista: Views/Categorias/Index.cshtml.
- Interaccion: wwwroot/js/categorias.js.
- Cambios protegidos por token antifalsificacion. La restriccion por rol sigue pendiente del modulo de autenticacion; no desplegar este modulo como administracion publica.
- Pruebas HTTP contra MySQL: listado, alta, edicion, baja logica, busqueda, ajuste de pagina fuera de rango, nombre obligatorio/longitud, duplicados, registro inexistente y rechazo de cambios sin token. Compilacion sin errores ni advertencias. Falta revision visual manual del flujo completo.
- Se conservo una categoria de prueba inactiva identificada por el prefijo `Prueba ABM`.

## Carpetas generadas

bin/Debug/net10.0 contiene la aplicacion compilada, bibliotecas NuGet, simbolos de depuracion y manifiestos de recursos. obj contiene dependencias restauradas, codigo generado y caches intermedias. Se regeneran al restaurar/compilar: no editar ni versionar. Revision: JSON de manifiestos valido, framework net10.0, 36 dependencias restauradas y carpetas excluidas de Git.

Para pruebas en un entorno restringido puede configurarse `DataProtection__KeysPath` con una carpeta privada escribible; solamente se usa en Development. Si no se configura, ASP.NET conserva su almacenamiento de claves predeterminado.


## ABM de Productos

Abrir `/Productos`. Permite alta, consulta, modificacion, baja logica y reactivacion, sin recargar la pagina. Incluye nombre, marca, precio, stock y categoria. El campo Imagen del modelo se conserva; la subida de imagenes queda pendiente para el modulo de archivos.

- Controllers/ProductosController.cs: consultas con paginado de 10 filas y cambios en MySQL.
- ViewModels/ProductoFormulario.cs: validacion de nombre, marca, precio positivo, stock entero no negativo y categoria.
- Views/Productos/Index.cshtml y wwwroot/js/productos.js: pantalla Vue y peticiones AJAX.
- Seleccion de categoria: busqueda asincrona acotada a 10 resultados activos. Nunca descarga el catalogo completo.
- Se permite conservar la categoria historica de un producto aunque haya sido desactivada; las nuevas asignaciones exigen categoria activa.
- Se verificaron por HTTP contra MySQL: alta, edicion, baja, reactivacion, decimales, valores invalidos, categoria inexistente/inactiva, antiforgery, busqueda y ajuste de paginado. Falta revision visual por el usuario.
- Todavia falta la restriccion por rol de ambos ABM al implementar autenticacion.

Para recorrer el proyecto en VS Code se ocultan bin y obj desde .vscode/settings.json; siguen funcionando y se regeneran normalmente. Los cambios de esta etapa quedan sin commit ni push a pedido del estudiante.
