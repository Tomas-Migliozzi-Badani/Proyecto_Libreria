using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Proyecto_Libreria.Models;
namespace Proyecto_Libreria.Services;

// Genera el JWT con el mismo esquema de claims, firma y vencimiento de los apuntes.
public class JwtService
{
    private readonly IConfiguration _config;
    public JwtService(IConfiguration config) => _config = config;

    public object CrearToken(Usuario usuario)
    {
        var ahora = DateTime.UtcNow;
        var vencimiento = ahora.AddMinutes(30);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("name", usuario.Nombre + " " + usuario.Apellido),
            new("role", usuario.Rol),
            new("version", usuario.VersionToken.ToString(CultureInfo.InvariantCulture))
        };
        // No incluir passwords, hashes ni la clave secreta en el payload: no esta cifrado.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenAuthentication:SecretKey"]!));
        var token = new JwtSecurityToken(
            issuer: _config["TokenAuthentication:Issuer"], audience: _config["TokenAuthentication:Audience"],
            claims: claims, notBefore: ahora, expires: vencimiento,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new { accessToken = new JwtSecurityTokenHandler().WriteToken(token), tokenType = "Bearer",
            expiresAt = vencimiento, usuario = new { id = usuario.IdUsuario, usuario.Nombre, usuario.Rol } };
    }
}
