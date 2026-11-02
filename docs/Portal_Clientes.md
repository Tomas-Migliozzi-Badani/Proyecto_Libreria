# Portal de clientes: guia para la defensa

## Recorrido

Catalogo publico -> Seleccion -> Registro o login -> Confirmar reserva -> Mis reservas.

## Cliente, Usuario y Reserva

Cliente es la persona: DNI y contacto. Usuario es la cuenta: email, hash, rol y estado. Usuario.IdCliente vincula ambos de forma opcional y unica. El personal tiene cuenta sin cliente vinculado.

Reserva.IdCliente identifica para quien se reserva. Reserva.IdUsuario identifica quien la creo: el propio cliente en el portal o un empleado/administrador desde el ABM. Modificar una reserva no cambia su autor original.

## Registro

CuentaController recibe RegistroFormulario, valida datos y duplicados, crea Cliente y Usuario dentro de una transaccion y fija Rol=Cliente. El formulario no acepta privilegios. El indice unico de DNI y el de email protegen tambien frente a registros simultaneos. Los conflictos no vinculan automaticamente personas existentes. El administrador revisa y asocia cuentas antiguas desde Usuarios.

## Catalogo y seleccion

CatalogoController consulta productos activos de categorias activas. Skip/Take limita las filas en MySQL. Vue muestra el resultado y mantiene una seleccion de hasta 50 productos distintos. sessionStorage conserva la seleccion en la misma pestaña al ir al login. Ese almacenamiento no representa una reserva ni compromete stock.

## Confirmacion

MisReservasController obtiene el ID del usuario de la cookie y consulta su IdCliente en MySQL. Construye el formulario interno con esos datos y vencimiento a 48 horas. El servicio consulta los precios actuales y ejecuta la misma transaccion de stock que ya usaba el personal. No acepta precios, propietario o estado enviados por el navegador. La seleccion muestra un total estimado; el detalle confirmado muestra los valores guardados.

## Propiedad y permisos

Authorize(Roles=Cliente) controla el acceso al modulo, pero no alcanza por si solo. Las consultas ademas filtran por IdCliente de la sesion. La cancelacion comprueba el propietario dentro de la transaccion, despues de bloquear la reserva. Cambiar el ID de la URL no da acceso a reservas de otra persona.

## Limites actuales

No hay pagos ni envios. El vencimiento todavia no dispara cancelacion automatica. El cliente puede cancelar una reserva pendiente y volver a reservar; no edita directamente sus detalles confirmados. Avatar e imagenes se incorporan en la etapa de archivos.

## Prueba manual sugerida

1. Abrir Catalogo sin sesion y agregar un producto.
2. Registrarse con datos nuevos e iniciar sesion en la misma pestaña.
3. Confirmar la seleccion y consultar Mis reservas.
4. Revisar que el stock haya disminuido en Productos desde una cuenta del personal.
5. Cancelar la reserva desde el cliente y comprobar la devolucion de stock.
6. Con otra cuenta cliente, intentar consultar el ID de esa reserva: el servidor responde 404.
