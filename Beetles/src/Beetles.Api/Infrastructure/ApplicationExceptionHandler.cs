using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Beetles.Application.Exceptions;

namespace Beetles.Api.Infrastructure;

internal sealed class ApplicationExceptionHandler(ILogger<ApplicationExceptionHandler> logger) : IExceptionHandler
{
    private const string ProblemJson = "application/problem+json";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            FluentValidation.ValidationException ex => Handle(httpContext, ex),
            Microsoft.EntityFrameworkCore.DbUpdateException ex => Handle(httpContext, ex),
            BadHttpRequestException
                => Handle(httpContext, new BadRequestException()),
            IApplicationException ex => Handle(httpContext, ex),
            _ => InternalServerError(httpContext, exception),
        };

        await httpContext.Response.WriteAsJsonAsync(
            value: problemDetails,
            type: problemDetails.GetType(),
            options: null,
            contentType: ProblemJson,
            cancellationToken: cancellationToken);

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
                => Handle(httpContext, new ConflictException()),
            _ => InternalServerError(httpContext, exception),
        };
    }

    private static ProblemDetails Handle(HttpContext httpContext, IApplicationException exception)
    {
        httpContext.Response.StatusCode = exception.StatusCode;

        return new ProblemDetails
        {
            Title = exception.Title,
            Status = httpContext.Response.StatusCode,
            Detail = exception.Message,
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
