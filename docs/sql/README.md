# SQL de instalación y demostración

- **Libreria_Demo.sql:** archivo completo para abrir en MySQL Workbench y ejecutar una vez en una base nueva. Crea la base Libreria_Demo si no existe, selecciona esa base y aplica estructura y datos ficticios.
- **01_Estructura.sql:** estructura generada con dotnet ef migrations script a partir de las tres migraciones actuales. No selecciona una base por sí solo.
- **02_Datos_Demo.sql:** datos para tablas vacías después de aplicar la estructura o migraciones. Incluye hashes compatibles con PasswordHasher de ASP.NET.

Usar el archivo completo **o** el recorrido separado; no ejecutar ambos sobre las mismas tablas. Si Libreria_Demo ya contiene datos, seleccionar otro nombre de base en CREATE DATABASE y USE del archivo completo. Los scripts no contienen borrados ni intentan fusionar o reemplazar registros existentes.

No son idempotentes: volver a ejecutarlos produce errores por tablas/IDs existentes. No se deben ejecutar sobre la base de trabajo del estudiante. No desactivar foreign_key_checks para forzar una importación.

Contenido inicial: 4 categorías, 16 productos, 12 clientes, 3 cuentas, 12 reservas y 16 detalles. Una reserva pendiente tiene cinco detalles. Su stock ya está descontado en los productos. Los datos personales son ficticios y los emails usan dominios de demostración.

Las tres credenciales se encuentran en el README principal. Son públicas de evaluación. No hay claves JWT ni credenciales reales de MySQL en estos archivos.

Las imágenes no se insertan como BLOB ni se copian desde archivos personales. Las referencias iniciales están vacías. Hay dos PNG ficticios en ../datos-demo para probar las cargas desde la web.

Se comprobó la importación completa y que EF no intentara reaplicar las migraciones. Ver ../Verificacion_Entrega.md.
