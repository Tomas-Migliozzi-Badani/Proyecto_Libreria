using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Filters;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.ViewModels;
namespace Proyecto_Libreria.Controllers.Api;

[ApiController]
[Route("api/productos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class ProductosApiController : ControllerBase
{
    private readonly LibreriaContext _context;
    public ProductosApiController(LibreriaContext context) => _context = context;
    private IQueryable<Producto> Visibles()
    {
        var consulta = _context.Productos.AsNoTracking();
        if (User.IsInRole("Cliente")) consulta = consulta.Where(p => p.Estado && p.Categoria.Estado);
        return consulta;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? buscar, [FromQuery] int pagina = 1)
    {
        var consulta = Visibles();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(p => p.Nombre.Contains(texto) || p.Marca.Contains(texto));
        }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d)); pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderBy(p => p.Nombre).ThenBy(p => p.IdProducto)
            .Skip((pagina - 1) * 10).Take(10)
            .Select(p => new
            {
                p.IdProducto,
                p.Nombre,
                p.Marca,
                p.Precio,
                p.Stock,
                p.Estado,
                p.Imagen,
                p.IdCategoria,
                categoria = p.Categoria.Nombre
            }).ToListAsync();
        return Ok(new { elementos, pagina, paginas, total });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener([FromRoute] int id)
    {
        var producto = await Visibles().Where(p => p.IdProducto == id)
            .Select(p => new
            {
                p.IdProducto,
                p.Nombre,
                p.Marca,
                p.Precio,
                p.Stock,
                p.Estado,
                p.Imagen,
                p.IdCategoria,
                categoria = p.Categoria.Nombre
            }).SingleOrDefaultAsync();
        return producto is null ? NotFound(new { mensaje = "El producto no existe." }) : Ok(producto);
    }

    [HttpPost, Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> Crear(ProductoFormulario formulario)
    {
        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == formulario.IdCategoria && c.Estado))
            return BadRequest(new { mensaje = "Selecciona una categoria activa." });
        var producto = new Producto();
        CopiarDatos(producto, formulario);
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = producto.IdProducto }, new { producto.IdProducto, mensaje = "Producto creado." });
    }

    [HttpPut("{id:int}"), Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> Editar(int id, ProductoFormulario formulario)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == formulario.IdCategoria
            && (c.Estado || c.IdCategoria == producto.IdCategoria)))
            return BadRequest(new { mensaje = "Selecciona una categoria activa." });
        CopiarDatos(producto, formulario);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return NotFound(new { mensaje = "El producto no existe." });
        producto.Estado = false; await _context.SaveChangesAsync();
        return NoContent();
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
}
