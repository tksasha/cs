using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Beetles.Application.Responses;
using Beetles.Application.Tests.Database;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

[Collection("Database")]
public sealed class WallApiContractTests : WallApiTestBase
{
    public WallApiContractTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task C02_Post_EmptyColor_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task C03_Post_MissingColor_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task C05_Post_NonUtcDateTime_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "red", DateTime = "2025-05-01T00:00:00+05:00" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task C07_Patch_NonExistingId_Returns404()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PatchWall(99999, new { Color = "red", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task C09_Patch_EmptyBody_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PatchWall(1, new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task C12_Patch_NonIntegerId_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PatchAsJsonAsync("/walls/abc", new { Color = "red", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task C13_Delete_MissingDate_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.DeleteAsync("/walls/1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task C14_Delete_NonUtcDate_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.DeleteAsync("/walls/1?dateTime=2025-05-01T00:00:00%2B05:00");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task C15_Delete_NonExistingId_Returns404()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await DeleteWall(99999, Utc(2025, 5, 1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task C16_Post_ValidationProblem_HasExpectedShape()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "", DateTime = Utc(2025, 5, 1) });

        await AssertValidationProblemContainsAsync(response, "must not be empty");
    }

    [Fact]
    public async Task C17_Patch_NotFound_ProblemDetailsShape()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PatchWall(99999, new { Color = "red", DateTime = Utc(2025, 5, 1) });

        var problem = await AssertProblemDetailsAsync(response, 404, "Not Found");
        Assert.True(problem.TryGetProperty("detail", out var detail));
        Assert.False(string.IsNullOrWhiteSpace(detail.GetString()));
    }

    [Fact]
    public async Task C18_Delete_ValidationProblem_HasExpectedShape()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.DeleteAsync("/walls/1?dateTime=2025-05-01T00:00:00%2B05:00");

        await AssertValidationProblemContainsAsync(response, "Must be UTC.");
    }

    [Fact]
    public async Task C19_Delete_MalformedDate_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.DeleteAsync("/walls/1?dateTime=not-a-date");

        await AssertProblemDetailsAsync(response, 400, "Bad Request");
    }

    [Fact]
    public async Task C21_Post_ResponseBody_ContainsTemporalFields()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("id", out var id));
        Assert.True(id.GetInt32() > 0);
        Assert.Equal("red", root.GetProperty("color").GetString());
        Assert.True(root.TryGetProperty("businessStart", out _));
        Assert.True(root.TryGetProperty("systemStart", out _));
        Assert.True(root.TryGetProperty("systemEnd", out _));
    }

    [Fact]
    public async Task C30_Delete_Success_Returns204_WithEmptyBody()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var response = await DeleteWall(id, Utc(2025, 5, 3));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.Empty(body);
    }

    [Fact]
    public async Task C40_Post_SameColorAsExistingWall_Returns409()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var first = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task C35_Patch_ResponseBody_ReflectsSystemStartAtUpdateTime()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);
        Assert.Equal(Utc(2025, 5, 11), payload!.SystemStart);
        Assert.Equal("blue", payload.Color);
        Assert.Equal(id, payload.Id);
    }
}
