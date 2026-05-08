using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Beetles.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Beetles.Infrastructure.Tests.Repositories;

[Collection("Database Collection")]
public sealed class WallRepositoryTest(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseContext _databaseContext = fixture.DatabaseContext;

    private readonly Mock<TimeProvider> _timeProviderMock = fixture.TimeProviderMock;

    private readonly IBitemporalRepository _repository = fixture.BitemporalRepository;

    private static readonly DateTimeOffset Infinity = DateTimeOffset.MaxValue;

    public async Task InitializeAsync()
    {
        await _databaseContext.Database.ExecuteSqlRawAsync("TRUNCATE walls", CancellationToken.None);
        _databaseContext.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    { }

    [Fact]
    public async Task ShouldCreateRedWallAsync()
    {
        await CreateRedWallAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Single(walls),
            () => Assert.Contains(walls, RedWall)
        );
    }

    [Fact]
    public async Task ShouldUpdateBlueWallAsync()
    {
        await CreateRedWallAsync(CancellationToken.None);
        await UpdateBlueWallAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(3, walls.Count),
            () => Assert.Contains(walls, SupersededRedWall),
            () => Assert.Contains(walls, ClosedRedWall),
            () => Assert.Contains(walls, BlueWall)
        );
    }

    [Fact]
    public async Task ShouldUpdateBlackWallAsync()
    {
        await CreateRedWallAsync(CancellationToken.None);
        await UpdateBlueWallAsync(CancellationToken.None);
        await UpdateBlackWallAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(5, walls.Count),
            () => Assert.Contains(walls, SupersededRedWall),
            () => Assert.Contains(walls, ClosedRedWall),
            () => Assert.Contains(walls, SupersedBlueWall),
            () => Assert.Contains(walls, ClosedBlueWall),
            () => Assert.Contains(walls, BlackWall)
        );
    }

    private static DateTimeOffset Date(string date)
        => DateTimeOffset.Parse($"{date}T00:00:00Z");

    private async Task CreateRedWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("2025-05-10"));

        var wall = new Wall { Id = 1, Color = "red", BusinessStart = Date("2025-05-01") };

        await _repository.InsertAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task UpdateBlueWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("2025-05-11"));

        var wall = new Wall { Id = 1, Color = "blue", BusinessStart = Date("2025-05-03") };

        await _repository.UpdateAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task UpdateBlackWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("2025-06-15"));

        var wall = new Wall { Id = 1, Color = "black", BusinessStart = Date("2025-06-13") };

        await _repository.UpdateAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private static bool RedWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "red"
        && wall.BusinessStart == Date("2025-05-01")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("2025-05-10")
        && wall.SystemEnd == Infinity;

    private static bool SupersededRedWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "red"
        && wall.BusinessStart == Date("2025-05-01")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("2025-05-10")
        && wall.SystemEnd == Date("2025-05-11");


    private static bool ClosedRedWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "red"
        && wall.BusinessStart == Date("2025-05-01")
        && wall.BusinessEnd == Date("2025-05-03")
        && wall.SystemStart == Date("2025-05-11")
        && wall.SystemEnd == Infinity;

    private static bool BlueWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("2025-05-03")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("2025-05-11")
        && wall.SystemEnd == Infinity;

    private static bool SupersedBlueWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("2025-05-03")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("2025-05-11")
        && wall.SystemEnd == Date("2025-06-15");

    private static bool ClosedBlueWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("2025-05-03")
        && wall.BusinessEnd == Date("2025-06-13")
        && wall.SystemStart == Date("2025-06-15")
        && wall.SystemEnd == Infinity;

    private static bool BlackWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "black"
        && wall.BusinessStart == Date("2025-06-13")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("2025-06-15")
        && wall.SystemEnd == Infinity;
}
