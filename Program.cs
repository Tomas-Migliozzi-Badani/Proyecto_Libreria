using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.RateLimiting;
using Proyecto_Libreria.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Proyecto_Libreria.Data;

var builder = WebApplication.CreateBuilder(args);

// Cada equipo configura sus credenciales fuera de Git.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true)
    .AddEnvironmentVariables();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ReservasService>();
builder.Services.AddScoped<SesionCookieEvents>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    options.Cookie.Name = "Libreria.Sesion";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.EventsType = typeof(SesionCookieEvents);
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("Demasiados intentos. Espera un minuto antes de volver a iniciar sesion.", cancellationToken);
    };
    options.AddPolicy("registro", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// Ruta opcional para pruebas en entornos con permisos restringidos.
if (builder.Environment.IsDevelopment() && builder.Configuration["DataProtection:KeysPath"] is string keysPath)
{
    Directory.CreateDirectory(keysPath);
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

builder.Services.AddDbContext<LibreriaContext>(options =>
{
    var connection = builder.Configuration.GetConnectionString("Libreria")
        ?? throw new InvalidOperationException(
            "Falta ConnectionStrings:Libreria. Segui los pasos del README para configurar MySQL.");
    options.UseMySQL(connection);
});

var app = builder.Build();

if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("DatosPrueba:Habilitar"))
{
    using var scope = app.Services.CreateScope();
    await DatosDemostracion.CrearUsuarios(scope.ServiceProvider.GetRequiredService<LibreriaContext>());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
