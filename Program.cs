using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Ruta opcional para pruebas en entornos con permisos restringidos.
if (builder.Environment.IsDevelopment() && builder.Configuration["DataProtection:KeysPath"] is string keysPath)
{
    Directory.CreateDirectory(keysPath);
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

// Cada equipo configura sus credenciales fuera de Git.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true)
    .AddEnvironmentVariables();
builder.Services.AddDbContext<LibreriaContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("Libreria")
        ?? throw new InvalidOperationException(
            "Falta ConnectionStrings:Libreria. Segui los pasos del README para configurar MySQL.");
    options.UseMySQL(connection);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
