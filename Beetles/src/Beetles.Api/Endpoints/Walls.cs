using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Responses;

using FluentValidation;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

            walls.MapPatch("/{id}", async Task<Ok<WallResponse>> (
                int id,
                IValidator<WallRequest> validator,
                WallRequest request,
                IWallService service,
                CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);

                    var response = await service.UpdateAsync(id, request, cancellationToken);

                    return TypedResults.Ok(response);
                });

            walls.MapDelete("/{id}", async Task<NoContent> (
                int id,
                [FromQuery] DateTimeOffset date,
                IValidator<DateTimeOffset> validator,
                IWallService service,
                CancellationToken cancellationToken) =>
            {
                await validator.ValidateAndThrowAsync(date, cancellationToken);

                await service.DeleteAsync(id, date, cancellationToken);

                return TypedResults.NoContent();
            });

            return builder;
        }
    }
}
