using Beetles.Application.Exceptions;
using Beetles.Application.Tests.Database;
using Beetles.Domain.Entities;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// Bitemporal-correctness tests that are unique to this file (not covered by
/// WallBitemporalCompleteCoverageDbTests). Removed duplicates: AD01–AD03 (→QRY01–03),
/// AD10/AD11 (→DEL04/05), AD20 (→UPD05), AD30/AD31 (→INV03/INV01),
/// AD40/AD41 (→AUD01/02), AD50 (→B04), AD61 (→INV05).
/// </summary>
[Collection("Database")]
public sealed class WallBitemporalAdvancedDbTests : WallDbTestBase
{
    public WallBitemporalAdvancedDbTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    // ---------------------------------------------------------------------
    // Delete — structural detail not fully covered by DEL01/DEL02
    // ---------------------------------------------------------------------

    [Fact]
    public async Task AD04_Delete_RevertsToPriorColor_AndClosesPreviousRow()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, Utc(2025, 5, 3));

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("red", current.Color);

        var blueRows = Repo.QueryAll<Wall>().Where(w => w.Id == id && w.Color == "blue").ToList();
        Assert.NotEmpty(blueRows);
        Assert.All(blueRows, b => Assert.NotEqual(Infinity, b.SystemEnd));

        var active = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity && w.BusinessEnd == Infinity)
            .ToList();
        Assert.Single(active);
        Assert.Equal("red", active[0].Color);
        Assert.Equal(Utc(2025, 5, 12), active[0].SystemStart);
    }

    [Fact]
    public async Task AD04b_DeleteAtCreateDate_OfOnlyVersion_RetractsOnlyVersion()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await Repo.DeleteAsync<Wall>(id, Utc(2025, 5, 1), CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.Single(rows);
        Assert.Equal("red", rows[0].Color);
        Assert.Equal(Utc(2025, 5, 1), rows[0].BusinessStart);
        Assert.Equal(Infinity, rows[0].BusinessEnd ?? Infinity);
        Assert.Equal(Utc(2025, 5, 10), rows[0].SystemStart);
        Assert.Equal(Utc(2025, 5, 11), rows[0].SystemEnd);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, CancellationToken.None));
    }

    [Fact]
    public async Task AD12_Delete_InsideSingleVersionInterval_IsRejected_AndLeavesDbUnchanged()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");
        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.DeleteAsync<Wall>(id, Utc(2025, 5, 5), CancellationToken.None));

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }

    // ---------------------------------------------------------------------
    // Update — structural detail
    // ---------------------------------------------------------------------

    [Fact]
    public async Task AD21_Update_PreservesEntityId()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal(id, current.Id);
        Assert.Equal("blue", current.Color);
    }

    // ---------------------------------------------------------------------
    // Invariants — not covered by INV01–INV05
    // ---------------------------------------------------------------------

    [Fact]
    public async Task AD32_HistoricalRows_HaveFiniteSystemEnd()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        var historical = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd != Infinity)
            .ToList();

        Assert.NotEmpty(historical);
        Assert.All(historical, w =>
            Assert.True(w.SystemEnd > w.SystemStart,
                $"Row {w.TransactionId} has SystemEnd <= SystemStart"));
    }

    // ---------------------------------------------------------------------
    // Multi-wall isolation — unique scenario (5 walls)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task AD51_ManyWalls_EachKeepsTheirOwnColor()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var colors = new[] { "red", "blue", "green", "yellow", "purple" };
        var ids = new List<int>();
        foreach (var color in colors)
            ids.Add(await CreateWall(Utc(2025, 5, 1), color));

        for (var i = 0; i < ids.Count; i++)
        {
            var wall = await Repo.GetAsync<Wall>(ids[i], CancellationToken.None);
            Assert.Equal(colors[i], wall.Color);
        }
    }

    // ---------------------------------------------------------------------
    // QueryAll — detachment and repeat-update semantics
    // ---------------------------------------------------------------------

    [Fact]
    public async Task AD60_QueryAll_ReturnsEntitiesDetached()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        var snapshot = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.NotEmpty(snapshot);

        foreach (var w in snapshot)
            w.Color = "mutated-in-memory";

        var reloaded = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.All(reloaded, w => Assert.Equal("red", w.Color));
    }

    [Fact]
    public async Task AD70_RepeatUpdate_AtSameSystemTime_PreservesCurrentState()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("blue", current.Color);
    }
}
