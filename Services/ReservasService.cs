using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.ViewModels;
namespace Proyecto_Libreria.Services;

// Una reserva y sus movimientos de stock se confirman juntos o se deshacen juntos.
public class ReservasService
{
    private readonly LibreriaContext _context;
    public ReservasService(LibreriaContext context) => _context = context;

    public async Task<int> Guardar(int? id, ReservaFormulario formulario, int autorId, bool desdePortal = false)
    {
        if (formulario.FechaVencimiento <= DateTime.Now)
            throw new ArgumentException("El vencimiento debe ser posterior al momento actual.");
        if (formulario.Detalles.Select(d => d.IdProducto).Distinct().Count() != formulario.Detalles.Count)
            throw new ArgumentException("Cada producto debe aparecer una sola vez; ajusta su cantidad.");
        await using var transaction = await _context.Database.BeginTransactionAsync();
        Reserva reserva;
        var anteriores = new List<DetalleReserva>();
        if (id.HasValue)
        {
            // Bloquea solo esta reserva hasta terminar la transaccion, incluso si no cambia el estado.
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Reservas SET EstadoReserva = EstadoReserva WHERE IdReserva = {id.Value}");
            reserva = await _context.Reservas.FindAsync(id.Value)
                ?? throw new ArgumentException("La reserva no existe.");
            if (reserva.EstadoReserva != "Pendiente")
                throw new ArgumentException("Solo se pueden modificar reservas pendientes.");
            anteriores = await _context.DetallesReserva.AsNoTracking().Where(d => d.IdReserva == id.Value).ToListAsync();
        }
        else reserva = new Reserva { FechaReserva = DateTime.Now, EstadoReserva = "Pendiente" };

        if (!await _context.Clientes.AnyAsync(c => c.IdCliente == formulario.IdCliente))
            throw new ArgumentException("Selecciona un cliente existente.");
        var autor = await _context.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.IdUsuario == autorId && u.Estado);
        if (autor is null || (desdePortal
                ? autor.Rol != "Cliente" || autor.IdCliente != formulario.IdCliente || id.HasValue
                : autor.Rol != "Administrador" && autor.Rol != "Empleado"))
            throw new ArgumentException("La cuenta no tiene permiso para registrar esta reserva.");

        // Bloquear productos siempre por ID reduce la posibilidad de interbloqueos.
        var ids = formulario.Detalles.Select(d => d.IdProducto).Concat(anteriores.Select(d => d.IdProducto)).Distinct().Order().ToList();
        foreach (var productoId in ids)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Productos SET Stock = Stock WHERE IdProducto = {productoId}");
        }
        foreach (var detalle in anteriores)
            await _context.Productos.Where(p => p.IdProducto == detalle.IdProducto)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock + detalle.Cantidad));

        var nuevos = new List<DetalleReserva>();
        foreach (var linea in formulario.Detalles.OrderBy(d => d.IdProducto))
        {
            var producto = await _context.Productos.AsNoTracking().SingleOrDefaultAsync(p => p.IdProducto == linea.IdProducto);
            if (producto is null || !producto.Estado || (desdePortal && !await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria && c.Estado)))
                throw new ArgumentException("Uno de los productos no existe o esta inactivo.");
            // El control se ejecuta en SQL bajo bloqueo: dos reservas no pueden consumir el mismo stock.
            var cambiado = await _context.Productos.Where(p => p.IdProducto == linea.IdProducto && p.Stock >= linea.Cantidad)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock - linea.Cantidad));
            if (cambiado == 0) throw new ArgumentException($"Stock insuficiente para {producto.Nombre}.");
            var precioAnterior = anteriores.FirstOrDefault(d => d.IdProducto == linea.IdProducto)?.PrecioUnitario;
            nuevos.Add(new DetalleReserva { IdProducto = linea.IdProducto, Cantidad = linea.Cantidad,
                PrecioUnitario = precioAnterior ?? producto.Precio, Reserva = reserva });
        }
        reserva.IdCliente = formulario.IdCliente;
        if (!id.HasValue) reserva.IdUsuario = autorId;
        reserva.FechaVencimiento = formulario.FechaVencimiento;
        if (!id.HasValue) _context.Reservas.Add(reserva);
        else await _context.DetallesReserva.Where(d => d.IdReserva == id.Value).ExecuteDeleteAsync();
        _context.DetallesReserva.AddRange(nuevos);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return reserva.IdReserva;
    }

    public async Task Cancelar(int id, int? clienteId = null)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Reservas SET EstadoReserva = EstadoReserva WHERE IdReserva = {id}");
        var reserva = await _context.Reservas.FindAsync(id) ?? throw new KeyNotFoundException("La reserva no existe.");
        if (clienteId.HasValue && reserva.IdCliente != clienteId.Value)
            throw new KeyNotFoundException("La reserva no existe.");
        // Repetir la cancelacion no devuelve el stock una segunda vez.
        if (reserva.EstadoReserva == "Cancelada") return;
        if (reserva.EstadoReserva != "Pendiente") throw new ArgumentException("Solo se pueden cancelar reservas pendientes.");
        var detalles = await _context.DetallesReserva.AsNoTracking().Where(d => d.IdReserva == id).OrderBy(d => d.IdProducto).ToListAsync();
        foreach (var detalle in detalles)
            await _context.Productos.Where(p => p.IdProducto == detalle.IdProducto)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock + detalle.Cantidad));
        reserva.EstadoReserva = "Cancelada";
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
