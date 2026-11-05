using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.ViewModels;
namespace Proyecto_Libreria.Controllers;

[Authorize(Roles = "Administrador,Empleado")]
public class ClientesController : Controller
{
    private readonly LibreriaContext _context;
    public ClientesController(LibreriaContext context) => _context = context;
    [HttpGet] public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(string? buscar, int pagina = 1)
    {
        var consulta = _context.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(c => c.Nombre.Contains(texto) || c.Apellido.Contains(texto) || c.Dni.Contains(texto));
        }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d));
        pagina = Math.Clamp(pagina, 1, paginas);
        var elementos = await consulta.OrderBy(c => c.Apellido).ThenBy(c => c.IdCliente)
            .Skip((pagina - 1) * 10).Take(10)
            .Select(c => new { id = c.IdCliente, c.Nombre, c.Apellido, c.Dni, c.Telefono, c.Email }).ToListAsync();
        return Json(new { elementos, total, paginas, pagina });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] ClienteFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid) return BadRequest(new { mensaje = "Revisa nombre, apellido, DNI (7 a 9 digitos), telefono y email." });
        if (await _context.Clientes.AnyAsync(c => c.Dni == formulario.Dni)) return Conflict(new { mensaje = "Ya existe un cliente con ese DNI." });
        var cliente = new Cliente();
        CopiarDatos(cliente, formulario);
        _context.Clientes.Add(cliente);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "El DNI ya esta registrado o los datos cambiaron. Revisa el cliente." }); }
        return Json(new { mensaje = "Cliente creado.", id = cliente.IdCliente });
    }
    [HttpPut, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [FromBody] ClienteFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid) return BadRequest(new { mensaje = "Revisa nombre, apellido, DNI (7 a 9 digitos), telefono y email." });
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound(new { mensaje = "El cliente no existe." });
        if (await _context.Clientes.AnyAsync(c => c.IdCliente != id && c.Dni == formulario.Dni)) return Conflict(new { mensaje = "Ya existe un cliente con ese DNI." });
        CopiarDatos(cliente, formulario);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "El DNI ya esta registrado o los datos cambiaron. Revisa el cliente." }); }
        return Json(new { mensaje = "Cliente actualizado." });
    }
    private static void CopiarDatos(Cliente cliente, ClienteFormulario formulario)
    {
        cliente.Nombre = formulario.Nombre.Trim();
        cliente.Apellido = formulario.Apellido.Trim();
        cliente.Dni = formulario.Dni;
        cliente.Telefono = formulario.Telefono.Trim();
        cliente.Email = formulario.Email.Trim().ToLowerInvariant();
    }
    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound(new { mensaje = "El cliente no existe." });
        if (await _context.Reservas.AnyAsync(r => r.IdCliente == id))
            return Conflict(new { mensaje = "El cliente tiene reservas. Se conserva para mantener el historial." });
        _context.Clientes.Remove(cliente);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "No se puede eliminar un cliente relacionado con otros registros." }); }
        return Json(new { mensaje = "Cliente eliminado." });
    }
}
