using Core.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace backend.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        var (statusCode, title, detail) = exception switch
        {
            DuplicateEmailException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            UserNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            TokenNotValidException  => (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            TokenNotFoundException => (StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message),
            PatientNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            ClinicianNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            TimeSlotNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            TimeSlotNotAvailableException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),
            AppointmentNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
            
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}