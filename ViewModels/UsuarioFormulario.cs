using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels;

public class UsuarioFormulario
{
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, StringLength(100)] public string Apellido { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [StringLength(100, MinimumLength = 8)] public string? Password { get; set; }
    [Required, RegularExpression("^(Administrador|Empleado|Cliente)$")]
    public string Rol { get; set; } = "Empleado";
    [Range(1, int.MaxValue)] public int? IdCliente { get; set; }
    public bool Estado { get; set; } = true;
}
