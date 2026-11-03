using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels.Api;
public class LoginApiFormulario
{
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(100)] public string Password { get; set; } = "";
}
