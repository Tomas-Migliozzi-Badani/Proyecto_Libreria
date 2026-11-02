using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.ViewModels;

namespace Proyecto_Libreria.Controllers;

// Este modulo se restringira por rol cuando incorporemos el login.
public class CategoriasController : Controller
{
    private readonly LibreriaContext _context;

    public CategoriasController(LibreriaContext context) => _context = context;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(string? buscar, int pagina = 1)
    {
        const int tamanio = 10;
        var consulta = _context.Categorias.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(c => c.Nombre.Contains(texto));
        }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanio));
        pagina = Math.Clamp(pagina, 1, paginas);
        // Skip y Take se traducen a SQL: no traemos todas las filas a memoria.
        var elementos = await consulta.OrderBy(c => c.Nombre).ThenBy(c => c.IdCategoria)
            .Skip((pagina - 1) * tamanio).Take(tamanio)
            .Select(c => new { c.IdCategoria, c.Nombre, c.Estado }).ToListAsync();
        return Json(new { elementos, total, pagina, paginas });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] CategoriaFormulario? formulario)
    {
        if (formulario is null) return BadRequest(new { mensaje = "Faltan los datos." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revisa el nombre: es obligatorio y admite hasta 100 caracteres." });
        var nombre = formulario.Nombre.Trim();
        if (await _context.Categorias.AnyAsync(c => c.Nombre == nombre))
            return Conflict(new { mensaje = "Ya existe una categoria con ese nombre." });
        var categoria = new Categoria { Nombre = nombre, Estado = formulario.Estado };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return Json(new { mensaje = "Categoria creada.", categoria.IdCategoria });
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [FromBody] CategoriaFormulario? formulario)
    {
        if (formulario is null) return BadRequest(new { mensaje = "Faltan los datos." });
        if (!ModelState.IsValid) return BadRequest(new { mensaje = "Revisa el nombre: es obligatorio y admite hasta 100 caracteres." });
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null) return NotFound(new { mensaje = "La categoria no existe." });
        var nombre = formulario.Nombre.Trim();
        if (await _context.Categorias.AnyAsync(c => c.IdCategoria != id && c.Nombre == nombre))
            return Conflict(new { mensaje = "Ya existe una categoria con ese nombre." });
        categoria.Nombre = nombre;
        categoria.Estado = formulario.Estado;
        await _context.SaveChangesAsync();
        return Json(new { mensaje = "Categoria actualizada." });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria is null) return NotFound(new { mensaje = "La categoria no existe." });
        categoria.Estado = false;
        await _context.SaveChangesAsync();
        return Json(new { mensaje = "Categoria desactivada. Sus productos se conservan." });
    }
}
