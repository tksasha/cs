using Beetles.Application.Common.Interfaces;
using Beetles.Application.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Beetles.Api.Endpoints;

public static class IEnpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder MapBeetlesEndpoints()
        {
            builder.MapGet("/beetles", async Task<Ok<List<BeetleResponse>>> (
                CancellationToken cancellationToken,
                IBeetleService service) =>
            {
                var beetles = await service.GetAllAsync(cancellationToken);

                return TypedResults.Ok(beetles);
            });

            return builder;
        }
    }
}
