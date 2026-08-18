using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (statusCode, message) = ex switch
            {
                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, ex.Message),

                KeyNotFoundException =>
                    (HttpStatusCode.NotFound, ex.Message),

                InvalidOperationException =>
                    (HttpStatusCode.BadRequest, ex.Message),

                ArgumentException =>
                    (HttpStatusCode.BadRequest, ex.Message),

                DbUpdateException =>
                    (HttpStatusCode.Conflict,
                     "La operacion no pudo completarse porque entra en conflicto con datos existentes."),

                _ =>
                    (HttpStatusCode.InternalServerError,
                     "Ocurrio un error interno en el servidor.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Error no controlado en la API.");
            }
            else
            {
                _logger.LogWarning(
                    ex,
                    "Solicitud rechazada con codigo {StatusCode}.",
                    (int)statusCode
                );
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new
            {
                success = false,
                status = (int)statusCode,
                message
            };

            var json = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            );

            await context.Response.WriteAsync(json);
        }
    }
}
