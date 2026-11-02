using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels;

public class LoginFormulario
{
    [Required(ErrorMessage = "Ingresa tu email."), EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Ingresa tu contrasena."), DataType(DataType.Password), StringLength(100)]
    public string Password { get; set; } = "";
    public bool Recordarme { get; set; }
    public string? ReturnUrl { get; set; }
}
