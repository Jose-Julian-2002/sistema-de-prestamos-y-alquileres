using System.Net;
using FinanzasManager.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, mensaje) = exception switch
        {
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Credenciales incorrectas o usuario inactivo."),
            AccesoDenegadoException =>
                (HttpStatusCode.Forbidden, exception.Message),
            ArgumentException =>
                (HttpStatusCode.BadRequest, exception.Message),
            InvalidOperationException =>
                (HttpStatusCode.Conflict, exception.Message),
            _ =>
                (HttpStatusCode.InternalServerError, "Error interno del servidor.")
        };

        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new { mensaje },
            cancellationToken);

        return true;
    }
}
