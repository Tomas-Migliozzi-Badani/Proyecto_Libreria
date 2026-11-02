using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Libreria.Models;

public class Producto
{
    [Key]
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = "";
    public string Marca { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string Imagen { get; set; } = "";
    public bool Estado { get; set; }
    public int IdCategoria { get; set; }
    [ForeignKey(nameof(IdCategoria))]
    public Categoria Categoria { get; set; } = null!;
}
