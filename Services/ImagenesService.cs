using System.Text.RegularExpressions;
using SkiaSharp;

namespace Proyecto_Libreria.Services;

// Guarda solo pixeles recodificados: nunca conserva el nombre ni el contenido original.
public class ImagenesService(IWebHostEnvironment entorno, ILogger<ImagenesService> logger)
{
    public const long MaximoBytes = 2 * 1024 * 1024;
    private readonly string carpeta = Path.Combine(entorno.ContentRootPath, "App_Data", "imagenes");
    private static bool EsNombrePropio(string nombre) => Regex.IsMatch(nombre, @"\A[a-f0-9]{32}\.png\z");
    public string? Ruta(string nombre) => EsNombrePropio(nombre) ? Path.Combine(carpeta, nombre) : null;

    public async Task<string> Guardar(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0 || archivo.Length > MaximoBytes)
            throw new ArgumentException("Selecciona una imagen de hasta 2 MB.");
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png"))
            throw new ArgumentException("Solo se permiten imagenes JPG o PNG.");
        using var memoria = new MemoryStream();
        await archivo.CopyToAsync(memoria);
        memoria.Position = 0;
        using var codec = SKCodec.Create(memoria);
        if (codec is null || codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png)
            || (extension == ".png") != (codec.EncodedFormat == SKEncodedImageFormat.Png))
            throw new ArgumentException("El contenido no coincide con una imagen JPG o PNG valida.");
        var info = codec.Info;
        if (info.Width <= 0 || info.Height <= 0 || info.Width > 4096 || info.Height > 4096
            || (long)info.Width * info.Height > 12_000_000)
            throw new ArgumentException("La imagen debe medir hasta 4096 pixeles por lado y 12 megapixeles.");
        using var bitmap = new SKBitmap(new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            throw new ArgumentException("La imagen esta incompleta o dañada.");
        using var imagen = SKImage.FromBitmap(bitmap);
        using var datos = imagen.Encode(SKEncodedImageFormat.Png, 100);
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid().ToString("N") + ".png";
        var ruta = Ruta(nombre)!;
        try
        {
            await using var salida = new FileStream(ruta, FileMode.CreateNew);
            datos.SaveTo(salida);
        }
        catch { Borrar(nombre); throw; }
        return nombre;
    }

    public void Borrar(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return;
        var ruta = Ruta(nombre);
        if (ruta is null) return;
        try { File.Delete(ruta); }
        catch (IOException ex) { logger.LogWarning(ex, "No se pudo limpiar una imagen reemplazada."); }
        catch (UnauthorizedAccessException ex) { logger.LogWarning(ex, "No se pudo limpiar una imagen reemplazada."); }
    }
}
