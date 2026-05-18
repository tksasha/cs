using Beetles.Application.Common.Interfaces;
using Beetles.Application.Tests.Database;
using Beetles.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

public abstract class WallDbTestBase : IAsyncLifetime
{
    private IServiceScope? _scope;

    protected readonly DatabaseFixture Fixture;

    protected WallDbTestBase(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    protected IBitemporalRepository Repo =>
        _scope!.ServiceProvider.GetRequiredService<IBitemporalRepository>();

    public async Task InitializeAsync()
    {
        await Fixture.ResetAsync();
        _scope = Fixture.CreateScope();
    }

    public Task DisposeAsync()
    {
        _scope?.Dispose();
        return Task.CompletedTask;
    }

    protected Task<int> CreateWall(DateTimeOffset businessStart, string color, DateTimeOffset? businessEnd = null)
        => WallDbHelper.CreateWall(Repo, businessStart, color, businessEnd);

    protected Task UpdateWall(int wallId, DateTimeOffset businessStart, string color, DateTimeOffset? businessEnd = null)
        => WallDbHelper.UpdateWall(Repo, wallId, businessStart, color, businessEnd);

    protected Task DeleteWall(int wallId, DateTimeOffset date)
        => WallDbHelper.DeleteWall(Repo, wallId, date);

    protected async Task<int> CreateDefaultWall()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        return await CreateWall(Utc(2025, 5, 1), "red");
    }

    protected async Task<int> CreateDefaultWallThenBlue()
    {
        var id = await CreateDefaultWall();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        return id;
    }

    protected async Task<int> CreateDefaultWallThenBlueThenBlack()
    {
        var id = await CreateDefaultWallThenBlue();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 15));
        await UpdateWall(id, Utc(2025, 6, 13), "black");

        return id;
    }

    protected async Task AssertCurrentColor(int wallId, string color)
    {
        var current = await Repo.GetAsync<Wall>(wallId, CancellationToken.None);
        Assert.Equal(color, current.Color);
    }

    protected List<Wall> WallRows(int wallId)
        => Repo.QueryAll<Wall>().Where(w => w.Id == wallId).ToList();

    protected List<Wall> ActiveWallRows(int wallId)
        => WallRows(wallId)
            .Where(w => w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();

    protected void AssertActiveRowsDoNotOverlap(int wallId)
    {
        var activeRows = ActiveWallRows(wallId);

        for (var i = 1; i < activeRows.Count; i++)
        {
            var previousEnd = activeRows[i - 1].BusinessEnd ?? Infinity;
            Assert.True(
                previousEnd <= activeRows[i].BusinessStart,
                $"Active business intervals overlap for wall {wallId}: {activeRows[i - 1].BusinessStart:o}-{previousEnd:o} vs {activeRows[i].BusinessStart:o}");
        }
    }

    protected void AssertExactlyOneCurrentRow(int wallId, string color)
    {
        var current = Repo.QueryAll<Wall>()
            .Where(w => w.Id == wallId && w.BusinessEnd == Infinity && w.SystemEnd == Infinity)
            .ToList();

        Assert.Single(current);
        Assert.Equal(color, current[0].Color);
    }
}
