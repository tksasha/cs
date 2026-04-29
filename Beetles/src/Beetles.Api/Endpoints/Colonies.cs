using Beetles.Application.Common.Interfaces;
using Beetles.Application.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

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

            return builder;
        }
    }
}
