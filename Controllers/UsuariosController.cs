using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.ViewModels;
namespace Proyecto_Libreria.Controllers;

// Solo Administrador puede gestionar usuarios.
[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly LibreriaContext _context;
    private readonly PasswordHasher<Usuario> _hasher = new();
    public UsuariosController(LibreriaContext context) => _context = context;
    [HttpGet] public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(string? buscar, int pagina = 1)
    {
        var consulta = _context.Usuarios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(u => u.Nombre.Contains(texto) || u.Apellido.Contains(texto) || u.Email.Contains(texto));
        }
        var total = await consulta.CountAsync();
        var paginas = Math.Max(1, (int)Math.Ceiling(total / 10d));
        pagina = Math.Clamp(pagina, 1, paginas);
        // Nunca devolver el hash ni la contrasena al navegador.
        var elementos = await consulta.OrderBy(u => u.Apellido).ThenBy(u => u.IdUsuario)
            .Skip((pagina - 1) * 10).Take(10)
            .Select(u => new { id = u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.Rol, u.Estado, u.IdCliente,
                clienteNombre = u.Cliente == null ? "" : u.Cliente.Apellido + ", " + u.Cliente.Nombre }).ToListAsync();
        return Json(new { elementos, total, paginas, pagina });
    }
    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string? buscar)
    {
        var texto = (buscar ?? "").Trim();
        return Json(await _context.Clientes.AsNoTracking()
            .Where(c => c.Nombre.Contains(texto) || c.Apellido.Contains(texto) || c.Dni.Contains(texto))
            .OrderBy(c => c.Apellido).ThenBy(c => c.IdCliente).Take(10)
            .Select(c => new { id = c.IdCliente, nombre = c.Apellido + ", " + c.Nombre + " (" + c.Dni + ")" }).ToListAsync());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] UsuarioFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid || string.IsNullOrWhiteSpace(formulario.Password))
            return BadRequest(new { mensaje = "Revisa los datos. La contrasena debe tener entre 8 y 100 caracteres." });
        var usuario = new Usuario();
        return await Guardar(usuario, formulario, true);
    }
    [HttpPut, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [FromBody] UsuarioFormulario? formulario)
    {
        if (formulario is null || !ModelState.IsValid) return BadRequest(new { mensaje = "Revisa los datos y el rol. La nueva contrasena es opcional (8 a 100 caracteres)." });
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound(new { mensaje = "El usuario no existe." });
        return await Guardar(usuario, formulario, false);
    }
    private async Task<IActionResult> Guardar(Usuario usuario, UsuarioFormulario f, bool nuevo)
    {
        if (!nuevo && usuario.IdUsuario.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier)
            && (!f.Estado || f.Rol != "Administrador"))
            return BadRequest(new { mensaje = "No podes desactivar tu cuenta ni quitarte el rol de administrador." });
        var email = f.Email.Trim().ToLowerInvariant();
        if (await _context.Usuarios.AnyAsync(u => u.Email == email && u.IdUsuario != usuario.IdUsuario))
            return Conflict(new { mensaje = "El email ya esta registrado." });
        if (f.Rol == "Cliente")
        {
            if (f.IdCliente is null || !await _context.Clientes.AnyAsync(c => c.IdCliente == f.IdCliente))
                return BadRequest(new { mensaje = "Selecciona los datos del cliente que corresponden a esta cuenta." });
            if (await _context.Usuarios.AnyAsync(u => u.IdCliente == f.IdCliente && u.IdUsuario != usuario.IdUsuario))
                return Conflict(new { mensaje = "Ese cliente ya tiene otra cuenta vinculada." });
        }
        var nuevoCliente = f.Rol == "Cliente" ? f.IdCliente : null;
        if (!nuevo && (usuario.Rol != f.Rol || usuario.Estado != f.Estado
            || usuario.Email != email || usuario.IdCliente != nuevoCliente)) usuario.VersionToken++;
        usuario.IdCliente = nuevoCliente;
        usuario.Nombre = f.Nombre.Trim(); usuario.Apellido = f.Apellido.Trim(); usuario.Email = email;
        usuario.Rol = f.Rol; usuario.Estado = f.Estado;
        if (!string.IsNullOrEmpty(f.Password))
        {
            usuario.Password = _hasher.HashPassword(usuario, f.Password);
            usuario.VersionToken++;
        }
        if (nuevo) _context.Usuarios.Add(usuario);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { mensaje = "No se pudo guardar. Revisa si el email ya existe." }); }
        return Json(new { mensaje = nuevo ? "Usuario creado." : "Usuario actualizado.", id = usuario.IdUsuario });
    }
    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound(new { mensaje = "El usuario no existe." });
        if (usuario.IdUsuario.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier))
            return BadRequest(new { mensaje = "No podes desactivar tu propia cuenta." });
        usuario.Estado = false;
        usuario.VersionToken++;
        await _context.SaveChangesAsync();
        return Json(new { mensaje = "Usuario desactivado. Su historial se conserva." });
    }
}
