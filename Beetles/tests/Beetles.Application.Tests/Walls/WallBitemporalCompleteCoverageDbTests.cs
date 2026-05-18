using Beetles.Application.Exceptions;
using Beetles.Application.Tests.Database;
using Beetles.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// Exhaustive bitemporal coverage. Each section enumerates the axes of variation for one
/// operation and pins the expected behaviour for every combination. Read in order:
///
///   INS — INSERT axes      (business interval, system time, id, color)
///   UPD — UPDATE axes      (business position relative to existing rows × system time)
///   DEL — DELETE axes      (business position × history depth)
///   QRY — GET (point-in-time) axes
///   AUD — system-time-travel / audit replay
///   INV — invariants under stress
///   CON — concurrency / EXCLUDE constraint
///   SCR — multi-step screenshot scenarios (TC-01..TC-06 end to end)
/// </summary>
[Collection("Database")]
public sealed class WallBitemporalCompleteCoverageDbTests : WallDbTestBase
{
    public WallBitemporalCompleteCoverageDbTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    // =====================================================================
    // INS — Insert
    // =====================================================================

    [Fact]
    public async Task INS01_Insert_FixesSystemStart_FromTimeProvider()
    {
        // A caller-supplied SystemStart is overwritten by TimeProvider.GetUtcNow().
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var entity = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            SystemStart = Utc(1999, 1, 1),   // should be ignored
        };
        await Repo.InsertAsync(entity, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var fromDb = Repo.QueryAll<Wall>().Single(w => w.Id == entity.Id);
        Assert.Equal(Utc(2025, 5, 10), fromDb.SystemStart);
        Assert.Equal(Infinity, fromDb.SystemEnd);
    }

