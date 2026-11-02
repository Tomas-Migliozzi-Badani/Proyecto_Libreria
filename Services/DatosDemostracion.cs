using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;
using Proyecto_Libreria.Models;
namespace Proyecto_Libreria.Services;

public static class DatosDemostracion
{
    // Solo se invoca en Development y con DatosPrueba:Habilitar=true.
    // No cambia ni reactiva cuentas existentes.
    public static async Task CrearUsuarios(LibreriaContext context)
    {
        var hasher = new PasswordHasher<Usuario>();
        foreach (var rol in new[] { "Administrador", "Empleado", "Cliente" })
        {
            var email = rol == "Empleado" ? "empleado@test" : rol.ToLowerInvariant() + "@libreria.test";
            if (await context.Usuarios.AnyAsync(u => u.Email == email)) continue;
            var usuario = new Usuario { Nombre = rol, Apellido = "Demostracion", Email = email, Rol = rol, Estado = true };
            if (rol == "Cliente")
            {
                const string dniDemo = "99999991";
                if (await context.Clientes.AnyAsync(c => c.Dni == dniDemo))
                    throw new InvalidOperationException("El DNI de demostracion ya existe. Revisa la vinculacion antes de crear la cuenta demo.");
                usuario.Cliente = new Cliente { Nombre = "Cliente", Apellido = "Demostracion", Dni = dniDemo,
                    Email = email, Telefono = "0000000000" };
            }
            usuario.Password = hasher.HashPassword(usuario, rol is "Administrador" or "Empleado" ? "123" : "Libreria2026!");
            context.Usuarios.Add(usuario);
        }
        await context.SaveChangesAsync();
    }
}
