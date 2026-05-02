using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Beetles.Api.Endpoints;

internal static partial class IEndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapBeetleColonyEndpoints()
        {
            var beetleColonies = builder.MapGroup("/beetle-colonies").WithTags("BeetleColonies");

            beetleColonies.MapGet("/", async Task<Ok<List<BeetleColonyResponse>>> (
                IBeetleColonyService service,
                CancellationToken cancellationToken) =>
                {
                    var beetleColonies = await service.GetAllAsync(cancellationToken);

                    return TypedResults.Ok(beetleColonies);
                });

            beetleColonies.MapPost("/", async Task<NoContent> (
                BeetleColonyRequest request,
                IValidator<BeetleColonyRequest> validator,
                IBeetleColonyService service,
                CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);

                    await service.CreateAsync(request, cancellationToken);

                    return TypedResults.NoContent();
                });

            return builder;
        }
    }
}
