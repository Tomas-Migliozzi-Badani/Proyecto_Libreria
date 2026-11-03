using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
namespace Proyecto_Libreria.Services;

public class JwtUsuarioEvents : JwtBearerEvents
{
    private readonly LibreriaContext _context;
    public JwtUsuarioEvents(LibreriaContext context) => _context = context;
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        if (!int.TryParse(context.Principal?.FindFirstValue("sub"), out var id)
            || !int.TryParse(context.Principal?.FindFirstValue("version"), out var version))
        {
            context.Fail("Identidad invalida."); return;
        }
        var usuario = await _context.Usuarios.AsNoTracking().SingleOrDefaultAsync(u => u.IdUsuario == id);
        if (usuario is null || !usuario.Estado || usuario.VersionToken != version
            || usuario.Rol != context.Principal!.FindFirstValue("role"))
            context.Fail("El usuario o sus permisos cambiaron. Solicita un nuevo token.");
    }
}
