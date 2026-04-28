using Microsoft.AspNetCore.Http.HttpResults;

using People.Entities;

namespace People.Endpoints;

public class Locations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder builder)
    {
        builder.MapGet("/", GetLocations);
    }

    public static Ok<List<Location>> GetLocations()
    {
        List<Location> locations = [
            new() { Id = 1, Name = "Paris" },
            new() { Id = 2, Name = "London" },
            new() { Id = 3, Name = "New York" },
        ];

        return TypedResults.Ok(locations);
    }
}
