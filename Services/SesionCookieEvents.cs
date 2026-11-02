using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
namespace Proyecto_Libreria.Services;

public class SesionCookieEvents : CookieAuthenticationEvents
{
    private readonly LibreriaContext _context;
    public SesionCookieEvents(LibreriaContext context) => _context = context;

    // Solo viaja dentro de la cookie cifrada; un cambio de hash invalida sesiones anteriores.
    public static string VersionPassword(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        if (!int.TryParse(principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
        var usuario = await _context.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.IdUsuario == id);
        if (usuario is null || !usuario.Estado || usuario.Rol != principal!.FindFirstValue(ClaimTypes.Role)
            || usuario.Email != principal.FindFirstValue(ClaimTypes.Email)
            || VersionPassword(usuario.Password) != principal.FindFirstValue("VersionPassword"))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
        }
    }
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context) => Responder(context, 401);
    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context) => Responder(context, 403);

    private static Task Responder(RedirectContext<CookieAuthenticationOptions> context, int status)
    {
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = status;
            return context.Response.WriteAsJsonAsync(new { mensaje = status == 401 ? "La sesion vencio. Inicia sesion nuevamente." : "No tenes permiso para realizar esta accion." });
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }
}
