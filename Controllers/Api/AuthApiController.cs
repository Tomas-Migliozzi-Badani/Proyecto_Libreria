using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Filters;
using Proyecto_Libreria.Models;
using Proyecto_Libreria.Services;
using Proyecto_Libreria.ViewModels.Api;
namespace Proyecto_Libreria.Controllers.Api;

[ApiController]
[Route("api/auth")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class AuthApiController : ControllerBase
{
    private readonly LibreriaContext _context;
    private readonly JwtService _jwt;
    private readonly PasswordHasher<Usuario> _hasher = new();
    public AuthApiController(LibreriaContext context, JwtService jwt) { _context = context; _jwt = jwt; }

    [AllowAnonymous, HttpPost("login"), EnableRateLimiting("api-login")]
    public async Task<IActionResult> Login(LoginApiFormulario formulario)
    {
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
            Response.Headers.WWWAuthenticate = "Bearer";
            return Unauthorized(new { mensaje = "Email o contrasena incorrectos, o cuenta inactiva." });
        }
        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.Password = _hasher.HashPassword(usuario, formulario.Password);
            await _context.SaveChangesAsync();
        }
        Response.Headers.CacheControl = "no-store";
        return Ok(_jwt.CrearToken(usuario));
    }

    [HttpGet("me")]
    public async Task<IActionResult> MiCuenta()
    {
        var id = int.Parse(User.FindFirstValue("sub")!);
        return Ok(await _context.Usuarios.AsNoTracking().Where(u => u.IdUsuario == id)
            .Select(u => new { id = u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.Rol }).SingleAsync());
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var id = int.Parse(User.FindFirstValue("sub")!);
        await _context.Usuarios.Where(u => u.IdUsuario == id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.VersionToken, u => u.VersionToken + 1));
        return NoContent();
    }
}
