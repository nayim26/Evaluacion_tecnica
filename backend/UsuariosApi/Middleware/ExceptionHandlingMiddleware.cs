using Microsoft.Data.SqlClient;
using UsuariosApi.Dtos;
using UsuariosApi.Exceptions;

namespace UsuariosApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ApiException exception)
        {
            await Write(context, exception.StatusCode, exception.Message);
        }
        catch (SqlException exception) when (exception.Number is 2627 or 2601)
        {
            await Write(context, StatusCodes.Status409Conflict, "El nombre de usuario o el correo ya están registrados.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error no controlado en {Method} {Path}", context.Request.Method, context.Request.Path);
            await Write(context, StatusCodes.Status500InternalServerError, "Ocurrió un error interno. Inténtelo nuevamente.");
        }
    }

    private static async Task Write(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ErrorResponse { Message = message });
    }
}
