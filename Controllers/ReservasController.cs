using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Services;
using Proyecto_Libreria.ViewModels;
namespace Proyecto_Libreria.Controllers;

[Authorize(Roles = "Administrador,Empleado")]
public class ReservasController : Controller
{
    private readonly LibreriaContext _context;
    private readonly ReservasService _reservas;
    public ReservasController(LibreriaContext context, ReservasService reservas) { _context = context; _reservas = reservas; }
    [HttpGet] public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(string? buscar, int pagina = 1)
    {
        var consulta = _context.Reservas.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(r => r.Cliente.Nombre.Contains(texto) || r.Cliente.Apellido.Contains(texto) || r.EstadoReserva.Contains(texto));
        }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d)); pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderByDescending(r => r.IdReserva).Skip((pagina - 1) * 10).Take(10)
            .Select(r => new { r.IdReserva, r.FechaReserva, r.FechaVencimiento, r.EstadoReserva,
                cliente = r.Cliente.Apellido + ", " + r.Cliente.Nombre,
                usuario = r.Usuario.Nombre, total = _context.DetallesReserva.Where(d => d.IdReserva == r.IdReserva)
                    .Sum(d => (decimal?)(d.Cantidad * d.PrecioUnitario)) ?? 0 }).ToListAsync();
        return Json(new { elementos, pagina, paginas, total });
    }
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var reserva = await _context.Reservas.AsNoTracking().Where(r => r.IdReserva == id)
            .Select(r => new { r.IdReserva, r.IdCliente, cliente = r.Cliente.Apellido + ", " + r.Cliente.Nombre,
                r.IdUsuario, usuario = r.Usuario.Nombre + " " + r.Usuario.Apellido, r.FechaVencimiento, r.EstadoReserva }).SingleOrDefaultAsync();
        if (reserva is null) return NotFound(new { mensaje = "La reserva no existe." });
        var detalles = await _context.DetallesReserva.AsNoTracking().Where(d => d.IdReserva == id)
            .Select(d => new { d.IdProducto, nombre = d.Producto.Nombre, d.Cantidad, d.PrecioUnitario }).ToListAsync();
        return Json(new { reserva, detalles });
    }
    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string? buscar)
    {
        var texto = (buscar ?? "").Trim();
        return Json(await _context.Clientes.AsNoTracking().Where(c => c.Nombre.Contains(texto) || c.Apellido.Contains(texto) || c.Dni.Contains(texto))
            .OrderBy(c => c.Apellido).ThenBy(c => c.IdCliente).Take(10)
            .Select(c => new { id = c.IdCliente, nombre = c.Apellido + ", " + c.Nombre + " (" + c.Dni + ")" }).ToListAsync());
    }
    [HttpGet]
    public async Task<IActionResult> BuscarProductos(string? buscar)
    {
        var texto = (buscar ?? "").Trim();
        return Json(await _context.Productos.AsNoTracking().Where(p => p.Estado && (p.Nombre.Contains(texto) || p.Marca.Contains(texto)))
            .OrderBy(p => p.Nombre).ThenBy(p => p.IdProducto).Take(10)
            .Select(p => new { id = p.IdProducto, p.Nombre, p.Precio, p.Stock }).ToListAsync());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Crear([FromBody] ReservaFormulario? formulario) => Guardar(null, formulario);
    [HttpPut, ValidateAntiForgeryToken]
    public Task<IActionResult> Editar(int id, [FromBody] ReservaFormulario? formulario) => Guardar(id, formulario);
    private async Task<IActionResult> Guardar(int? id, ReservaFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid || formulario.Detalles.Any(d => d is null))
            return BadRequest(new { mensaje = "Revisa cliente, vencimiento y al menos un producto con cantidad positiva." });
        try
        {
            var reservaId = await _reservas.Guardar(id, formulario, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
            return Json(new { mensaje = "Reserva guardada y stock actualizado.", id = reservaId });
        }
        catch (ArgumentException e) { return BadRequest(new { mensaje = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "Los datos cambiaron. Actualiza la pantalla y vuelve a intentar." }); }
        catch (MySqlException) { return Conflict(new { mensaje = "Otra operacion modifico el stock. Actualiza y vuelve a intentar." }); }
    }
    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        try { await _reservas.Cancelar(id); return Json(new { mensaje = "Reserva cancelada. Stock devuelto." }); }
        catch (KeyNotFoundException) { return NotFound(new { mensaje = "La reserva no existe." }); }
        catch (ArgumentException e) { return BadRequest(new { mensaje = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "No se pudo cancelar. Actualiza y vuelve a intentar." }); }
        catch (MySqlException) { return Conflict(new { mensaje = "Otra operacion esta modificando la reserva. Vuelve a intentar." }); }
    }
}
