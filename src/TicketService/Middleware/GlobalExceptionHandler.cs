

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TicketService.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Wystąpił nieoczekiwany błąd {Message}",exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch(exception)
        {
            case KeyNotFoundException:
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Title = "Nie znaleziono zasobu";
                problemDetails.Detail = exception.Message;  
                break;

            case ArgumentException:
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Title = "Błąd walidacji";
                problemDetails.Detail = exception.Message;
                break;
            case UnauthorizedAccessException:
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Title = "Brak dostępu";
                problemDetails.Detail = exception.Message;
                break;
            default:
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Title = "Błąd serwera";
                problemDetails.Detail = exception.Message;
                break;
        }
        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails,cancellationToken);

        return true;
    }
}