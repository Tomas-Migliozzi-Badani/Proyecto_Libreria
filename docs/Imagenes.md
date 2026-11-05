# Avatares e imagenes

## Probar antes del commit

1. Iniciar MySQL y ejecutar `dotnet run` desde la carpeta del proyecto.
2. Entrar como administrador. En Mi perfil, elegir un JPG/PNG de hasta 2 MB y pulsar Guardar avatar. Volver al perfil para comprobar que persiste. Probar reemplazarlo y quitarlo.
3. En Productos, crear un producto con una categoria activa y elegir una foto. Al guardar, debe aparecer en la lista sin recargar. Abrir el Catalogo y buscar su nombre: se muestra la misma foto.
4. Editar el producto, cambiar la imagen y guardar. Probar Quitar imagen actual; quitar una foto es inmediato y no guarda otros cambios pendientes del formulario.
5. Intentar subir un TXT renombrado a PNG o una imagen de mas de 2 MB: debe rechazarse. Si se guardaron los datos pero fallo la foto, el aviso lo explica; corregir la foto y guardar de nuevo edita el mismo producto.
6. Entrar como Cliente: puede cambiar su avatar y ver las fotos del catalogo, pero no abrir Productos ni modificar sus imagenes.
7. Detener con Ctrl+C antes de recompilar. Hacer el commit desde GitHub Desktop como de costumbre. No incluir App_Data, bin, obj ni appsettings.Local.json.

## Como funciona

El formulario envia el archivo con `multipart/form-data`. En Productos usamos FormData y fetch para conservar el ABM sin recargar; el navegador genera el Content-Type y su boundary. El perfil usa un formulario MVC convencional con redireccion para evitar reenviar al actualizar la pagina.

ImagenesService concentra las reglas compartidas: peso, extension, formato real, dimensiones y decodificacion completa. SkiaSharp lee los pixeles y genera un PNG nuevo: no conservamos scripts, nombres originales ni metadatos del archivo subido. Solo se guarda la primera imagen de un PNG animado. Los nombres GUID evitan sobreescribir otro archivo o construir rutas desde el nombre aportado por el usuario.

CuentaController toma el IdUsuario de la cookie autenticada, no del formulario. ProductosController exige rol Administrador o Empleado. Ambos validan antiforgery. La eliminacion tambien requiere permiso y antiforgery.

El archivo queda en App_Data/imagenes y la columna Avatar o Imagen guarda su nombre. El controlador Imagenes sirve unicamente nombres GUID con extension PNG; no expone otros archivos del servidor. Las fotos son publicas, de modo que nunca deben usarse para documentos privados. La respuesta incluye Content-Type image/png y X-Content-Type-Options nosniff.

En un reemplazo se guarda primero la nueva foto, luego la referencia en MySQL y al final se borra la anterior. Si falla el guardado en MySQL se elimina la nueva foto y se conserva la anterior. Si no puede limpiarse un archivo viejo, se registra un aviso y se conserva la foto nueva; el registro permite detectar archivos sin uso. Un cierre abrupto del proceso entre esos pasos tambien puede dejar un archivo sin referencia: el sistema de archivos y MySQL no comparten una transaccion.

El archivo es independiente de los datos del producto. Primero guardamos sus datos y luego su foto. Si falla la carga, el producto sigue creado/editado y el mensaje permite reintentar sin duplicarlo. Quitar imagen actua inmediatamente sobre la imagen guardada.

## Archivos principales

- Services/ImagenesService.cs: validacion y almacenamiento compartidos.
- Controllers/ImagenesController.cs: lectura publica controlada.
- Controllers/CuentaController.cs y Views/Cuenta/Perfil.cshtml: avatar propio.
- Controllers/ProductosController.cs, Views/Productos/Index.cshtml y wwwroot/js/productos.js: carga por AJAX.
- Controllers/CatalogoController.cs y Views/Catalogo/Index.cshtml: muestra publica.

## Instalacion y respaldo

`dotnet restore` instala SkiaSharp y sus bibliotecas nativas para Windows, macOS y Linux. No se requiere instalar un editor de fotos ni configurar claves de licencia. No cambiamos las tablas: Avatar e Imagen ya estaban en las migraciones.

Git comparte el codigo; MySQL contiene las referencias y App_Data/imagenes contiene las fotos. Para trasladar los datos completos hay que respaldar ambas partes. La carpeta de imagenes se crea automaticamente al cargar una foto, queda excluida de Git y de la publicacion automatica, y necesita permisos de escritura. Al desplegar, conservar/restaurar esa carpeta y sus permisos.

Limites actuales: no hay recorte, redimensionado, edicion ni conversion automatica de la orientacion EXIF. Si una foto queda girada, exportarla ya orientada desde el editor antes de cargarla. Se conserva el tamaño en pixeles y CSS adapta su presentacion. Para mayor volumen se podria reemplazar el almacenamiento local por un servicio de archivos manteniendo los controladores y sus permisos.
