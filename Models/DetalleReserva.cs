using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Libreria.Models;

public class DetalleReserva
{
    [Key]
    public int IdDetalleReserva { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public int IdReserva { get; set; }
    [ForeignKey(nameof(IdReserva))]
    public Reserva Reserva { get; set; } = null!;
    public int IdProducto { get; set; }
    [ForeignKey(nameof(IdProducto))]
    public Producto Producto { get; set; } = null!;
}
