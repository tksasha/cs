using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Beetles.Api.Endpoints;

internal static class IEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapColonyEndpoints()
        {
            var colonies = builder.MapGroup("/colonies").WithTags("Colonies");

            colonies.MapGet("/", async Task<Ok<List<ColonyResponse>>> (
                IColonyService service,
                CancellationToken cancellationToken) =>
                {
                    var colonies = await service.GetAllAsync(cancellationToken);

                    return TypedResults.Ok(colonies);
                });

            colonies.MapPost("/", async Task<Results<Created<ColonyResponse>, ValidationProblem>> (
                ColonyRequest request,
                IValidator<ColonyRequest> validator,
                IColonyService service,
                CancellationToken cancellationToken) =>
                {
                    var validationResult = await validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return TypedResults.ValidationProblem(validationResult.ToDictionary());
                    }

                    var response = await service.CreateAsync(request, cancellationToken);

                    return TypedResults.Created($"/{nameof(colonies)}/{response.Id}", response);
                });

            return builder;
        }
    }
}
