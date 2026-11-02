using System.ComponentModel.DataAnnotations;

namespace Proyecto_Libreria.Models;

public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = "";
    public bool Estado { get; set; }
}
