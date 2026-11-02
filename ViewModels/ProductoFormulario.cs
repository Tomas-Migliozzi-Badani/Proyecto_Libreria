using System.ComponentModel.DataAnnotations;

namespace Proyecto_Libreria.ViewModels;

public class ProductoFormulario
{
    [Required, StringLength(150)]
    public string Nombre { get; set; } = "";
    [StringLength(100)]
    public string Marca { get; set; } = "";
    [Range(typeof(decimal), "0.01", "9999999999999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Precio { get; set; }
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    [Range(1, int.MaxValue)]
    public int IdCategoria { get; set; }
    public bool Estado { get; set; } = true;
}

