using System.Globalization;

using Beetles.Api.Tests.Fixtures;
using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Beetles.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace Beetles.Api.Tests.Endpoints;

[Collection("Database Collection")]
public sealed class WallsTest(DatabaseFixture fixture) : IAsyncLifetime
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
    public async Task ShouldCreateRedWall()
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("12 May 2026"));

        using var client = _factory.CreateClient();

        var payload = new { Color = "red", DateTime = "2026-05-01T00:00:00Z" };

        var response = await client.PostAsJsonAsync("/walls", payload);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode),
            () => Assert.Single(walls),
            () => Assert.Contains(walls, RedWall)
        );

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private static DateTimeOffset Date(string date)
        => DateTimeOffset.ParseExact(
            $"{date}, 00:00Z",
            "d MMM yyyy, HH:mm'Z'",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    private static bool RedWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "red"
        && wall.BusinessStart == Date("1 May 2026")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("12 May 2026")
        && wall.SystemEnd == Infinity;
}
