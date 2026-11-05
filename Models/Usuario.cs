using System.ComponentModel.DataAnnotations;

namespace Proyecto_Libreria.Models;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    // Guarda el hash, no la contrasena original.
    public string Password { get; set; } = "";
    public string Rol { get; set; } = "";
    public bool Estado { get; set; }
    // Cambiar la contrasena o cerrar sesiones API invalida tokens anteriores.
    public int VersionToken { get; set; }
    public string Avatar { get; set; } = "";

    // Solo las cuentas de clientes necesitan esta vinculacion.
    public int? IdCliente { get; set; }
    public Cliente? Cliente { get; set; }
}
