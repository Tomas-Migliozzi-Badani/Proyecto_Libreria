using System.ComponentModel.DataAnnotations;

namespace Proyecto_Libreria.Models;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    // Guarda el hash de la contrasena; el login implementara el hashing.
    public string Password { get; set; } = "";
    public string Rol { get; set; } = "";
    public bool Estado { get; set; }
    public string Avatar { get; set; } = "";
}
