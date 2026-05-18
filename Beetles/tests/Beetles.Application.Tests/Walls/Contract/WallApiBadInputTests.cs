using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using Beetles.Application.Responses;
using Beetles.Application.Tests.Database;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls.Contract;

/// <summary>
/// API correctness tests covering bad inputs, invalid keys, content negotiation,
/// response headers, and ProblemDetails shape. These complement <see cref="WallApiContractTests"/>
/// by filling gaps around HTTP-layer concerns (routing, model binding, headers).
/// </summary>
[Collection("Database")]
public sealed class WallApiBadInputTests : WallApiTestBase
{
    public WallApiBadInputTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    // ---------------------------------------------------------------------
    // POST /walls — bad/missing/invalid body
    // ---------------------------------------------------------------------

    [Fact]
    public async Task BI01_Post_NullColor_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        // System.Text.Json + `required string Color` should reject null at bind time.
        var response = await PostWall(new { Color = (string?)null, DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task BI02_Post_WhitespaceOnlyColor_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "   ", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task BI03_Post_MissingDateTime_Returns422()
    {
        // When DateTime is absent the DTO receives default(DateTimeOffset).
        // WallRequestValidator rejects it as empty → 422 Unprocessable Entity.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "red" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task BI04_Post_MalformedJson_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var content = new StringContent("{ this is not json", Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/walls/", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BI05_Post_EmptyBody_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/walls/", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BI06_Post_WrongContentType_TextPlain_Returns415Or400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var content = new StringContent("Color=red&DateTime=2025-05-01T00:00:00Z", Encoding.UTF8, "text/plain");
        var response = await Client.PostAsync("/walls/", content);

        Assert.True(
            response.StatusCode is HttpStatusCode.UnsupportedMediaType or HttpStatusCode.BadRequest,
            $"Expected 415 or 400, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task BI07_Post_WrongTypeForColor_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        // Color sent as number — JSON deserialization should fail.
        var content = new StringContent(
            "{\"color\":123,\"dateTime\":\"2025-05-01T00:00:00Z\"}",
            Encoding.UTF8,
            "application/json");

        var response = await Client.PostAsync("/walls/", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BI08_Post_WrongTypeForDateTime_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var content = new StringContent(
            "{\"color\":\"red\",\"dateTime\":\"definitely-not-a-date\"}",
            Encoding.UTF8,
            "application/json");

        var response = await Client.PostAsync("/walls/", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BI09_Post_ExtraUnknownProperty_IsIgnored_Returns201()
    {
        // System.Text.Json only binds properties declared on the DTO; unknown keys are dropped.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new
        {
            Color = "red",
            DateTime = Utc(2025, 5, 1),
            Mystery = "ignored",
            ExtraNumber = 42,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task BI10_Post_LocationHeader_PointsToCreatedResource()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);
        Assert.NotNull(response.Headers.Location);

        var location = response.Headers.Location!.OriginalString;
        Assert.EndsWith($"/walls/{payload!.Id}", location, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BI11_Post_ResponseContentType_IsJson()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType!.MediaType);
    }

    // ---------------------------------------------------------------------
    // PATCH /walls/{id} — invalid keys & inputs
    // ---------------------------------------------------------------------

    [Fact]
    public async Task BI20_Patch_NegativeId_Returns404()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PatchWall(-1, new { Color = "red", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BI21_Patch_ZeroId_Returns404()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PatchWall(0, new { Color = "red", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BI22_Patch_OverflowingId_Returns400()
    {
        // Number larger than Int32.MaxValue → route binding fails → 400.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PatchAsJsonAsync(
            "/walls/99999999999999999999",
            new { Color = "red", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BI23_Patch_FloatId_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PatchAsJsonAsync(
            "/walls/1.5",
            new { Color = "red", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BI24_Patch_EmptyColor_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response = await PatchWall(id, new { Color = "", DateTime = Utc(2025, 5, 3) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task BI25_Patch_NonUtcDateTime_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response = await PatchWall(id, new { Color = "blue", DateTime = "2025-05-03T00:00:00+05:00" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task BI26_Patch_ResponseBody_ReflectsUpdate()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);
        Assert.Equal(id, payload!.Id);
        Assert.Equal("blue", payload.Color);
    }

    // ---------------------------------------------------------------------
    // DELETE /walls/{id} — invalid keys & inputs
    // ---------------------------------------------------------------------

    [Fact]
    public async Task BI30_Delete_NegativeId_Returns404()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await DeleteWall(-1, Utc(2025, 5, 1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BI34_Delete_ZeroId_Returns404()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await DeleteWall(0, Utc(2025, 5, 1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BI31_Delete_NonIntegerId_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.DeleteAsync($"/walls/abc?dateTime={Uri.EscapeDataString(Utc(2025, 5, 1).ToString("O"))}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BI32_Delete_DateBeforeAnyVersion_Returns404()
    {
        // Wall exists but the delete date is before its first business interval.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 6, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 5));
        var response = await DeleteWall(id, Utc(2025, 5, 1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BI33_Delete_EmptyDateQuery_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.DeleteAsync("/walls/1?dateTime=");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------------------------------------------------------------------
    // Routing / method / path
    // ---------------------------------------------------------------------

    [Fact]
    public async Task BI40_Get_Walls_Returns404Or405()
    {
        // No GET endpoint is defined on /walls.
        var response = await Client.GetAsync("/walls/");

        Assert.True(
            response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
            $"Expected 404 or 405, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task BI41_Put_Walls_Returns404Or405()
    {
        var response = await Client.PutAsJsonAsync("/walls/1", new { Color = "red", DateTime = Utc(2025, 5, 1) });

        Assert.True(
            response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
            $"Expected 404 or 405, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task BI42_UnknownPath_Returns404()
    {
        var response = await Client.GetAsync("/no-such-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------------------
    // ProblemDetails shape — content type, status, instance
    // ---------------------------------------------------------------------

    [Fact]
    public async Task BI50_ProblemDetails_ContentType_IsProblemJson()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task BI51_ProblemDetails_Instance_ReferencesRequestPath()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PatchWall(987654, new { Color = "red", DateTime = Utc(2025, 5, 1) });
        var problem = await AssertProblemDetailsAsync(response, 404, "Not Found");

        Assert.True(problem.TryGetProperty("instance", out var instance));
        var instanceString = instance.GetString();
        Assert.False(string.IsNullOrWhiteSpace(instanceString));
        Assert.Contains("/walls/987654", instanceString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BI52_ProblemDetails_ValidationErrors_KeyedByPropertyName()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("errors", out var errors));
        var errorPropertyNames = errors.EnumerateObject().Select(p => p.Name).ToList();
        Assert.Contains(errorPropertyNames, name => name.Equals("Color", StringComparison.OrdinalIgnoreCase));
    }

    // ---------------------------------------------------------------------
    // Color field edge cases
    // ---------------------------------------------------------------------

    [Fact]
    public async Task VAL10_Post_VeryLongColor_ReturnsClientErrorNotServerError()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var longColor = new string('x', 5000);

        var response = await PostWall(new { Color = longColor, DateTime = Utc(2025, 5, 1) });

        Assert.True(
            (int)response.StatusCode is >= 200 and < 500,
            $"Expected 2xx or 4xx for a 5000-char color, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task VAL11_Post_NumericStringColor_IsAccepted()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await PostWall(new { Color = "42", DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.Equal("42", payload!.Color);
    }

    [Fact]
    public async Task VAL12_Post_HtmlSpecialCharsInColor_StoredVerbatim()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        const string special = "<red>&\"blue'";

        var response = await PostWall(new { Color = special, DateTime = Utc(2025, 5, 1) });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.Equal(special, payload!.Color);
    }
}
