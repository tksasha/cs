using Beetles.Application.Common.Interfaces;
using Beetles.Application.Exceptions;
using Beetles.Domain.Entities;
using Beetles.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace Beetles.Infrastructure.Tests.Repositories;

[Collection("Database Collection")]
public sealed class WallRepositoryTest(DatabaseFixture fixture) : AbstractRepositoryTest, IAsyncLifetime
{
    private readonly DatabaseContext _databaseContext = fixture.DatabaseContext;

    private readonly Mock<TimeProvider> _timeProviderMock = fixture.TimeProviderMock;

    private readonly IBitemporalRepository _repository = fixture.BitemporalRepository;

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
        var cancellationToken = CancellationToken.None;

        await CreateWallAsync(id: 1, color: "red", businessStart: "1 May 2025", now: "10 May 2025", cancellationToken);

        await UpdateWallAsync(id: 1, color: "blue",
            businessStart: "3 May 2025", now: "11 May 2025", cancellationToken);
        await UpdateWallAsync(id: 1, color: "black",
            businessStart: "13 Jun 2025", now: "15 Jun 2025", cancellationToken);
        await UpdateWallAsync(id: 1, color: "white",
            businessStart: "1 Jun 2025", now: "18 Jun 2025", cancellationToken);
        await UpdateWallAsync(id: 1, color: "yellow",
            businessStart: "3 May 2025", now: "20 Jun 2025", cancellationToken);

