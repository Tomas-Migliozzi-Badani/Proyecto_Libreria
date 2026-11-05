using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.Services;
using Proyecto_Libreria.ViewModels;
namespace Proyecto_Libreria.Controllers;

[Authorize]
public class CuentaController : Controller
{
    private readonly LibreriaContext _context;
    private readonly PasswordHasher<Usuario> _hasher = new();
    private readonly ImagenesService _imagenes;
    public CuentaController(LibreriaContext context, ImagenesService imagenes) { _context = context; _imagenes = imagenes; }

    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginFormulario { ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null });
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginFormulario formulario)
    {
        if (!ModelState.IsValid) return View(formulario);
        var email = formulario.Email.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == email);
        var resultado = PasswordVerificationResult.Failed;
        if (usuario is not null && usuario.Estado)
        {
            try { resultado = _hasher.VerifyHashedPassword(usuario, usuario.Password, formulario.Password); }
            catch (FormatException) { resultado = PasswordVerificationResult.Failed; }
        }
        if (usuario is null || !usuario.Estado || resultado == PasswordVerificationResult.Failed
            || !new[] { "Administrador", "Empleado", "Cliente" }.Contains(usuario.Rol))
        {
            ModelState.AddModelError("", "Email o contrasena incorrectos, o cuenta inactiva.");
            return View(formulario);
        }
        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.Password = _hasher.HashPassword(usuario, formulario.Password);
            await _context.SaveChangesAsync();
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.Nombre + " " + usuario.Apellido),
            new(ClaimTypes.Email, usuario.Email), new(ClaimTypes.Role, usuario.Rol),
            new("VersionPassword", SesionCookieEvents.VersionPassword(usuario.Password))
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = formulario.Recordarme });
        if (Url.IsLocalUrl(formulario.ReturnUrl)) return LocalRedirect(formulario.ReturnUrl!);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Registro()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Catalogo");
        return View(new RegistroFormulario());
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("registro")]
    public async Task<IActionResult> Registro(RegistroFormulario formulario)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Catalogo");
        if (!ModelState.IsValid) return View(formulario);
        var email = formulario.Email.Trim().ToLowerInvariant();
        if (await _context.Usuarios.AnyAsync(u => u.Email == email)
            || await _context.Clientes.AnyAsync(c => c.Dni == formulario.Dni || c.Email == email))
        {
            ModelState.AddModelError("", "El email o DNI ya esta registrado. Inicia sesion o consulta al personal para vincular tu cuenta; no creamos un vinculo automatico.");
            return View(formulario);
        }
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var cliente = new Cliente { Nombre = formulario.Nombre.Trim(), Apellido = formulario.Apellido.Trim(),
            Dni = formulario.Dni, Telefono = formulario.Telefono.Trim(), Email = email };
        var usuario = new Usuario { Nombre = cliente.Nombre, Apellido = cliente.Apellido, Email = email,
            Rol = "Cliente", Estado = true, Cliente = cliente };
        usuario.Password = _hasher.HashPassword(usuario, formulario.Password);
        _context.Usuarios.Add(usuario);
        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "El email o DNI ya fue registrado. Revisa tus datos o consulta al personal.");
            return View(formulario);
        }
        TempData["RegistroExitoso"] = "Tu cuenta fue creada. Ya podes iniciar sesion y reservar.";
        return RedirectToAction(nameof(Login), new { returnUrl = Url.Action("Index", "Catalogo") });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await _context.Usuarios.AsNoTracking().Where(u => u.IdUsuario == id)
            .Select(u => new PerfilVista { Avatar = u.Avatar, Vinculado = u.IdCliente != null, Nombre = u.Nombre, Apellido = u.Apellido, Email = u.Email, Rol = u.Rol }).SingleAsync();
        return View(usuario);
    }
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> Avatar(IFormFile? archivo)
    {
        // El propietario se obtiene de la sesion, nunca de un ID del formulario.
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await _context.Usuarios.SingleAsync(u => u.IdUsuario == id);
        string nombre;
        try { nombre = await _imagenes.Guardar(archivo); }
        catch (ArgumentException ex) { TempData["ErrorAvatar"] = ex.Message; return RedirectToAction(nameof(Perfil)); }
        var anterior = usuario.Avatar;
        usuario.Avatar = nombre;
        try { await _context.SaveChangesAsync(); }
        catch { _imagenes.Borrar(nombre); throw; }
        _imagenes.Borrar(anterior);
        TempData["AvatarExitoso"] = "Tu avatar fue actualizado.";
        return RedirectToAction(nameof(Perfil));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarAvatar()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await _context.Usuarios.SingleAsync(u => u.IdUsuario == id);
        var anterior = usuario.Avatar; usuario.Avatar = "";
        await _context.SaveChangesAsync(); _imagenes.Borrar(anterior);
        return RedirectToAction(nameof(Perfil));
    }

    [AllowAnonymous, HttpGet]
    public IActionResult AccesoDenegado() { Response.StatusCode = 403; return View(); }
}
