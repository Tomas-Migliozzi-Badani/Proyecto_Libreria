using System.Text.RegularExpressions;
using SkiaSharp;

namespace Proyecto_Libreria.Services;

// Guarda solo pixeles recodificados: nunca conserva el nombre ni el contenido original.
public class ImagenesService
{
    public const long MaximoBytes = 2 * 1024 * 1024;
    private readonly string _carpetaImagenes;
    private readonly ILogger<ImagenesService> _logger;

    public ImagenesService(IWebHostEnvironment entorno, ILogger<ImagenesService> logger)
    {
        _carpetaImagenes = Path.Combine(entorno.ContentRootPath, "App_Data", "imagenes");
        _logger = logger;
    }
    private static bool EsNombrePropio(string nombre) => Regex.IsMatch(nombre, @"\A[a-f0-9]{32}\.png\z");
    public string? Ruta(string nombre) => EsNombrePropio(nombre) ? Path.Combine(_carpetaImagenes, nombre) : null;

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
        var datosImagen = codec.Info;
        if (datosImagen.Width <= 0 || datosImagen.Height <= 0 || datosImagen.Width > 4096 || datosImagen.Height > 4096
            || (long)datosImagen.Width * datosImagen.Height > 12_000_000)
            throw new ArgumentException("La imagen debe medir hasta 4096 pixeles por lado y 12 megapixeles.");
        using var bitmap = new SKBitmap(new SKImageInfo(datosImagen.Width, datosImagen.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            throw new ArgumentException("La imagen esta incompleta o dañada.");
        using var imagen = SKImage.FromBitmap(bitmap);
        using var datos = imagen.Encode(SKEncodedImageFormat.Png, 100);
        Directory.CreateDirectory(_carpetaImagenes);
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
        catch (IOException ex) { _logger.LogWarning(ex, "No se pudo limpiar una imagen reemplazada."); }
        catch (UnauthorizedAccessException ex) { _logger.LogWarning(ex, "No se pudo limpiar una imagen reemplazada."); }
    }
}
