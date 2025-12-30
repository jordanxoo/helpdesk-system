using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Exceptions;

/// <summary>
/// Globalny handler wyjątków zgodny z RFC 7807 (Problem Details)
/// Używa IExceptionHandler z .NET 8+
/// </summary>
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
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Exception occurred: {Message}",
            exception.Message);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception handled
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception),
            Detail = exception.Message,
            Instance = context.Request.Path,
            Type = GetTypeUrl(exception)
        };

        switch (exception)
        {
            case ValidationException validationException:
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            case BadRequestException badRequestException when badRequestException.Errors != null:
                problemDetails.Extensions["errors"] = badRequestException.Errors;
                break;

            case NotFoundException notFoundException:
                problemDetails.Extensions["resourceName"] = notFoundException.ResourceName;
                problemDetails.Extensions["resourceId"] = notFoundException.ResourceId?.ToString() ?? "";
                break;

            case ConflictException conflictException:
                problemDetails.Extensions["resourceName"] = conflictException.ResourceName;
                problemDetails.Extensions["conflictField"] = conflictException.ConflictField;
                problemDetails.Extensions["conflictValue"] = conflictException.ConflictValue?.ToString() ?? "";
                break;
        }

        // development  stack trace
        if (context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            ForbiddenException => StatusCodes.Status403Forbidden,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(Exception exception) =>
        exception switch
        {
            ValidationException => "Validation Error",
            BadRequestException => "Bad Request",
            NotFoundException => "Resource Not Found",
            ConflictException => "Resource Conflict",
            ForbiddenException => "Forbidden",
            UnauthorizedAccessException => "Unauthorized",
            _ => "Internal Server Error"
        };

    private static string GetTypeUrl(Exception exception) =>
        exception switch
        {
            ValidationException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            BadRequestException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            NotFoundException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            ConflictException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            ForbiddenException => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            UnauthorizedAccessException => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };
}