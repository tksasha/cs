using Microsoft.AspNetCore.Http.HttpResults;

using People.Entities;
using People.Services;

namespace People.Endpoints;

public class Locations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetLocationsAsync);
    }

    public static async Task<Ok<List<Location>>> GetLocationsAsync(
        ILocationService service,
        CancellationToken cancellationToken)
    {
        List<Location> locations = await service.GetAllAsync(cancellationToken);

        return TypedResults.Ok(locations);
    }
}
