using Beetles.Api.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Beetles.Api.Tests.Endpoints;

[Collection("Database Collection")]
public sealed class WallsTest(DatabaseFixture fixture)
{
    private readonly WebApplicationFactory<Program> _factory = fixture.Factory;

    [Fact]
    public async Task Post_ShouldRespondWithCreated()
    {
        using var client = _factory.CreateClient();

        var payload = new { Color = "red", BusinessStart = "2026-05-01T00:00:00Z" };

        var response = await client.PostAsJsonAsync("/walls", payload);

        Assert.Multiple(
            () => Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode)
        );
    }
}
