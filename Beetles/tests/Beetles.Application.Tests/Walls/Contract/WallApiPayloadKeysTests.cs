using System.Net;
using System.Net.Http.Json;
using System.Text;

using Beetles.Application.Responses;
using Beetles.Application.Tests.Database;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls.Contract;

/// <summary>
/// Tests that lock down the JSON payload key contract for POST /walls and PATCH /walls/{id}.
///
/// Key contract:
///   - The body MUST contain the keys "color" and "dateTime".
///   - Keys are matched case-insensitively (System.Text.Json default), so "Color", "COLOR" are accepted.
///   - Unknown extra keys are silently ignored by the framework (System.Text.Json only binds
///     properties declared on the DTO).
///   - Missing or misspelled "dateTime" key causes the DTO to receive default(DateTimeOffset),
///     which WallRequestValidator rejects via NotEmpty() → 422 Unprocessable Entity.
///   - Missing or misspelled "color" key causes the DTO to receive null/empty,
///     which WallRequestValidator rejects via NotEmpty() → 422 Unprocessable Entity.
/// </summary>
[Collection("Database")]
public sealed class WallApiPayloadKeysTests : WallApiTestBase
{
    public WallApiPayloadKeysTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    private static StringContent Json(string body) =>
        new(body, Encoding.UTF8, "application/json");

    // =====================================================================
    // POST /walls — correct keys
    // =====================================================================

    [Theory]
    [InlineData("color", "dateTime")]   // canonical camelCase
    [InlineData("Color", "DateTime")]   // PascalCase (case-insensitive default)
    [InlineData("COLOR", "DATETIME")]   // upper-case
    [InlineData("cOlOr", "dAtEtImE")]   // mixed case
    public async Task PK01_Post_CorrectKeys_AreAccepted_RegardlessOfCase(string colorKey, string dateTimeKey)
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var body = $"{{\"{colorKey}\":\"red\",\"{dateTimeKey}\":\"2025-05-01T00:00:00Z\"}}";

        var response = await Client.PostAsync("/walls/", Json(body));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // =====================================================================
    // POST /walls — simple, named cases for the two typos that matter most
    // =====================================================================

