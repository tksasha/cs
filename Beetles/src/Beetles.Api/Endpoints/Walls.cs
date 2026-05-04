using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Beetles.Api.Endpoints;

internal static class IEntpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapWallEndpoints()
        {
            var walls = builder.MapGroup("/walls").WithTags("Walls");

            walls.MapPost("/", async Task<Created<WallResponse>> (
                IValidator<WallRequest> validator,
                WallRequest request,
                IWallService service,
                CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);

                    var response = await service.CreateAsync(request, cancellationToken);

                    return TypedResults.Created($"/{nameof(walls)}/{response.Id}", response);
                });

            return builder;
        }
    }
}
