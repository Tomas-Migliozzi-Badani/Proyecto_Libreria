using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Models;

namespace Proyecto_Libreria.Data;

public class LibreriaContext : DbContext
{
    public LibreriaContext(DbContextOptions<LibreriaContext> options)
        : base(options)
    {
        
    }
    
    

    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<DetalleReserva> DetallesReserva { get; set; }
}