using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels.Api;
public class ReservaApiFormulario
{
    // Solo el personal usa estos dos campos; el cliente los obtiene de su cuenta y del servidor.
    [Range(1, int.MaxValue)] public int? IdCliente { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    [Required, MinLength(1), MaxLength(50)] public List<LineaReservaFormulario> Detalles { get; set; } = [];
}
