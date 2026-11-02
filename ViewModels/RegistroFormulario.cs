using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels;

public class RegistroFormulario
{
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, StringLength(100)] public string Apellido { get; set; } = "";
    [Required, RegularExpression(@"\d{7,9}", ErrorMessage = "El DNI debe tener entre 7 y 9 digitos, sin puntos.")]
    public string Dni { get; set; } = "";
    [Required, StringLength(30)] public string Telefono { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    [Required, Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden."), DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = "";
}
