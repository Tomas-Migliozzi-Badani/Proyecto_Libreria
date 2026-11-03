using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
namespace Proyecto_Libreria.Filters;

// La API responde JSON tambien ante errores, sin devolver una pagina HTML de MVC.
public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;
    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) => _logger = logger;
    public void OnException(ExceptionContext context)
    {
        var (status, mensaje) = context.Exception switch
        {
            KeyNotFoundException => (404, "El registro no existe."),
            ArgumentException e => (400, e.Message),
            DbUpdateException => (409, "Los datos cambiaron o tienen relaciones que impiden la operacion."),
            MySqlException e when e.Number is 1205 or 1213 => (409, "Otra operacion modifico los datos. Intenta nuevamente."),
            MySqlException => (503, "No se pudo acceder a la base de datos. Intenta nuevamente."),
            _ => (500, "No se pudo completar la operacion.")
        };
        if (status >= 500) _logger.LogError(context.Exception, "Error en la API");
        context.Result = new ObjectResult(new { mensaje }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