        await DeleteWallAsync(id: 1, businessStart: "13 Jun 2025", now: "25 Jun 2025", cancellationToken);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(9, walls.Count),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "red",
                businessStart: "1 May 2025",
                businessEnd: "infinity",
                systemStart: "10 May 2025",
                systemEnd: "11 May 2025")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "red",
                businessStart: "1 May 2025",
                businessEnd: "3 May 2025",
                systemStart: "11 May 2025",
                systemEnd: "infinity")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "blue",
                businessStart: "3 May 2025",
                businessEnd: "infinity",
                systemStart: "11 May 2025",
                systemEnd: "15 Jun 2025")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "blue",
                businessStart: "3 May 2025",
                businessEnd: "13 Jun 2025",
                systemStart: "15 Jun 2025",
                systemEnd: "18 Jun 2025")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "black",
                businessStart: "13 Jun 2025",
                businessEnd: "infinity",
                systemStart: "15 Jun 2025",
                systemEnd: "25 Jun 2025")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "blue",
                businessStart: "3 May 2025",
                businessEnd: "1 Jun 2025",
                systemStart: "18 Jun 2025",
                systemEnd: "20 Jun 2025")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "white",
                businessStart: "1 Jun 2025",
                businessEnd: "13 Jun 2025",
                systemStart: "18 Jun 2025",
                systemEnd: "25 Jun 2025")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "yellow",
                businessStart: "3 May 2025",
                businessEnd: "1 Jun 2025",
                systemStart: "20 Jun 2025",
                systemEnd: "infinity")),
            () => Assert.Contains(walls, Wall(
                id: 1,
                color: "white",
                businessStart: "1 Jun 2025",
                businessEnd: "infinity",
                systemStart: "25 Jun 2025",
                systemEnd: "infinity"))
        );

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    [Fact]
    public async Task ShouldUpdateRedisAsync()
    {
        await CreateRedWallAsync(CancellationToken.None);
        await UpdateRedisWallAsync(CancellationToken.None);

        var walls = await _repository.QueryAll<Wall>().ToListAsync(CancellationToken.None);

        Assert.Multiple(
            () => Assert.Equal(2, walls.Count),
            () => Assert.Contains(walls, RedWall),
            () => Assert.Contains(walls, ClosedRedisWall)
        );
    }

    [Fact]
    public async Task Update_ShouldThrowNotFoundException()
    {
        var wall = new Wall { Id = 123, Color = "white" };

        await Assert.ThrowsAnyAsync<NotFoundException>(async () =>
            await _repository.UpdateAsync(wall, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_ShouldThrowNotFoundException()
    {
        var cancellationToken = CancellationToken.None;

        var wall = await CreateWallAsync(
            id: 22, color: "brown", businessStart: "1 May 2025", now: "13 May 2025", cancellationToken);

        wall = await UpdateWallAsync(
            id: wall.Id, color: "brown", businessStart: "3 May 2025", now: "14 May 2025", cancellationToken);

        await DeleteWallAsync(id: wall.Id, businessStart: wall.BusinessStart, now: "15 May 2026", cancellationToken);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _repository.DeleteAsync<Wall>(wall.Id, wall.BusinessStart, cancellationToken));
    }

    [Fact]
    public async Task ShouldDeleteIntradayRecord()
    {
        var cancellationToken = CancellationToken.None;

        var wall = await CreateWallAsync(
            id: 43,
            color: "red",
            businessStart: DateTimeOffset.Parse("2025-05-01T10:00:00Z"),
            now: "1 Jun 2026",
            cancellationToken);

        wall = await UpdateWallAsync(
            id: wall.Id,
            color: "blue",
            businessStart: DateTimeOffset.Parse("2025-05-01T14:00:00Z"),
            now: "1 Jun 2026",
            cancellationToken);

        await _repository.DeleteAsync<Wall>(wall.Id, wall.BusinessStart, CancellationToken.None);
    }

    [Fact]
    public async Task Delete_ShouldThrowExceptionWhenDateDoesNotExist()
    {
        var cancellationToken = CancellationToken.None;

        var wall = await CreateWallAsync(id: 13, color: "red", businessStart: "1 May 2025", now: "1 Jun 2026", cancellationToken);

        await UpdateWallAsync(id: wall.Id, color: "blue", businessStart: "10 May 2025", now: "2 Jun 2026", cancellationToken);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _repository.DeleteAsync<Wall>(wall.Id, DateTimeOffset.Parse("2025-05-09T23:59:59.9999999Z"), cancellationToken));
    }

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

    private Task DeleteWallAsync(int id, string businessStart, string now, CancellationToken cancellationToken)
        => DeleteWallAsync(id, Date(businessStart), now, cancellationToken);

    private async Task DeleteWallAsync(int id, DateTimeOffset businessStart, string now, CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date(now));

        await _repository.DeleteAsync<Wall>(id, businessStart, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private async Task UpdateRedisWallAsync(CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date("10 May 2025"));

        var wall = new Wall { Id = 1, Color = "redis", BusinessStart = Date("1 Apr 2025") };

        await _repository.UpdateAsync(wall, cancellationToken);

        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());
    }

    private Task<Wall> CreateWallAsync(int id, string color, string businessStart, string now, CancellationToken cancellationToken)
        => CreateWallAsync(id, color, businessStart: Date(businessStart), now, cancellationToken);

    private async Task<Wall> CreateWallAsync(int id, string color, DateTimeOffset businessStart, string now, CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date(now));

        var wall = new Wall { Id = id, Color = color, BusinessStart = businessStart };

        await _repository.InsertAsync(wall, cancellationToken);
        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());

        return wall;
    }

    private Task<Wall> UpdateWallAsync(
        int id, string color, string businessStart, string now, CancellationToken cancellationToken)
        => UpdateWallAsync(id, color, businessStart: Date(businessStart), now, cancellationToken);

    private async Task<Wall> UpdateWallAsync(
        int id, string color, DateTimeOffset businessStart, string now, CancellationToken cancellationToken)
    {
        _timeProviderMock
            .Setup(p => p.GetUtcNow())
            .Returns(Date(now));

        var wall = new Wall { Id = id, Color = color, BusinessStart = businessStart };

        await _repository.UpdateAsync(wall, cancellationToken);
        await _repository.CommitChangesAsync(cancellationToken);

        _timeProviderMock.Verify(p => p.GetUtcNow());

        return wall;
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

    private static bool ClosedRedisWall(Wall wall)
        => wall.Id == 1
        && wall.Color == "redis"
        && wall.BusinessStart == Date("1 Apr 2025")
        && wall.BusinessEnd == Date("1 May 2025")
        && wall.SystemStart == Date("10 May 2025")
        && wall.SystemEnd == Infinity;

    private static Predicate<Wall> Wall(
        int id,
        string color,
        string businessStart,
        string systemStart,
        string businessEnd,
        string systemEnd)
    {
        return (wall) =>
            wall.Id == id &&
            wall.Color == color &&
            wall.BusinessStart == Date(businessStart) &&
            wall.BusinessEnd == (businessEnd == "infinity" ? Infinity : Date(businessEnd)) &&
            wall.SystemStart == Date(systemStart) &&
            wall.SystemEnd == (systemEnd == "infinity" ? Infinity : Date(systemEnd));
    }
}
