using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Services;
using Proyecto_Libreria.ViewModels;
namespace Proyecto_Libreria.Controllers;

[Authorize(Roles = "Cliente")]
public class MisReservasController : Controller
{
    private readonly LibreriaContext _context;
    private readonly ReservasService _reservas;
    public MisReservasController(LibreriaContext context, ReservasService reservas) { _context = context; _reservas = reservas; }
    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Task<int?> ClienteId() => _context.Usuarios.Where(u => u.IdUsuario == UsuarioId && u.Estado && u.Rol == "Cliente")
        .Select(u => u.IdCliente).SingleOrDefaultAsync();
    private IActionResult SinVinculo() => Conflict(new { mensaje = "Tu cuenta todavia no esta vinculada a un cliente. Solicita al administrador que revise la vinculacion en Usuarios." });
    [HttpGet] public IActionResult Index() => View();
    [HttpGet]
    public async Task<IActionResult> Listar(int pagina = 1)
    {
        var clienteId = await ClienteId();
        if (clienteId is null) return SinVinculo();
        var consulta = _context.Reservas.AsNoTracking().Where(r => r.IdCliente == clienteId);
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d)); pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderByDescending(r => r.IdReserva).Skip((pagina - 1) * 10).Take(10)
            .Select(r => new { r.IdReserva, r.FechaReserva, r.FechaVencimiento, r.EstadoReserva,
                total = _context.DetallesReserva.Where(d => d.IdReserva == r.IdReserva).Sum(d => (decimal?)(d.Cantidad * d.PrecioUnitario)) ?? 0 }).ToListAsync();
        return Json(new { elementos, total, pagina, paginas });
    }
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var clienteId = await ClienteId();
        if (clienteId is null) return SinVinculo();
        var reserva = await _context.Reservas.AsNoTracking().Where(r => r.IdReserva == id && r.IdCliente == clienteId)
            .Select(r => new { r.IdReserva, r.FechaReserva, r.FechaVencimiento, r.EstadoReserva }).SingleOrDefaultAsync();
        if (reserva is null) return NotFound(new { mensaje = "La reserva no existe." });
        var detalles = await _context.DetallesReserva.AsNoTracking().Where(d => d.IdReserva == id)
            .Select(d => new { producto = d.Producto.Nombre, d.Cantidad, d.PrecioUnitario, subtotal = d.Cantidad * d.PrecioUnitario }).ToListAsync();
        return Json(new { reserva, detalles });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] ReservaPublicaFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid || formulario.Detalles.Any(d => d is null))
            return BadRequest(new { mensaje = "Agrega entre 1 y 50 productos, con cantidades enteras positivas." });
        var clienteId = await ClienteId();
        if (clienteId is null) return SinVinculo();
        var datos = new ReservaFormulario { IdCliente = clienteId.Value,
            FechaVencimiento = DateTime.Now.AddDays(2), Detalles = formulario.Detalles };
        try
        {
            var id = await _reservas.Guardar(null, datos, UsuarioId, desdePortal: true);
            return Json(new { mensaje = "Reserva confirmada. Tenes 48 horas para coordinar el retiro en la libreria.", id });
        }
        catch (ArgumentException e) { return BadRequest(new { mensaje = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "Cambio la disponibilidad. Actualiza el catalogo e intenta nuevamente." }); }
        catch (MySqlException) { return Conflict(new { mensaje = "Otra reserva esta modificando el stock. Intenta nuevamente." }); }
    }
    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var clienteId = await ClienteId();
        if (clienteId is null) return SinVinculo();
        try { await _reservas.Cancelar(id, clienteId.Value); return Json(new { mensaje = "Reserva cancelada. El stock fue devuelto." }); }
        catch (KeyNotFoundException) { return NotFound(new { mensaje = "La reserva no existe." }); }
        catch (ArgumentException e) { return BadRequest(new { mensaje = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "No se pudo cancelar. Actualiza e intenta nuevamente." }); }
        catch (MySqlException) { return Conflict(new { mensaje = "Otra operacion esta modificando la reserva. Intenta nuevamente." }); }
    }
}
