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
            var beatles = builder.MapGroup("/beetles").WithTags("Beetles");

            beatles.MapGet("/", async Task<Ok<List<BeetleResponse>>> (
                IBeetleService service,
                CancellationToken cancellationToken) =>
            {
                var beetles = await service.GetAllAsync(cancellationToken);

                return TypedResults.Ok(beetles);
            });

            beatles.MapPost("/", async Task<Created<BeetleResponse>> (
                IBeetleService service,
                BeetleRequest request,
                IValidator<BeetleRequest> validator,
                CancellationToken cancellationToken) =>
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var response = await service.CreateAsync(request, cancellationToken);

                return TypedResults.Created($"/{nameof(beatles)}/{response.Id}", response);
            });

            beatles.MapPatch("/{id}", async Task<Ok<BeetleResponse>> (
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

            return builder;
        }
    }
}
