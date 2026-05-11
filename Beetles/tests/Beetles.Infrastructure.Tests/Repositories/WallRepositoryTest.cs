using System.Globalization;

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

    [Fact]
    public async Task ShouldUpdateWhiteWallAsync()
    {
        await CreateRedWallAsync(CancellationToken.None);
        await UpdateBlueWallAsync(CancellationToken.None);
        await UpdateBlackWallAsync(CancellationToken.None);
        await UpdateWhiteWallAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(7, walls.Count),
            () => Assert.Contains(walls, SupersededRedWall),
            () => Assert.Contains(walls, ClosedRedWall),
            () => Assert.Contains(walls, SupersedBlueWall),
            () => Assert.Contains(walls, DeadBlueWall),
            () => Assert.Contains(walls, BlackWall),
            () => Assert.Contains(walls, ClosedBlueWall2),
            () => Assert.Contains(walls, ClosedWhiteWall)
        );
    }

    [Fact]
    public async Task ShouldUpdateYellowWallAsync()
    {
        await CreateRedWallAsync(CancellationToken.None);
        await UpdateBlueWallAsync(CancellationToken.None);
        await UpdateBlackWallAsync(CancellationToken.None);
        await UpdateWhiteWallAsync(CancellationToken.None);
        await UpdateYellowWallAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(8, walls.Count),
            () => Assert.Contains(walls, SupersededRedWall),
            () => Assert.Contains(walls, ClosedRedWall),
            () => Assert.Contains(walls, SupersedBlueWall),
            () => Assert.Contains(walls, DeadBlueWall),
            () => Assert.Contains(walls, BlackWall),
            () => Assert.Contains(walls, DeadBlueWall2),
            () => Assert.Contains(walls, ClosedWhiteWall),
            () => Assert.Contains(walls, ClosedYellowWall)
        );
    }

    [Fact]
    public async Task ShouldDeleteAsync()
    {
        await CreateRedWallAsync(CancellationToken.None);
        await UpdateBlueWallAsync(CancellationToken.None);
        await UpdateBlackWallAsync(CancellationToken.None);
        await UpdateWhiteWallAsync(CancellationToken.None);
        await UpdateYellowWallAsync(CancellationToken.None);

        await DeleteAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(9, walls.Count),
            () => Assert.Contains(walls, SupersededRedWall),
            () => Assert.Contains(walls, ClosedRedWall),
            () => Assert.Contains(walls, SupersedBlueWall),
            () => Assert.Contains(walls, DeadBlueWall),
            () => Assert.Contains(walls, SupersedBlackWall),
            () => Assert.Contains(walls, DeadBlueWall2),
            () => Assert.Contains(walls, DeadWhiteWall),
            () => Assert.Contains(walls, ClosedYellowWall),
            () => Assert.Contains(walls, WhiteWall)
        );
    }

    private static DateTimeOffset Date(string date)
        => DateTimeOffset.ParseExact(
            $"{date}, 00:00Z",
            "d MMM yyyy, HH:mm'Z'",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    private async Task CreateRedWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("10 May 2025"));

        var wall = new Wall { Id = 1, Color = "red", BusinessStart = Date("1 May 2025") };

        await _repository.InsertAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task UpdateBlueWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("11 May 2025"));

        var wall = new Wall { Id = 1, Color = "blue", BusinessStart = Date("3 May 2025") };

        await _repository.UpdateAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task UpdateBlackWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("15 Jun 2025"));

        var wall = new Wall { Id = 1, Color = "black", BusinessStart = Date("13 Jun 2025") };

        await _repository.UpdateAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task UpdateWhiteWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("18 Jun 2025"));

        var wall = new Wall { Id = 1, Color = "white", BusinessStart = Date("1 Jun 2025") };

        await _repository.UpdateAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task UpdateYellowWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("20 Jun 2025"));

        var wall = new Wall { Id = 1, Color = "yellow", BusinessStart = Date("3 May 2025") };

        await _repository.UpdateAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task DeleteAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("25 Jun 2025"));

        await _repository.DeleteAsync<Wall>(1, Date("13 Jun 2025"), cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private static bool RedWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "red"
        && wall.BusinessStart == Date("1 May 2025")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("10 May 2025")
        && wall.SystemEnd == Infinity;

    private static bool SupersededRedWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "red"
        && wall.BusinessStart == Date("1 May 2025")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("10 May 2025")
        && wall.SystemEnd == Date("11 May 2025");

    private static bool ClosedRedWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "red"
        && wall.BusinessStart == Date("1 May 2025")
        && wall.BusinessEnd == Date("3 May 2025")
        && wall.SystemStart == Date("11 May 2025")
        && wall.SystemEnd == Infinity;

    private static bool BlueWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("3 May 2025")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("11 May 2025")
        && wall.SystemEnd == Infinity;

    private static bool SupersedBlueWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("3 May 2025")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("11 May 2025")
        && wall.SystemEnd == Date("15 Jun 2025");

    private static bool ClosedBlueWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("3 May 2025")
        && wall.BusinessEnd == Date("13 Jun 2025")
        && wall.SystemStart == Date("15 Jun 2025")
        && wall.SystemEnd == Infinity;

    private static bool BlackWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "black"
        && wall.BusinessStart == Date("13 Jun 2025")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("15 Jun 2025")
        && wall.SystemEnd == Infinity;

    private static bool DeadBlueWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("3 May 2025")
        && wall.BusinessEnd == Date("13 Jun 2025")
        && wall.SystemStart == Date("15 Jun 2025")
        && wall.SystemEnd == Date("18 Jun 2025");

    private static bool ClosedBlueWall2(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("3 May 2025")
        && wall.BusinessEnd == Date("1 Jun 2025")
        && wall.SystemStart == Date("18 Jun 2025")
        && wall.SystemEnd == Infinity;

    private static bool ClosedWhiteWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "white"
        && wall.BusinessStart == Date("1 Jun 2025")
        && wall.BusinessEnd == Date("13 Jun 2025")
        && wall.SystemStart == Date("18 Jun 2025")
        && wall.SystemEnd == Infinity;

    public static bool ClosedYellowWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "yellow"
        && wall.BusinessStart == Date("3 May 2025")
        && wall.BusinessEnd == Date("1 Jun 2025")
        && wall.SystemStart == Date("20 Jun 2025")
        && wall.SystemEnd == Infinity;

    private static bool DeadBlueWall2(Wall wall)
        => wall.Id == 1
        && wall.Color == "blue"
        && wall.BusinessStart == Date("3 May 2025")
        && wall.BusinessEnd == Date("1 Jun 2025")
        && wall.SystemStart == Date("18 Jun 2025")
        && wall.SystemEnd == Date("20 Jun 2025");

    private static bool SupersedBlackWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "black"
        && wall.BusinessStart == Date("13 Jun 2025")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("15 Jun 2025")
        && wall.SystemEnd == Date("25 Jun 2025");

    private static bool DeadWhiteWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "white"
        && wall.BusinessStart == Date("1 Jun 2025")
        && wall.BusinessEnd == Date("13 Jun 2025")
        && wall.SystemStart == Date("18 Jun 2025")
        && wall.SystemEnd == Date("25 Jun 2025");

    private static bool WhiteWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "white"
        && wall.BusinessStart == Date("1 Jun 2025")
        && wall.BusinessEnd == Infinity
        && wall.SystemStart == Date("25 Jun 2025")
        && wall.SystemEnd == Infinity;
}