    [Fact]
    public async Task INS02_Insert_DefaultsBusinessEnd_ToInfinity_WhenNotSpecified()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var entity = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1) };
        await Repo.InsertAsync(entity, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var fromDb = Repo.QueryAll<Wall>().Single(w => w.Id == entity.Id);
        Assert.Equal(Infinity, fromDb.BusinessEnd ?? Infinity);
    }

    [Fact]
    public async Task INS03_Insert_FutureBusinessStart_AllowsFutureGet_ButNotPresentGet()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 6, 1), "red");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, Utc(2025, 5, 15), CancellationToken.None));

        var future = await Repo.GetAsync<Wall>(id, Utc(2025, 6, 1), CancellationToken.None);
        Assert.Equal("red", future.Color);
    }

    [Fact]
    public async Task INS04_Insert_TwoWallsSameBusinessPeriod_GetDifferentIds()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id1 = await CreateWall(Utc(2025, 5, 1), "red");
        var id2 = await CreateWall(Utc(2025, 5, 1), "red");

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task INS05_Insert_ZeroLengthBusinessPeriod_IsAccepted_ButYieldsNoCoverage()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var wall = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 10),
            BusinessEnd = Utc(2025, 5, 10),
        };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        // Zero-length range covers no date → GetAsync at the boundary throws.
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(wall.Id, Utc(2025, 5, 10), CancellationToken.None));
    }

    [Fact]
    public async Task INS06_Insert_InvertedBusinessPeriod_IsRejectedByDb()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var wall = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 10),
            BusinessEnd = Utc(2025, 5, 5),
        };
        await Repo.InsertAsync(wall, CancellationToken.None);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            Repo.CommitChangesAsync(CancellationToken.None));
    }

    // =====================================================================
    // UPD — Update axes
    // =====================================================================

    [Fact]
    public async Task UPD01_UpdateInside_RetroactiveSplit_ProducesThreeRows()
    {
        await CreateDefaultWallThenBlue();

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity,        Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity,         "red"),
            Row(Utc(2025, 5, 3), Infinity,        Utc(2025, 5, 11), Infinity,         "blue")
        );
    }

    [Fact]
    public async Task UPD02_UpdateAtExactBusinessStart_OfCurrentRow_DoesNotAppendClosingRow()
    {
        // CloseAsync short-circuits when actual.BusinessStart == entity.BusinessStart.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 1), "green");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 11), Infinity,         "green")
        );
    }

    [Fact]
    public async Task UPD03_UpdateBeforeAllVersions_BackfillsHistory()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 5), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var wall = new Wall { Id = id, Color = "blue", BusinessStart = Utc(2025, 5, 1) };

        await Repo.UpdateAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Utc(2025, 5, 5), Utc(2025, 5, 11), Infinity, "blue"),
            Row(Utc(2025, 5, 5), Infinity, Utc(2025, 5, 10), Infinity, "red")
        );
    }

    [Fact]
    public async Task UPD03b_UpdateBeforeAllVersions_DifferentPayload_BackfillsOnlyMissingPeriod()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 5), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 1), "green");

        var beforeOriginalStart = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 4), CancellationToken.None);
        var atOriginalStart = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 5), CancellationToken.None);

        Assert.Equal("green", beforeOriginalStart.Color);
        Assert.Equal("red", atOriginalStart.Color);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Utc(2025, 5, 5), Utc(2025, 5, 11), Infinity, "green"),
            Row(Utc(2025, 5, 5), Infinity, Utc(2025, 5, 10), Infinity, "red")
        );
    }

    [Fact]
    public async Task UPD03c_UpdateBeforeAllVersions_SecondBackfill_ExtendsEarlierHistoryWithoutOverlap()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 5), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 1), "green");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 4, 20), "blue");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 4, 20), Utc(2025, 5, 1), Utc(2025, 5, 12), Infinity, "blue"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 5), Utc(2025, 5, 11), Infinity, "green"),
            Row(Utc(2025, 5, 5), Infinity, Utc(2025, 5, 10), Infinity, "red")
        );
    }

    [Fact]
    public async Task UPD04_UpdateAtExactBusinessEnd_OfFiniteRow_CreatesContiguousSegment()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var closed = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            BusinessEnd = Utc(2025, 5, 10),
        };
        await Repo.InsertAsync(closed, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var update = new Wall { Id = closed.Id, Color = "blue", BusinessStart = Utc(2025, 5, 10) };

        await Repo.UpdateAsync(update, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var activeRows = Repo.QueryAll<Wall>()
            .Where(w => w.Id == closed.Id && w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();

        Assert.Equal(2, activeRows.Count);
        Assert.Equal("red", activeRows[0].Color);
        Assert.Equal(Utc(2025, 5, 1), activeRows[0].BusinessStart);
        Assert.Equal(Utc(2025, 5, 10), activeRows[0].BusinessEnd);
        Assert.Equal("blue", activeRows[1].Color);
        Assert.Equal(Utc(2025, 5, 10), activeRows[1].BusinessStart);
        Assert.Equal(Infinity, activeRows[1].BusinessEnd ?? Infinity);
    }

    [Fact]
    public async Task UPD05_UpdateAtNonExistentId_ThrowsNotFound()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var ghost = new Wall { Id = int.MaxValue - 1, Color = "red", BusinessStart = Utc(2025, 5, 1) };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.UpdateAsync(ghost, CancellationToken.None));
    }

    [Fact]
    public async Task UPD06_Update_ExplicitBusinessEnd_IsIgnored_AndOverwrittenByActualBusinessEnd()
    {
        // The implementation passes `actual.BusinessEnd ?? MaxValue` to AppendAsync, ignoring
        // any BusinessEnd supplied by the caller. This test pins that contract — change it
        // deliberately if the contract is ever extended to honour a caller-supplied BusinessEnd.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var update = new Wall
        {
            Id = id,
            Color = "blue",
            BusinessStart = Utc(2025, 5, 3),
            BusinessEnd = Utc(2025, 5, 20),   // ignored
        };
        await Repo.UpdateAsync(update, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("blue", current.Color);
        Assert.Equal(Infinity, current.BusinessEnd ?? Infinity);   // not 20-May
    }

    [Fact]
    public async Task UPD07_Update_ActualHasFiniteBusinessEnd_AppendedRowInheritsThatEnd()
    {
        // When updating inside a finite "closed" segment, the new row inherits the segment's
        // BusinessEnd from the actual row that was found.
        //
        // Setup:        red [1-May .. 10-May)
        //               red [10-May .. ∞)
        // Update @ 5-May to blue:
        //               blue inherits the 1-May..10-May segment → BusinessEnd = 10-May.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var first = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1), BusinessEnd = Utc(2025, 5, 10) };
        await Repo.InsertAsync(first, CancellationToken.None);
        var second = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 10) };
        await Repo.InsertAsync(second, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        // The two rows have different ids (separate inserts). We update the first one.
        var id = first.Id;

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 5), "blue");

        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == id && w.SystemEnd == Infinity).ToList();
        var blueRow = Assert.Single(rows, r => r.Color == "blue");
        Assert.Equal(Utc(2025, 5, 5), blueRow.BusinessStart);
        Assert.Equal(Utc(2025, 5, 10), blueRow.BusinessEnd);
    }

    [Fact]
    public async Task UPD08_Update_WithSamePayload_PreservesCurrentState()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 1), "red");

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("red", current.Color);
    }

    [Fact]
    public async Task UPD09_ChainOfFiveUpdates_AllInvariantsHold()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        var schedule = new[]
        {
            (sys: Utc(2025, 5, 11), bs: Utc(2025, 5, 3),  color: "blue"),
            (sys: Utc(2025, 5, 12), bs: Utc(2025, 5, 5),  color: "green"),
            (sys: Utc(2025, 5, 13), bs: Utc(2025, 5, 7),  color: "yellow"),
            (sys: Utc(2025, 5, 14), bs: Utc(2025, 5, 9),  color: "purple"),
            (sys: Utc(2025, 5, 15), bs: Utc(2025, 5, 11), color: "black"),
        };

        foreach (var step in schedule)
        {
            Fixture.TimeProvider.SetUtcNow(step.sys);
            await UpdateWall(id, step.bs, step.color);
        }

        // Current view is "black".
        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("black", current.Color);

        // SystemStart is monotonic.
        var bySystemStart = Repo.QueryAll<Wall>().Where(w => w.Id == id).OrderBy(w => w.SystemStart).ToList();
        for (var i = 1; i < bySystemStart.Count; i++)
            Assert.True(bySystemStart[i - 1].SystemStart <= bySystemStart[i].SystemStart);

        // Exactly one current row.
        Assert.Single(Repo.QueryAll<Wall>().Where(w =>
            w.Id == id && w.BusinessEnd == Infinity && w.SystemEnd == Infinity));
    }

    // =====================================================================
    // DEL — Delete axes
    // =====================================================================

    [Fact]
    public async Task DEL01_Delete_InsideSingleVersionInterval_IsRejected_AndLeavesDbUnchanged()
    {
        // Option A: DELETE retracts an exact recorded event only. A date inside the interval
        // is not an event date, so it must be rejected without mutating history.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");
        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.DeleteAsync<Wall>(id, Utc(2025, 5, 5), CancellationToken.None));

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task DEL02_Delete_RevertsLastUpdate_AndRestoresPreviousColor()
    {
        var id = await CreateDefaultWallThenBlue();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, Utc(2025, 5, 3));

        await AssertCurrentColor(id, "red");
    }

    [Fact]
    public async Task DEL03_Delete_AtCreateDate_OfOnlyVersion_RetractsOnlyVersion()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await Repo.DeleteAsync<Wall>(id, Utc(2025, 5, 1), CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.Single(rows);
        Assert.Equal(Utc(2025, 5, 11), rows[0].SystemEnd);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, CancellationToken.None));
    }

    [Fact]
    public async Task DEL04_Delete_NonExistentId_ThrowsNotFound()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.DeleteAsync<Wall>(int.MaxValue - 2, Utc(2025, 5, 5), CancellationToken.None));
    }

    [Fact]
    public async Task DEL05_Delete_DateBeforeAnyVersion_ThrowsNotFound_And_LeavesDbUnchanged()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 6, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 5));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.DeleteAsync<Wall>(id, Utc(2025, 5, 1), CancellationToken.None));

        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.Single(rows);
        Assert.Equal(Infinity, rows[0].SystemEnd);
    }

    [Fact]
    public async Task DEL06_Delete_DateInFarFuture_IsRejected_AndLeavesDbUnchanged()
    {
        // Option A: the far future date is covered by the current open interval, but it is not
        // an exact recorded event date. DELETE must not create synthetic reassertions.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");
        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.DeleteAsync<Wall>(id, Utc(2099, 1, 1), CancellationToken.None));

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task DEL07_DoubleDelete_SecondHasNoFurtherEffect()
    {
        var id = await CreateDefaultWallThenBlue();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, Utc(2025, 5, 3));   // reverts blue → red

        var afterFirst = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        try
        {
            await DeleteWall(id, Utc(2025, 5, 3));
        }
        catch (NotFoundException)
        {
            // Replaying an already retracted event may be rejected; the invariant is that
            // rejected replay does not mutate persisted history.
        }

        var afterSecond = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Equal(afterFirst, afterSecond);
    }

    [Fact]
    public async Task DEL08_Delete_FirstEvent_WithLaterVersion_PreservesLaterVersion_AndLeavesGap()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, Utc(2025, 5, 1));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, Utc(2025, 5, 2), CancellationToken.None));

        var on3May = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 3), CancellationToken.None);
        Assert.Equal("blue", on3May.Color);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity,        Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Utc(2025, 5, 12), "red"),
            Row(Utc(2025, 5, 3), Infinity,        Utc(2025, 5, 11), Infinity,         "blue")
        );
    }

    [Fact]
    public async Task DEL09_Delete_MiddleEvent_WithLaterEvents_RevertsOnlyUntilNextEvent()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 3), "black");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 7), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 5, 8), "green");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        await DeleteWall(id, Utc(2025, 5, 7));

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 3), Infinity,        Utc(2025, 5, 10), Utc(2025, 5, 11), "black"),
            Row(Utc(2025, 5, 3), Utc(2025, 5, 7), Utc(2025, 5, 11), Utc(2025, 5, 13), "black"),
            Row(Utc(2025, 5, 7), Infinity,        Utc(2025, 5, 11), Utc(2025, 5, 12), "red"),
            Row(Utc(2025, 5, 7), Utc(2025, 5, 8), Utc(2025, 5, 12), Utc(2025, 5, 13), "red"),
            Row(Utc(2025, 5, 8), Infinity,        Utc(2025, 5, 12), Infinity,         "green"),
            Row(Utc(2025, 5, 3), Utc(2025, 5, 8), Utc(2025, 5, 13), Infinity,         "black")
        );
    }

    // =====================================================================
    // QRY — point-in-time GET
    // =====================================================================

    [Fact]
    public async Task QRY01_GetAt_BusinessStart_IsInclusive()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        var atStart = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 1), CancellationToken.None);
        Assert.Equal("red", atStart.Color);
    }

    [Fact]
    public async Task QRY02_GetAt_BusinessEnd_IsExclusive()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 5), "blue");

        var atBoundary = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 5), CancellationToken.None);
        Assert.Equal("blue", atBoundary.Color);
    }

    [Fact]
    public async Task QRY03_GetAt_OneTickBeforeBusinessStart_ThrowsNotFound()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, Utc(2025, 5, 1).AddTicks(-1), CancellationToken.None));
    }

    [Fact]
    public async Task QRY04_GetAt_NonExistentId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(int.MaxValue - 3, CancellationToken.None));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(int.MaxValue - 4, Utc(2025, 5, 5), CancellationToken.None));
    }

    [Fact]
    public async Task QRY05_GetAt_MaxValueDate_ReturnsNotFound_BecauseBusinessEndStrictly_Greater()
    {
        // GetAsync uses `BusinessEnd > date`. For BusinessEnd == MaxValue and date == MaxValue,
        // MaxValue > MaxValue is false → NotFound.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, DateTimeOffset.MaxValue, CancellationToken.None));
    }

    [Fact]
    public async Task QRY06_Get_OnFiniteOnlyRow_OutsideRange_ThrowsNotFound()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var wall = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1), BusinessEnd = Utc(2025, 5, 10) };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(wall.Id, Utc(2025, 5, 20), CancellationToken.None));
    }

    [Fact]
    public async Task QRY07_GetCurrent_OnFiniteOnlyActiveRow_Throws_ButPointInTimeGetWorks()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var wall = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            BusinessEnd = Utc(2025, 5, 10),
        };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(wall.Id, CancellationToken.None));

        var insideFinitePeriod = await Repo.GetAsync<Wall>(wall.Id, Utc(2025, 5, 5), CancellationToken.None);
        Assert.Equal("red", insideFinitePeriod.Color);
    }

    [Fact]
    public async Task UPD10_Update_InsideGapAfterDeletedFirstEvent_FillsOnlyGap()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, Utc(2025, 5, 1));

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        await UpdateWall(id, Utc(2025, 5, 2), "green");

        var on2May = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 2), CancellationToken.None);
        var on3May = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 3), CancellationToken.None);

        Assert.Equal("green", on2May.Color);
        Assert.Equal("blue", on3May.Color);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity,        Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Utc(2025, 5, 12), "red"),
            Row(Utc(2025, 5, 2), Utc(2025, 5, 3), Utc(2025, 5, 13), Infinity,         "green"),
            Row(Utc(2025, 5, 3), Infinity,        Utc(2025, 5, 11), Infinity,         "blue")
        );
    }

    [Fact]
    public async Task UPD11_Update_AfterFullDeletion_ReAddsNewVersion()
    {
        // After every event is deleted (wall disappears from current view), UpdateAsync
        // should re-introduce the wall under the same ID. EnsureExistsAsync finds the
        // superseded rows, FindAsync finds no active row, AppendAsync creates a fresh
        // open-ended segment.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await Repo.DeleteAsync<Wall>(id, Utc(2025, 5, 1), CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, CancellationToken.None));

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var update = new Wall { Id = id, Color = "blue", BusinessStart = Utc(2025, 5, 5) };
        await Repo.UpdateAsync(update, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("blue", current.Color);
        Assert.Equal(Utc(2025, 5, 5), current.BusinessStart);
        Assert.Equal(Infinity, current.BusinessEnd ?? Infinity);
        Assert.Equal(Infinity, current.SystemEnd);

        var allActive = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity)
            .ToList();
        Assert.Single(allActive);
    }

    // =====================================================================
    // AUD — system-time travel / audit replay
    // =====================================================================

    [Fact]
    public async Task AUD01_BeforeAnyKnowledge_NoRowsVisible()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        var asOf = Utc(2025, 5, 1);   // before SystemStart=10-May
        var rows = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemStart <= asOf && asOf < w.SystemEnd)
            .ToList();

        Assert.Empty(rows);
    }

    [Fact]
    public async Task AUD02_AtFutureSystemTime_SameAsCurrentView()
    {
        var id = await CreateDefaultWallThenBlue();

        var asOf = Utc(2099, 1, 1);
        var rows = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemStart <= asOf && asOf < w.SystemEnd)
            .ToList();

        var current = Repo.QueryAll<Wall>().Where(w => w.Id == id && w.SystemEnd == Infinity).ToList();
        Assert.Equal(current.Count, rows.Count);
        Assert.Equal(
            current.Select(r => r.Color).OrderBy(c => c),
            rows.Select(r => r.Color).OrderBy(c => c));
    }

    [Fact]
    public async Task AUD03_MultipleCheckpoints_VisibleColorChanges()
    {
        // Checkpoint at three system times across two updates.
        var id = await CreateDefaultWallThenBlueThenBlack();

        // As of 10.5 → only red is known.
        // As of 12.5 → red+blue.
        // As of 16.6 → red+blue+black.
        DateTimeOffset[] checkpoints = [Utc(2025, 5, 10), Utc(2025, 5, 12), Utc(2025, 6, 16)];
        int[] expectedCounts = [1, 2, 3];

        for (var i = 0; i < checkpoints.Length; i++)
        {
            var t = checkpoints[i];
            var rows = Repo.QueryAll<Wall>()
                .Where(w => w.Id == id && w.SystemStart <= t && t < w.SystemEnd)
                .ToList();
            Assert.Equal(expectedCounts[i], rows.Count);
        }
    }

    // =====================================================================
    // INV — Invariants under stress
    // =====================================================================

    [Fact]
    public async Task INV01_ActiveBusinessIntervals_NeverOverlap_AcrossManyUpdates()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        for (var i = 1; i <= 10; i++)
        {
            Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10 + i));
            await UpdateWall(id, Utc(2025, 5, i), $"color-{i}");
        }

        var actives = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();

        for (var i = 1; i < actives.Count; i++)
        {
            var prevEnd = actives[i - 1].BusinessEnd ?? Infinity;
            Assert.True(prevEnd <= actives[i].BusinessStart,
                $"Overlap between {actives[i - 1].BusinessStart:o}-{prevEnd:o} and {actives[i].BusinessStart:o}");
        }
    }

    [Fact]
    public async Task INV02_RowCount_OnlyEverGrows_AcrossOperations()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        long count = Repo.QueryAll<Wall>().Where(w => w.Id == id).LongCount();

        async Task TickAndAssertGrowth(DateTimeOffset sys, Func<Task> op)
        {
            Fixture.TimeProvider.SetUtcNow(sys);
            await op();
            var newCount = Repo.QueryAll<Wall>().Where(w => w.Id == id).LongCount();
            Assert.True(newCount > count, $"Expected row count to grow (was {count}, now {newCount})");
            count = newCount;
        }

        await TickAndAssertGrowth(Utc(2025, 5, 11), () => UpdateWall(id, Utc(2025, 5, 3), "blue"));
        await TickAndAssertGrowth(Utc(2025, 5, 12), () => UpdateWall(id, Utc(2025, 5, 5), "green"));
        await TickAndAssertGrowth(Utc(2025, 5, 13), () => DeleteWall(id, Utc(2025, 5, 5)));
    }

    [Fact]
    public async Task INV03_ExactlyOneCurrentRow_AcrossAllOperations()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        AssertExactlyOneCurrentRow(id, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");
        AssertExactlyOneCurrentRow(id, "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 5, 5), "green");
        AssertExactlyOneCurrentRow(id, "green");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        await DeleteWall(id, Utc(2025, 5, 5));
        AssertExactlyOneCurrentRow(id, "blue");
    }

    [Fact]
    public async Task INV04_PreviousSystemEnd_EqualsNextSystemStart_ForClosedRows()
    {
        var id = await CreateDefaultWallThenBlue();

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        var closed = all.Where(w => w.SystemEnd != Infinity);

        foreach (var c in closed)
            Assert.Contains(all, r => r.SystemStart == c.SystemEnd);
    }

    [Fact]
    public async Task INV05_TransactionIds_Unique_AcrossAllRows()
    {
        var id = await CreateDefaultWallThenBlueThenBlack();

        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.Equal(rows.Count, rows.Select(r => r.TransactionId).Distinct().Count());
    }

    // =====================================================================
    // CON — Concurrency / EXCLUDE constraint
    // =====================================================================

    [Fact]
    public async Task CON01_TwoManualInserts_OverlappingBitemporal_SameId_RejectedByExclusionConstraint()
    {
        // Insert two rows with the SAME id and overlapping business+system periods.
        // The exclusion constraint must reject the second.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var first = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1) };
        await Repo.InsertAsync(first, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var second = new Wall { Id = first.Id, Color = "blue", BusinessStart = Utc(2025, 5, 1) };
        await Repo.InsertAsync(second, CancellationToken.None);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            Repo.CommitChangesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task CON02_TwoManualInserts_NonOverlappingBusiness_SameId_Allowed()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var first = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            BusinessEnd = Utc(2025, 5, 10),
        };
        await Repo.InsertAsync(first, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var second = new Wall
        {
            Id = first.Id,
            Color = "blue",
            BusinessStart = Utc(2025, 5, 10),
        };
        await Repo.InsertAsync(second, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == first.Id).ToList();
        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public async Task CON03_TwoManualInserts_OverlappingBusiness_DisjointSystem_SameId_Allowed()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var historical = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            SystemEnd = Utc(2025, 5, 11),
        };
        await Repo.InsertAsync(historical, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var current = new Wall
        {
            Id = historical.Id,
            Color = "blue",
            BusinessStart = Utc(2025, 5, 1),
        };
        await Repo.InsertAsync(current, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == historical.Id).ToList();
        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, w => w.Color == "red" && w.SystemEnd == Utc(2025, 5, 11));
        Assert.Contains(rows, w => w.Color == "blue" && w.SystemStart == Utc(2025, 5, 11));
    }

    [Fact]
    public async Task CON04_TwoManualInserts_OverlappingBitemporal_DifferentIds_Allowed()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var first = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1) };
        var second = new Wall { Color = "blue", BusinessStart = Utc(2025, 5, 1) };

        await Repo.InsertAsync(first, CancellationToken.None);
        await Repo.InsertAsync(second, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(2, Repo.QueryAll<Wall>().Count(w => w.SystemStart == Utc(2025, 5, 10)));
    }

    // =====================================================================
    // SCR — End-to-end screenshot scenarios
    // =====================================================================

    [Fact]
    public async Task SCR_FullScreenshotChronology_Tc01_through_Tc05()
    {
        // The exact chronology described in testcases-from-screenshot.md, run in one test so the
        // entire bitemporal evolution is verified by a single AssertWallsAsync at the end.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 15));
        await UpdateWall(id, Utc(2025, 6, 13), "black");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 18));
        await UpdateWall(id, Utc(2025, 6, 1), "white");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 20));
        await UpdateWall(id, Utc(2025, 5, 3), "light-blue");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity,         Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3),  Utc(2025, 5, 11), Infinity,         "red"),
            Row(Utc(2025, 5, 3), Infinity,         Utc(2025, 5, 11), Utc(2025, 6, 15), "blue"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 13), Utc(2025, 6, 15), Utc(2025, 6, 18), "blue"),
            Row(Utc(2025, 6, 13), Infinity,        Utc(2025, 6, 15), Infinity,         "black"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 1),  Utc(2025, 6, 18), Utc(2025, 6, 20), "blue"),
            Row(Utc(2025, 6, 1), Utc(2025, 6, 13), Utc(2025, 6, 18), Infinity,         "white"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 1),  Utc(2025, 6, 20), Infinity,         "light-blue")
        );
    }

    [Fact]
    public async Task SCR_Tc06_NoRepaint_DeleteRetroactsBlackChange_WhiteRemainsCurrent()
    {
        // TC-06 from the screenshot: on 25-Jun, record that the 13-Jun repaint did NOT happen.
        // The "black" change at 13-Jun is reverted; the prior state (white, ending at 13-Jun)
        // becomes effective for that period.
        //
        // Verification path: after the delete, the current view at 14-Jun must report "white"
        // (the inheriting open-ended segment that the revert produces).
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 15));
        await UpdateWall(id, Utc(2025, 6, 13), "black");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 18));
        await UpdateWall(id, Utc(2025, 6, 1), "white");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 25));
        await DeleteWall(id, Utc(2025, 6, 13));

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("white", current.Color);

        // Point-in-time on 14-Jun must also be white (the reverted state covers it).
        var on14Jun = await Repo.GetAsync<Wall>(id, Utc(2025, 6, 14), CancellationToken.None);
        Assert.Equal("white", on14Jun.Color);
    }
}
