using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
namespace Proyecto_Libreria.Controllers;

[AllowAnonymous]
public class CatalogoController : Controller
{
    private readonly LibreriaContext _context;
    public CatalogoController(LibreriaContext context) => _context = context;
    [HttpGet] public IActionResult Index() => View();
    [HttpGet]
    public async Task<IActionResult> Listar(string? buscar, int pagina = 1)
    {
        var consulta = _context.Productos.AsNoTracking().Where(p => p.Estado && p.Categoria.Estado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(texto) || p.Marca.Contains(texto));
        }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 9d)); pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderBy(p => p.Nombre).ThenBy(p => p.IdProducto)
            .Skip((pagina - 1) * 9).Take(9)
            .Select(p => new { p.IdProducto, p.Nombre, p.Marca, p.Precio, p.Stock, p.Imagen, categoria = p.Categoria.Nombre }).ToListAsync();
        return Json(new { elementos, total, paginas, pagina });
    }
}
