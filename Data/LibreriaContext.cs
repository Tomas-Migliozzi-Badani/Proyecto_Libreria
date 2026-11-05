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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Usuario>().HasOne(u => u.Cliente).WithOne()
            .HasForeignKey<Usuario>(u => u.IdCliente).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Cliente>().Property(c => c.Dni).HasMaxLength(20);
        modelBuilder.Entity<Cliente>().HasIndex(c => c.Dni).IsUnique();
        modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleReserva>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);

        // Conservar el historial: no borrar reservas al eliminar clientes o productos.
        foreach (var relacion in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relacion.DeleteBehavior = DeleteBehavior.Restrict;

        modelBuilder.Entity<Categoria>().Property(c => c.Nombre).HasMaxLength(100);
        modelBuilder.Entity<Producto>().Property(p => p.Nombre).HasMaxLength(150);
        modelBuilder.Entity<Producto>().Property(p => p.Marca).HasMaxLength(100);
        modelBuilder.Entity<Reserva>().Property(r => r.EstadoReserva).HasMaxLength(30);
        modelBuilder.Entity<Usuario>().Property(u => u.Email).HasMaxLength(254);
        modelBuilder.Entity<Usuario>().Property(u => u.Rol).HasMaxLength(30);
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
    }
}
