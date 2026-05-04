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
        public IEndpointRouteBuilder MapBeetleEndpoints()
        {
            var beetles = builder.MapGroup("/beetles").WithTags("Beetles");

            beetles.MapGet("/", async Task<Ok<List<BeetleResponse>>> (
                IBeetleService service,
                CancellationToken cancellationToken) =>
            {
                var beetles = await service.GetAllAsync(cancellationToken);

                return TypedResults.Ok(beetles);
            });

            beetles.MapPost("/", async Task<Created<BeetleResponse>> (
                IBeetleService service,
                BeetleRequest request,
                IValidator<BeetleRequest> validator,
                CancellationToken cancellationToken) =>
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var response = await service.CreateAsync(request, cancellationToken);

                return TypedResults.Created($"/{nameof(beetles)}/{response.Id}", response);
            });

            beetles.MapPatch("/{id}", async Task<Ok<BeetleResponse>> (
                IBeetleService service,
                int id,
                BeetleRequest request,
                IValidator<BeetleRequest> validator,
                CancellationToken cancellationToken) =>
                {
                    await validator.ValidateAndThrowAsync(request, cancellationToken);

                    var response = await service.UpdateAsync(id, request, cancellationToken);

                    return TypedResults.Ok(response);
                });

            beetles.MapDelete("/{id}", async Task<NoContent> (
                int id,
                IBeetleService service,
                CancellationToken cancellationToken) =>
                {
                    await service.DeleteAsync(id, cancellationToken);

                    return TypedResults.NoContent();
                });

            return builder;
        }
    }
}
