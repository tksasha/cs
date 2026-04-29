using Beetles.Application.Common.Interfaces;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using Beetles.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Beetles.Api.Endpoints;

public static class IEnpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapBeetlesEndpoints()
        {
            var beatles = builder
                .MapGroup("/beetles")
                .WithTags("Beetles");

            beatles.MapGet("/", async Task<Ok<List<BeetleResponse>>> (
                IBeetleService service,
                CancellationToken cancellationToken) =>
            {
                var beetles = await service.GetAllAsync(cancellationToken);

                return TypedResults.Ok(beetles);
            });

            beatles.MapPost("/", async Task<Results<Created<BeetleResponse>, ValidationProblem>> (
                IBeetleService service,
                CreateBeetleRequest request,
                IValidator<CreateBeetleRequest> validator,
                CancellationToken cancellationToken) =>
            {
                var validationResult = await validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return TypedResults.ValidationProblem(validationResult.ToDictionary());
                }

                var beetle = await service.CreateAsync(request, cancellationToken);

                return TypedResults.Created($"/{nameof(beatles)}/{beetle.Id}", beetle);
            });

            return builder;
        }
    }
}
