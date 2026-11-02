using System.ComponentModel.DataAnnotations;

namespace Proyecto_Libreria.ViewModels;

// Recibimos solo los campos que el formulario permite editar.
public class CategoriaFormulario
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre admite hasta 100 caracteres.")]
    public string Nombre { get; set; } = "";
    public bool Estado { get; set; } = true;
}
