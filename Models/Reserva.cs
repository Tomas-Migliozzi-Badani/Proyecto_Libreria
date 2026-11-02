using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Libreria.Models;

public class Reserva
{
    [Key]
    public int IdReserva { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string EstadoReserva { get; set; } = "";
    public int IdCliente { get; set; }
    [ForeignKey(nameof(IdCliente))]
    public Cliente Cliente { get; set; } = null!;
    public int IdUsuario { get; set; }
    [ForeignKey(nameof(IdUsuario))]
    public Usuario Usuario { get; set; } = null!;
}
