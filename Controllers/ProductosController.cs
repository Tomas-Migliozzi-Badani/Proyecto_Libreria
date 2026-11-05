using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.Services;
using Proyecto_Libreria.ViewModels;

namespace Proyecto_Libreria.Controllers;

// Modulo de administracion para operadores autenticados.
[Authorize(Roles = "Administrador,Empleado")]
public class ProductosController : Controller
{
    private readonly LibreriaContext _context;
    private readonly ImagenesService _imagenes;
    public ProductosController(LibreriaContext context, ImagenesService imagenes) { _context = context; _imagenes = imagenes; }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(string? buscar, int pagina = 1)
    {
        const int tamanio = 10;
        var consulta = _context.Productos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(texto) || p.Marca.Contains(texto));
        }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanio));
        pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderBy(p => p.Nombre).ThenBy(p => p.IdProducto)
            .Skip((pagina - 1) * tamanio).Take(tamanio)
            .Select(p => new { p.IdProducto, p.Nombre, p.Marca, p.Precio, p.Stock,
                p.Estado, p.Imagen, p.IdCategoria, categoriaNombre = p.Categoria.Nombre }).ToListAsync();
        return Json(new { elementos, total, pagina, paginas });
    }

    // Busqueda acotada en el servidor, sin descargar todas las categorias.
    [HttpGet]
    public async Task<IActionResult> BuscarCategorias(string? buscar)
    {
        var consulta = _context.Categorias.AsNoTracking().Where(c => c.Estado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(c => c.Nombre.Contains(texto));
        }
        return Json(await consulta.OrderBy(c => c.Nombre).ThenBy(c => c.IdCategoria)
            .Take(10).Select(c => new { c.IdCategoria, c.Nombre }).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] ProductoFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid)
            return BadRequest(new { mensaje = "Revisa nombre, categoria, precio mayor a cero y stock no negativo." });
        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == formulario.IdCategoria && c.Estado))
            return BadRequest(new { mensaje = "Selecciona una categoria activa." });
        var producto = new Producto();
        CopiarDatos(producto, formulario);
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return Json(new { mensaje = "Producto creado.", producto.IdProducto });
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [FromBody] ProductoFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid)
            return BadRequest(new { mensaje = "Revisa nombre, categoria, precio mayor a cero y stock no negativo." });
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
        // Se puede conservar la categoria historica aunque haya sido desactivada.
        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == formulario.IdCategoria
            && (c.Estado || c.IdCategoria == producto.IdCategoria)))
            return BadRequest(new { mensaje = "Selecciona una categoria activa." });
        CopiarDatos(producto, formulario);
        await _context.SaveChangesAsync();
        return Json(new { mensaje = "Producto actualizado." });
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> SubirImagen(int id, IFormFile? archivo)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
        string nombre;
        try { nombre = await _imagenes.Guardar(archivo); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        var anterior = producto.Imagen; producto.Imagen = nombre;
        try { await _context.SaveChangesAsync(); }
        catch { _imagenes.Borrar(nombre); throw; }
        _imagenes.Borrar(anterior);
        return Json(new { mensaje = "Imagen actualizada.", imagen = nombre });
    }

    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarImagen(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
        var anterior = producto.Imagen; producto.Imagen = "";
        await _context.SaveChangesAsync(); _imagenes.Borrar(anterior);
        return Json(new { mensaje = "Imagen quitada." });
    }

    private static void CopiarDatos(Producto producto, ProductoFormulario formulario)
    {
        producto.Nombre = formulario.Nombre.Trim();
        producto.Marca = (formulario.Marca ?? "").Trim();
        producto.Precio = decimal.Round(formulario.Precio, 2, MidpointRounding.AwayFromZero);
        producto.Stock = formulario.Stock;
        producto.IdCategoria = formulario.IdCategoria;
        producto.Estado = formulario.Estado;
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
        producto.Estado = false;
        await _context.SaveChangesAsync();
        return Json(new { mensaje = "Producto desactivado. El historial se conserva." });
    }
}
