using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Filters;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.Services;
using Proyecto_Libreria.ViewModels;
using Proyecto_Libreria.ViewModels.Api;
namespace Proyecto_Libreria.Controllers.Api;

[ApiController]
[Route("api/reservas")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class ReservasApiController : ControllerBase
{
    private readonly LibreriaContext _context;
    private readonly ReservasService _reservas;
    public ReservasApiController(LibreriaContext context, ReservasService reservas) { _context = context; _reservas = reservas; }
    private int UsuarioId => int.Parse(User.FindFirstValue("sub")!);
    private async Task<int> ClienteActual()
    {
        var id = await _context.Usuarios.Where(u => u.IdUsuario == UsuarioId).Select(u => u.IdCliente).SingleAsync();
        return id ?? throw new ArgumentException("Tu cuenta no tiene un cliente vinculado. Solicita al administrador que lo revise.");
    }
    private async Task<IQueryable<Reserva>> Visibles()
    {
        var consulta = _context.Reservas.AsNoTracking();
        if (User.IsInRole("Cliente"))
        {
            var clienteId = await ClienteActual();
            consulta = consulta.Where(r => r.IdCliente == clienteId);
        }
        return consulta;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1)
    {
        var consulta = await Visibles();
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d)); pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderByDescending(r => r.IdReserva).Skip((pagina - 1) * 10).Take(10)
            .Select(r => new { r.IdReserva, r.IdCliente, r.FechaReserva, r.FechaVencimiento, r.EstadoReserva,
                total = _context.DetallesReserva.Where(d => d.IdReserva == r.IdReserva)
                    .Sum(d => (decimal?)(d.Cantidad * d.PrecioUnitario)) ?? 0 }).ToListAsync();
        return Ok(new { elementos, pagina, paginas, total });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var visibles = await Visibles();
        var reserva = await visibles.Where(r => r.IdReserva == id)
            .Select(r => new { r.IdReserva, r.IdCliente, r.IdUsuario, r.FechaReserva, r.FechaVencimiento, r.EstadoReserva }).SingleOrDefaultAsync();
        if (reserva is null) return NotFound(new { mensaje = "La reserva no existe." });
        var detalles = await _context.DetallesReserva.AsNoTracking().Where(d => d.IdReserva == id)
            .Select(d => new { d.IdProducto, producto = d.Producto.Nombre, d.Cantidad, d.PrecioUnitario,
                subtotal = d.Cantidad * d.PrecioUnitario }).ToListAsync();
        return Ok(new { reserva, detalles, total = detalles.Sum(d => d.subtotal) });
    }

    [HttpPost]
    public async Task<IActionResult> Crear(ReservaApiFormulario formulario)
    {
        if (formulario.Detalles.Any(d => d is null)) return BadRequest(new { mensaje = "Revisa los productos de la reserva." });
        var cliente = User.IsInRole("Cliente");
        var idCliente = cliente ? await ClienteActual() : formulario.IdCliente
            ?? throw new ArgumentException("El personal debe indicar IdCliente.");
        var datos = new ReservaFormulario { IdCliente = idCliente, Detalles = formulario.Detalles,
            FechaVencimiento = cliente ? DateTime.Now.AddDays(2) : formulario.FechaVencimiento ?? DateTime.Now.AddDays(2) };
        var id = await _reservas.Guardar(null, datos, UsuarioId, desdePortal: cliente);
        return CreatedAtAction(nameof(Obtener), new { id }, new { id, mensaje = "Reserva creada y stock actualizado." });
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> Editar(int id, ReservaApiFormulario formulario)
    {
        if (formulario.Detalles.Any(d => d is null)) return BadRequest(new { mensaje = "Revisa los productos de la reserva." });
        if (!await _context.Reservas.AnyAsync(r => r.IdReserva == id)) return NotFound(new { mensaje = "La reserva no existe." });
        var datos = new ReservaFormulario { IdCliente = formulario.IdCliente ?? throw new ArgumentException("Indica IdCliente."),
            Detalles = formulario.Detalles, FechaVencimiento = formulario.FechaVencimiento ?? DateTime.Now.AddDays(2) };
        await _reservas.Guardar(id, datos, UsuarioId);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var clienteId = User.IsInRole("Cliente") ? await ClienteActual() : (int?)null;
        await _reservas.Cancelar(id, clienteId);
        return NoContent();
    }
}