    [Fact]
    public async Task PK02_Post_TypoCalorInsteadOfColor_Returns400()
    {
        // Common typo: "calor" instead of "color". Since `Color` is required and the
        // deserializer doesn't recognise "calor", model binding fails → 400.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PostAsync(
            "/walls/",
            Json("{\"calor\":\"red\",\"dateTime\":\"2025-05-01T00:00:00Z\"}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PK03_Post_SimpleDateInsteadOfDateTime_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PostAsync(
            "/walls/",
            Json("{\"color\":\"red\",\"date\":\"2025-05-01T00:00:00Z\"}"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // =====================================================================
    // POST /walls — misspelled or unknown key for `color` MUST be rejected
    // =====================================================================

    [Theory]
    [InlineData("calor")]       // user-reported typo
    [InlineData("colour")]      // British spelling
    [InlineData("colors")]      // plural
    [InlineData("colur")]       // typo
    [InlineData("colorr")]      // trailing typo
    [InlineData("col0r")]       // letter→digit
    [InlineData("clr")]         // abbreviation
    [InlineData("c_olor")]      // underscore
    [InlineData("color ")]      // trailing space
    [InlineData(" color")]      // leading space
    [InlineData("colorName")]   // related-looking
    [InlineData("paint")]       // unrelated synonym
    public async Task PK10_Post_MisspelledColorKey_Returns400(string wrongKey)
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var body = $"{{\"{wrongKey}\":\"red\",\"dateTime\":\"2025-05-01T00:00:00Z\"}}";

        var response = await Client.PostAsync("/walls/", Json(body));

        // `Color` is `required` — when the deserializer can't find it, model binding fails → 400.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PK11_Post_NoKeysAtAll_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PostAsync("/walls/", Json("{}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PK12_Post_OnlyDateTime_NoColorKey_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PostAsync(
            "/walls/",
            Json("{\"dateTime\":\"2025-05-01T00:00:00Z\"}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================================
    // POST /walls — misspelled `dateTime` key is ignored; validator rejects empty DateTime → 422
    // =====================================================================

    [Theory]
    [InlineData("date")]
    [InlineData("dateTimes")]
    [InlineData("time")]
    [InlineData("datetime ")]
    [InlineData("when")]
    [InlineData("date_time")]
    [InlineData("date-time")]
    public async Task PK20_Post_MisspelledDateTimeKey_Returns422(string wrongKey)
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var body = $"{{\"color\":\"red\",\"{wrongKey}\":\"2025-05-01T00:00:00Z\"}}";

        var response = await Client.PostAsync("/walls/", Json(body));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task PK21_Post_OnlyColorKey_NoDateTime_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PostAsync("/walls/", Json("{\"color\":\"red\"}"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // =====================================================================
    // PATCH /walls/{id} — correct keys
    // =====================================================================

    [Theory]
    [InlineData("color", "dateTime")]
    [InlineData("Color", "DateTime")]
    [InlineData("COLOR", "DATETIME")]
    public async Task PK30_Patch_CorrectKeys_AreAccepted_RegardlessOfCase(string colorKey, string dateTimeKey)
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var body = $"{{\"{colorKey}\":\"blue\",\"{dateTimeKey}\":\"2025-05-03T00:00:00Z\"}}";

        var response = await Client.PatchAsync($"/walls/{id}", Json(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);
        Assert.Equal("blue", payload!.Color);
    }

    // =====================================================================
    // PATCH /walls/{id} — simple, named cases for the two typos that matter most
    // =====================================================================

    [Fact]
    public async Task PK31_Patch_TypoCalorInsteadOfColor_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response = await Client.PatchAsync(
            $"/walls/{id}",
            Json("{\"calor\":\"blue\",\"dateTime\":\"2025-05-03T00:00:00Z\"}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PK32_Patch_SimpleDateInsteadOfDateTime_Returns422()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response = await Client.PatchAsync(
            $"/walls/{id}",
            Json("{\"color\":\"blue\",\"date\":\"2025-05-03T00:00:00Z\"}"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // =====================================================================
    // PATCH /walls/{id} — misspelled or unknown key for `color` MUST be rejected
    // =====================================================================

    [Theory]
    [InlineData("calor")]
    [InlineData("colour")]
    [InlineData("colors")]
    [InlineData("colur")]
    [InlineData("clr")]
    [InlineData("paint")]
    [InlineData("newColor")]
    [InlineData("color ")]
    [InlineData(" color")]
    public async Task PK40_Patch_MisspelledColorKey_Returns400(string wrongKey)
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var body = $"{{\"{wrongKey}\":\"blue\",\"dateTime\":\"2025-05-03T00:00:00Z\"}}";

        var response = await Client.PatchAsync($"/walls/{id}", Json(body));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PK41_Patch_NoKeysAtAll_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        var response = await Client.PatchAsync($"/walls/{id}", Json("{}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PK42_Patch_OnlyDateTime_NoColorKey_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response = await Client.PatchAsync(
            $"/walls/{id}",
            Json("{\"dateTime\":\"2025-05-03T00:00:00Z\"}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================================
    // PATCH /walls/{id} — misspelled `dateTime` key is ignored; validator rejects empty DateTime → 422
    // =====================================================================

    [Theory]
    [InlineData("date")]
    [InlineData("dateTimes")]
    [InlineData("when")]
    [InlineData("date_time")]
    public async Task PK50_Patch_MisspelledDateTimeKey_Returns422(string wrongKey)
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var body = $"{{\"color\":\"blue\",\"{wrongKey}\":\"2025-05-03T00:00:00Z\"}}";

        var response = await Client.PatchAsync($"/walls/{id}", Json(body));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // =====================================================================
    // Unknown extra keys (POST + PATCH) — silently ignored by the framework
    // System.Text.Json only binds properties that exist on the request DTO;
    // unknown keys are dropped without error.
    // =====================================================================

    [Fact]
    public async Task PK90_Post_UnknownExtraKey_IsIgnored_Returns201()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var body = "{\"color\":\"red\",\"dateTime\":\"2025-05-01T00:00:00Z\",\"unknown\":\"value\",\"x\":123}";
        var response = await Client.PostAsync("/walls/", Json(body));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PK91_Patch_UnknownExtraKey_IsIgnored_Returns200()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var body = "{\"color\":\"blue\",\"dateTime\":\"2025-05-03T00:00:00Z\",\"unknown\":\"v\",\"extra\":42}";
        var response = await Client.PatchAsync($"/walls/{id}", Json(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================================
    // Duplicate keys — JSON spec leaves this undefined; STJ takes the last one
    // =====================================================================

    [Fact]
    public async Task PK95_Post_DuplicateColorKeys_LastValueWins()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var body = "{\"color\":\"red\",\"color\":\"green\",\"dateTime\":\"2025-05-01T00:00:00Z\"}";
        var response = await Client.PostAsync("/walls/", Json(body));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);
        Assert.Equal("green", payload!.Color);
    }

    [Fact]
    public async Task PK96_Post_BodyWithNumericTopLevel_Returns400()
    {
        // The body is not even an object — the deserializer must reject it.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PostAsync("/walls/", Json("42"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PK97_Post_BodyWithArrayTopLevel_Returns400()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var response = await Client.PostAsync(
            "/walls/",
            Json("[{\"color\":\"red\",\"dateTime\":\"2025-05-01T00:00:00Z\"}]"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
