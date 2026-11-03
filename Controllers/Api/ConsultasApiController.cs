using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Filters;
namespace Proyecto_Libreria.Controllers.Api;

[ApiController]
[Route("api")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class ConsultasApiController : ControllerBase
{
    private readonly LibreriaContext _context;
    public ConsultasApiController(LibreriaContext context) => _context = context;

    [HttpGet("categorias")]
    public async Task<IActionResult> Categorias([FromQuery] string? buscar, [FromQuery] int pagina = 1)
    {
        var consulta = _context.Categorias.AsNoTracking();
        if (User.IsInRole("Cliente")) consulta = consulta.Where(c => c.Estado);
        if (!string.IsNullOrWhiteSpace(buscar)) { var texto = buscar.Trim(); consulta = consulta.Where(c => c.Nombre.Contains(texto)); }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d)); pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderBy(c => c.Nombre).ThenBy(c => c.IdCategoria).Skip((pagina - 1) * 10).Take(10)
            .Select(c => new { c.IdCategoria, c.Nombre, c.Estado }).ToListAsync();
        return Ok(new { elementos, pagina, paginas, total });
    }

    [HttpGet("clientes"), Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> Clientes([FromQuery] string? buscar, [FromQuery] int pagina = 1)
    {
        var consulta = _context.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar)) { var texto = buscar.Trim(); consulta = consulta.Where(c => c.Nombre.Contains(texto) || c.Apellido.Contains(texto) || c.Dni.Contains(texto)); }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d)); pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderBy(c => c.Apellido).ThenBy(c => c.IdCliente).Skip((pagina - 1) * 10).Take(10)
            .Select(c => new { c.IdCliente, c.Nombre, c.Apellido, c.Dni }).ToListAsync();
        return Ok(new { elementos, pagina, paginas, total });
    }
}
