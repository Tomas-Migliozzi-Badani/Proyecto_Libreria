using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Libreria.Services;
namespace Proyecto_Libreria.Controllers;

// Las fotos del catalogo y los avatares son publicos; no admitir documentos privados aqui.
[AllowAnonymous]
public class ImagenesController(ImagenesService imagenes) : Controller
{
    [HttpGet("imagenes/{nombre}")]
    public IActionResult Obtener(string nombre)
    {
        var ruta = imagenes.Ruta(nombre);
        if (ruta is null || !System.IO.File.Exists(ruta)) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.CacheControl = "public,max-age=86400";
        return PhysicalFile(ruta, "image/png");
    }
}
