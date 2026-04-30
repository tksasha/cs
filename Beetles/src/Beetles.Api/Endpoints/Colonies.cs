using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using FluentValidation;

namespace Beetles.Api.Endpoints;

internal static partial class IEndpointRouteBuilderExtensions
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

            colonies.MapPost("/", async Task<Created<ColonyResponse>> (
                ColonyRequest request,
                IValidator<ColonyRequest> validator,
                IColonyService service,
                CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);

                    var response = await service.CreateAsync(request, cancellationToken);

                    return TypedResults.Created($"/{nameof(colonies)}/{response.Id}", response);
                });

            colonies.MapPatch("/{id}", async Task<Ok<ColonyResponse>> (
                int id,
                ColonyRequest request,
                IValidator<ColonyRequest> validator,
                IColonyService service,
                CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);

                    var response = await service.UpdateAsync(id, request, cancellationToken);

                    return TypedResults.Ok(response);
                });

            return builder;
        }
    }
}
