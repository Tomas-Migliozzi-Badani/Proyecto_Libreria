using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels;

// El cliente no elige ID de cliente, autor, estado, precios ni vencimiento.
public class ReservaPublicaFormulario
{
    [Required, MinLength(1), MaxLength(50)]
    public List<LineaReservaFormulario> Detalles { get; set; } = [];
}
