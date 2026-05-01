using Microsoft.AspNetCore.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beetles.Api.Infrastructure;

internal sealed class ApplicationExceptionHandler(ILogger<ApplicationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            FluentValidation.ValidationException ex => Handle(httpContext, ex),
            Microsoft.EntityFrameworkCore.DbUpdateException ex => Handle(httpContext, ex),
            _ => InternalServerError(httpContext, exception),
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ValidationProblemDetails Handle(
        HttpContext httpContext,
        FluentValidation.ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        return new ValidationProblemDetails(errors)
        {
            Title = "Validation Failed",
            Status = httpContext.Response.StatusCode,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
        };
    }

    private ProblemDetails Handle(
        HttpContext httpContext,
        Microsoft.EntityFrameworkCore.DbUpdateException exception)
    {
        return exception.InnerException switch
        {
            Npgsql.PostgresException ex when ex.SqlState is Npgsql.PostgresErrorCodes.ExclusionViolation
                => Conflict(httpContext),
            _ => InternalServerError(httpContext, exception),
        };
    }

    private static ProblemDetails Conflict(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return new ProblemDetails
        {
            Title = "Conflict",
            Status = httpContext.Response.StatusCode,
            Detail = "The resource already exists or conflicts with existing data",
            Instance = httpContext.Request.Path,
        };
    }

    private ProblemDetails InternalServerError(
        HttpContext httpContext,
        Exception exception)
    {
        logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return new ProblemDetails
        {
            Title = "Internal Server Error",
            Status = httpContext.Response.StatusCode,
            Instance = httpContext.Request.Path,
        };
    }
}
