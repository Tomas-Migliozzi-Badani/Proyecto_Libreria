using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels;

public class ClienteFormulario
{
    [Required, StringLength(100)] public string Nombre { get; set; } = "";
    [Required, StringLength(100)] public string Apellido { get; set; } = "";
    [Required, RegularExpression(@"\d{7,9}")] public string Dni { get; set; } = "";
    [Required, StringLength(30)] public string Telefono { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
}
