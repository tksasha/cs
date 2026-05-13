using Beetles.Api.Tests.Fixtures;
using Beetles.Application.Common.Interfaces;
using Beetles.Application.Responses;
using Beetles.Domain.Entities;
using Beetles.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace Beetles.Api.Tests.Endpoints;

[Collection("Database Collection")]
public sealed class WallsTest(DatabaseFixture fixture) : AbstractEndpointTest, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory = fixture.Factory;

    private readonly DatabaseContext _databaseContext = fixture.DatabaseContext;

    private readonly Mock<TimeProvider> _timeProviderMock = fixture.TimeProviderMock;

    private readonly IBitemporalRepository _repository = fixture.BitemporalRepository;

    private static readonly DateTimeOffset Infinity = DateTimeOffset.MaxValue;

    public async Task InitializeAsync()
        => await _databaseContext.Database.ExecuteSqlRawAsync("TRUNCATE walls", CancellationToken.None);

    public async Task DisposeAsync()
    { }

    [Fact]
    public async Task ShouldCreateRedWallAsync()
    {
        int id = await CreateRedWallAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Single(walls),
            () => Assert.Contains(walls, RedWall(id))
        );
    }

    [Fact]
    public async Task ShouldUpdateRedisWallAsync()
    {
        int id = await CreateRedWallAsync(CancellationToken.None);

        await UpdateRedisWallAsync(id, CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(2, walls.Count),
            () => Assert.Contains(walls, RedWall(id)),
            () => Assert.Contains(walls, ClosedRedisWall(id))
        );
    }

    [Fact]
    public async Task ShouldNotCreateDuplicate()
    {
        using var client = _factory.CreateClient();

        var payload = new { Color = "red", DateTime = "2025-05-01T00:00:00Z" };

        var response = await client.PostAsJsonAsync("/walls", payload, CancellationToken.None);

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        response = await client.PostAsJsonAsync("/walls", payload, CancellationToken.None);

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotUpdateDuplicate()
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("10 May 2025"));

        int id = await CreateRedWallAsync(CancellationToken.None);

        using var client = _factory.CreateClient();

        var payload = new { Color = "blue", DateTime = "2025-05-02T00:00:00Z" };

        var response = await client.PatchAsJsonAsync($"/walls/{id}", payload, CancellationToken.None);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        response = await client.PatchAsJsonAsync($"/walls/{id}", payload, CancellationToken.None);

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    [Fact]
    public async Task ShouldNotCreateWithEmptyColor()
    {
        using var client = _factory.CreateClient();

        var payload = new { Color = "", DateTime = "2025-05-01T00:00:00Z" };

        var response = await client.PostAsJsonAsync("/walls", payload, CancellationToken.None);

        Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotCreateWithEmptyDateTime()
    {
        using var client = _factory.CreateClient();

        var payload = new { Color = "brown", DateTime = "" };

        var response = await client.PostAsJsonAsync("/walls", payload, CancellationToken.None);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ShouldNotCreateNonUtcDatetime()
    {
        using var client = _factory.CreateClient();

        var payload = new { Color = "black", DateTime = "2025-05-03" };

        var response = await client.PostAsJsonAsync("/walls", payload, CancellationToken.None);

        Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private async Task<int> CreateRedWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("10 May 2025"));

        using var client = _factory.CreateClient();

        var payload = new { Color = "red", DateTime = "2025-05-01T00:00:00Z" };

        var response = await client.PostAsJsonAsync("/walls", payload, cancellationToken);

        var wallResponse = response.Content.ReadFromJsonAsync<WallResponse>(cancellationToken);

        Assert.Multiple(
            () => Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode),
            () => Assert.NotNull(wallResponse)
        );

        _timeProviderMock.Verify(p => p.GetUtcNow());

        return wallResponse.Id;
    }

    private async Task UpdateRedisWallAsync(int id, CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("11 May 2025"));

        using var client = _factory.CreateClient();

        var payload = new { Color = "redis", DateTime = "2025-04-01T00:00:00Z" };

        var response = await client.PatchAsJsonAsync($"/walls/{id}", payload, cancellationToken);

        var wallResponse = await response.Content.ReadFromJsonAsync<WallResponse>(cancellationToken);

        Assert.Multiple(
            () => Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode),
            () => Assert.NotNull(wallResponse)
        );

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private static Predicate<Wall> RedWall(int id)
    {
        return wall => wall.Id == id
            && wall.Color == "red"
            && wall.BusinessStart == Date("1 May 2025")
            && wall.BusinessEnd == Infinity
            && wall.SystemStart == Date("10 May 2025")
            && wall.SystemEnd == Infinity;
    }

    private static Predicate<Wall> ClosedRedisWall(int id)
    {
        return wall => wall.Id == id
            && wall.Color == "redis"
            && wall.BusinessStart == Date("1 Apr 2025")
            && wall.BusinessEnd == Date("1 May 2025")
            && wall.SystemStart == Date("11 May 2025")
            && wall.SystemEnd == Infinity;
    }
}
