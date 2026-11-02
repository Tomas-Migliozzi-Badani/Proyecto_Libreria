using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Proyecto_Libreria.Data;

// Permite generar migraciones y SQL sin una conexion activa a MySQL.
public class LibreriaContextFactory : IDesignTimeDbContextFactory<LibreriaContext>
{
    public LibreriaContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
        var connection = configuration.GetConnectionString("Libreria")
            ?? "server=localhost;port=3306;database=Libreria;user=CONFIGURAR;password=CONFIGURAR";
        var options = new DbContextOptionsBuilder<LibreriaContext>().UseMySQL(connection).Options;
        return new LibreriaContext(options);
    }
}
