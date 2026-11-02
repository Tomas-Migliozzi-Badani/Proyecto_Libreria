using System.ComponentModel.DataAnnotations;
namespace Proyecto_Libreria.ViewModels;

public class ReservaFormulario
{
    [Range(1, int.MaxValue)] public int IdCliente { get; set; }
    public DateTime FechaVencimiento { get; set; }
    [Required, MinLength(1), MaxLength(50)] public List<LineaReservaFormulario> Detalles { get; set; } = [];
}
public class LineaReservaFormulario
{
    [Range(1, int.MaxValue)] public int IdProducto { get; set; }
    [Range(1, 1000000)] public int Cantidad { get; set; }
}
