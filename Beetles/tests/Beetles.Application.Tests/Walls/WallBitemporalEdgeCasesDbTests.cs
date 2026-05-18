using Beetles.Application.Common.Interfaces;
using Beetles.Application.Exceptions;
using Beetles.Application.Tests.Database;
using Beetles.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// Tests for edge cases and gaps in bitemporal coverage:
/// - Concurrency and race conditions
/// - Intraday updates (sub-day precision)
/// - Transaction rollback and error handling
/// - BusinessEnd boundary semantics in DeleteAsync
/// </summary>
[Collection("Database")]
public sealed class WallBitemporalEdgeCasesDbTests : WallDbTestBase
{
    public WallBitemporalEdgeCasesDbTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    // =====================================================================
    // CONCURRENCY — Race conditions in UpdateAsync and DeleteAsync
    // =====================================================================

    [Fact]
    public async Task CONC01_ConcurrentUpdates_SameWall_SameBusinessDate_OneSucceeds_OneFailsOrBothSucceed()
    {
        // Two transactions attempt to update the same wall at the same business date simultaneously.
        // Expected: Either one succeeds and one fails (exclusion constraint violation), 
        // or both succeed if they happen at different system times.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));

        // Create two separate scopes to simulate concurrent transactions
        using var scope1 = Fixture.CreateScope();
        using var scope2 = Fixture.CreateScope();

        var repo1 = scope1.ServiceProvider.GetRequiredService<IBitemporalRepository>();
        var repo2 = scope2.ServiceProvider.GetRequiredService<IBitemporalRepository>();

        var update1 = new Wall { Id = id, Color = "blue", BusinessStart = Utc(2025, 5, 3) };
        var update2 = new Wall { Id = id, Color = "green", BusinessStart = Utc(2025, 5, 3) };

        // Both transactions prepare their updates
        await repo1.UpdateAsync(update1, CancellationToken.None);
        await repo2.UpdateAsync(update2, CancellationToken.None);

        // Attempt to commit both
        var commit1 = repo1.CommitChangesAsync(CancellationToken.None);
        var commit2 = repo2.CommitChangesAsync(CancellationToken.None);

        // At least one should succeed, one might fail with DbUpdateException
        var results = await Task.WhenAll(
            commit1.ContinueWith(t => new { Success = t.IsCompletedSuccessfully, Exception = t.Exception }),
            commit2.ContinueWith(t => new { Success = t.IsCompletedSuccessfully, Exception = t.Exception })
        );

        // At least one must succeed
        Assert.True(results.Any(r => r.Success), "At least one concurrent update should succeed");

        // If one failed, it should be due to exclusion constraint
        var failed = results.Where(r => !r.Success).ToList();
        foreach (var f in failed)
        {
            Assert.NotNull(f.Exception);
            Assert.True(
                f.Exception.InnerException is DbUpdateException,
                "Failed concurrent update should throw DbUpdateException due to exclusion constraint");
        }

        // Verify final state has valid bitemporal structure
        var allRows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.True(allRows.Count >= 2, "Should have at least original + one successful update");

        // Verify no overlapping active business intervals
        var actives = allRows.Where(w => w.SystemEnd == Infinity).OrderBy(w => w.BusinessStart).ToList();
        for (var i = 1; i < actives.Count; i++)
        {
            var prevEnd = actives[i - 1].BusinessEnd ?? Infinity;
            Assert.True(prevEnd <= actives[i].BusinessStart, "Active intervals must not overlap");
        }
    }

    [Fact]
    public async Task CONC02_ConcurrentDelete_SameWall_SameBusinessDate_BothSucceedOrOneFails()
    {
        // Two transactions attempt to delete the same wall at the same business date.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));

        using var scope1 = Fixture.CreateScope();
        using var scope2 = Fixture.CreateScope();

        var repo1 = scope1.ServiceProvider.GetRequiredService<IBitemporalRepository>();
        var repo2 = scope2.ServiceProvider.GetRequiredService<IBitemporalRepository>();

        // Both attempt to delete at same business date
        await repo1.DeleteAsync<Wall>(id, Utc(2025, 5, 3), CancellationToken.None);
        await repo2.DeleteAsync<Wall>(id, Utc(2025, 5, 3), CancellationToken.None);

        var commit1 = repo1.CommitChangesAsync(CancellationToken.None);
        var commit2 = repo2.CommitChangesAsync(CancellationToken.None);

        var results = await Task.WhenAll(
            commit1.ContinueWith(t => new { Success = t.IsCompletedSuccessfully, Exception = t.Exception }),
            commit2.ContinueWith(t => new { Success = t.IsCompletedSuccessfully, Exception = t.Exception })
        );

        // At least one should succeed
        Assert.True(results.Any(r => r.Success), "At least one concurrent delete should succeed");

        // Verify final state
        var allRows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.True(allRows.Any(), "Rows should still exist (delete is revert, not hard delete)");
    }

    [Fact]
    public async Task CONC03_ConcurrentUpdateAndDelete_SameWall_MayProduceEitherFinalState()
    {
        // One transaction updates, another deletes - both at same business date.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));

        using var scope1 = Fixture.CreateScope();
        using var scope2 = Fixture.CreateScope();

        var repo1 = scope1.ServiceProvider.GetRequiredService<IBitemporalRepository>();
        var repo2 = scope2.ServiceProvider.GetRequiredService<IBitemporalRepository>();

        // Transaction 1: update to green
        var update = new Wall { Id = id, Color = "green", BusinessStart = Utc(2025, 5, 3) };
        await repo1.UpdateAsync(update, CancellationToken.None);

        // Transaction 2: delete at same date
        await repo2.DeleteAsync<Wall>(id, Utc(2025, 5, 3), CancellationToken.None);

        var commit1 = repo1.CommitChangesAsync(CancellationToken.None);
        var commit2 = repo2.CommitChangesAsync(CancellationToken.None);

        var results = await Task.WhenAll(
            commit1.ContinueWith(t => new { Success = t.IsCompletedSuccessfully }),
            commit2.ContinueWith(t => new { Success = t.IsCompletedSuccessfully })
        );

        // At least one should succeed
        Assert.True(results.Any(r => r.Success), "At least one operation should succeed");

        // Verify database consistency
        var allRows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.NotEmpty(allRows);

        // Verify exactly one current row
        var current = allRows.Where(w => w.SystemEnd == Infinity && w.BusinessEnd == Infinity).ToList();
        Assert.Single(current);
    }

    // =====================================================================
    // INTRADAY — Sub-day precision edge cases
    // =====================================================================

    [Fact]
    public async Task INTRA01_MultipleSameDayUpdates_AllPreserved()
    {
        // Multiple updates on the same calendar day but different times.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start8am = Utc(2025, 5, 1).AddHours(8);
        var start10am = Utc(2025, 5, 1).AddHours(10);
        var start2pm = Utc(2025, 5, 1).AddHours(14);
        var start6pm = Utc(2025, 5, 1).AddHours(18);

        var id = await CreateWall(start8am, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, start10am, "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, start2pm, "green");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        await UpdateWall(id, start6pm, "yellow");

        // Verify all colors are preserved in active knowledge
        var actives = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();

        Assert.Equal(4, actives.Count);
        Assert.Equal("red", actives[0].Color);
        Assert.Equal("blue", actives[1].Color);
        Assert.Equal("green", actives[2].Color);
        Assert.Equal("yellow", actives[3].Color);

        // Verify business intervals are contiguous
        Assert.Equal(start8am, actives[0].BusinessStart);
        Assert.Equal(start10am, actives[0].BusinessEnd);
        Assert.Equal(start10am, actives[1].BusinessStart);
        Assert.Equal(start2pm, actives[1].BusinessEnd);
        Assert.Equal(start2pm, actives[2].BusinessStart);
        Assert.Equal(start6pm, actives[2].BusinessEnd);
        Assert.Equal(start6pm, actives[3].BusinessStart);
        Assert.Equal(Infinity, actives[3].BusinessEnd);
    }

    [Fact]
    public async Task INTRA02_DeleteSameDayUpdate_ShouldSucceed_ButThrowsNotFoundException()
    {
        // BUG: DeleteAsync uses date.AddDays(-1) which fails for same-day updates.
        // EXPECTED: Delete should revert the 2pm update back to red (state at 10am)
        // ACTUAL: Throws NotFoundException because it looks for state on previous day
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start10am = Utc(2025, 5, 1).AddHours(10);
        var start2pm = Utc(2025, 5, 1).AddHours(14);

        var id = await CreateWall(start10am, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, start2pm, "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));

        // EXPECTED: This should work - delete should revert blue back to red
        await DeleteWall(id, start2pm);
        
        // EXPECTED: Current state should be red (reverted from blue)
        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("red", current.Color);
        
        // EXPECTED: Should have proper history
        var actives = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();
        
        Assert.Equal(1, actives.Count);  // Should have one active segment: red [10am, ∞)
        Assert.Equal("red", actives[0].Color);
        Assert.Equal(start10am, actives[0].BusinessStart);
    }

    [Fact]
    public async Task INTRA03_QueryAtSpecificTime_WithinSameDay_ReturnsCorrectColor()
    {
        // Verify GetAsync works correctly with sub-day precision.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start8am = Utc(2025, 5, 1).AddHours(8);
        var start12pm = Utc(2025, 5, 1).AddHours(12);
        
        var id = await CreateWall(start8am, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, start12pm, "blue");

        // Query at 10:00 AM → should be red
        var query10am = Utc(2025, 5, 1).AddHours(10);
        var at10am = await Repo.GetAsync<Wall>(id, query10am, CancellationToken.None);
        Assert.Equal("red", at10am.Color);

        // Query at 12:00 PM (exact boundary) → should be blue (boundary is inclusive on start)
        var at12pm = await Repo.GetAsync<Wall>(id, start12pm, CancellationToken.None);
        Assert.Equal("blue", at12pm.Color);

        // Query at 11:59:59 → should be red
        var justBefore12 = start12pm.AddSeconds(-1);
        var justBeforeResult = await Repo.GetAsync<Wall>(id, justBefore12, CancellationToken.None);
        Assert.Equal("red", justBeforeResult.Color);
    }

    [Fact]
    public async Task INTRA04_DeleteOneMinuteAfterCreate_IsRejected_AndLeavesDbUnchanged()
    {
        // Option A: 12:01 is inside the open interval, but the recorded event is 12:00.
        // DELETE at a non-event timestamp is rejected and must not mutate history.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start12pm = Utc(2025, 5, 1).AddHours(12);
        var id = await CreateWall(start12pm, "red");
        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));

        var deleteTime = start12pm.AddMinutes(1);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.DeleteAsync<Wall>(id, deleteTime, CancellationToken.None));

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task INTRA05_DeleteAdjacentMicrosecondEvent_RevertsWithoutOverlap()
    {
        // PostgreSQL timestamptz stores microseconds, so this is the nearest representable
        // pair of distinct events around midnight.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var firstEvent = DateTimeOffset.Parse("2025-05-09T23:59:59.999999Z");
        var secondEvent = DateTimeOffset.Parse("2025-05-10T00:00:00Z");
        var id = await CreateWall(firstEvent, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, secondEvent, "blue");

        var beforeDelete = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();

        Assert.Equal(2, beforeDelete.Count);
        Assert.Equal("red", beforeDelete[0].Color);
        Assert.Equal(firstEvent, beforeDelete[0].BusinessStart);
        Assert.Equal(secondEvent, beforeDelete[0].BusinessEnd);
        Assert.Equal("blue", beforeDelete[1].Color);
        Assert.Equal(secondEvent, beforeDelete[1].BusinessStart);
        Assert.Equal(Infinity, beforeDelete[1].BusinessEnd);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, secondEvent);

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("red", current.Color);
        Assert.Equal(firstEvent, current.BusinessStart);

        var active = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();

        Assert.Single(active);
        Assert.Equal(firstEvent, active[0].BusinessStart);
        Assert.Equal(Infinity, active[0].BusinessEnd);
    }

    // =====================================================================
    // TRANSACTION ROLLBACK — Error handling and consistency
    // =====================================================================

    [Fact]
    public async Task TRANS02_CommitChangesAsync_DbUpdateException_LeavesNoPartialChanges()
    {
        // If CommitChangesAsync fails (e.g., exclusion constraint), no partial changes persist.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var wall = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1) };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var id = wall.Id;

        // Manually insert an overlapping row to trigger exclusion constraint
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var duplicate = new Wall { Id = id, Color = "blue", BusinessStart = Utc(2025, 5, 1) };
        await Repo.InsertAsync(duplicate, CancellationToken.None);

        // This should fail due to exclusion constraint
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            Repo.CommitChangesAsync(CancellationToken.None));

        // Verify only the original row exists
        var rows = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.Single(rows);
        Assert.Equal("red", rows[0].Color);
    }

    [Fact]
    public async Task TRANS03_MultipleOperations_OneFailsValidation_AllRollback()
    {
        // If a transaction contains multiple operations and one fails, all should roll back.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id1 = await CreateWall(Utc(2025, 5, 1), "red");
        var id2 = await CreateWall(Utc(2025, 5, 1), "blue");

        var countBefore = Repo.QueryAll<Wall>().Count();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));

        using var scope = Fixture.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();

        // Operation 1: Valid update
        await repo.UpdateAsync(new Wall { Id = id1, Color = "green", BusinessStart = Utc(2025, 5, 3) }, CancellationToken.None);

        // Operation 2: Invalid insert (overlapping period for id1)
        var invalid = new Wall { Id = id1, Color = "yellow", BusinessStart = Utc(2025, 5, 1) };
        await repo.InsertAsync(invalid, CancellationToken.None);

        // Commit should fail
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repo.CommitChangesAsync(CancellationToken.None));

        // Verify original state preserved - neither operation persisted
        var countAfter = Repo.QueryAll<Wall>().Count();
        Assert.Equal(countBefore, countAfter);

        // Verify id1 is still red (update rolled back)
        var wall1 = await Repo.GetAsync<Wall>(id1, CancellationToken.None);
        Assert.Equal("red", wall1.Color);
    }

    // =====================================================================
    // BOUNDARY — BusinessEnd boundary semantics in DeleteAsync
    // =====================================================================

    [Fact]
    public async Task BOUND01_Delete_AtExactBusinessEnd_DoesNotAffectPriorSegment()
    {
        // DeleteAsync uses BusinessEnd >= date (inclusive), which might match rows
        // where BusinessEnd == date. This tests if that's intentional or a bug.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        // Create finite segment: red [2025-05-01, 2025-05-10)
        var wall = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            BusinessEnd = Utc(2025, 5, 10)
        };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);
        var id = wall.Id;

        // Create another segment: blue [2025-05-10, ∞)
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var wall2 = new Wall
        {
            Id = id,
            Color = "blue",
            BusinessStart = Utc(2025, 5, 10)
        };
        await Repo.InsertAsync(wall2, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        // Delete at exact boundary: 2025-05-10
        // DeleteAsync will supersede rows where BusinessEnd >= 2025-05-10
        // This matches the red segment (BusinessEnd = 2025-05-10)
        // AND the blue segment (BusinessStart <= 2025-05-10 AND BusinessEnd >= 2025-05-10)
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));

        // According to GetAsync, 2025-05-10 returns blue (BusinessEnd is exclusive in Get)
        // So DeleteAsync at 2025-05-10 should revert blue back to red (state at 2025-05-09)
        await DeleteWall(id, Utc(2025, 5, 10));

        // Current state should be red (reverted from blue)
        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("red", current.Color);
    }

    [Fact]
    public async Task BOUND02_Delete_OneTickBeforeBusinessEnd_IsRejected_AndLeavesDbUnchanged()
    {
        // Option A: one tick before BusinessEnd is inside the red segment, not the exact
        // business start of a recorded event. DELETE must reject it without mutation.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 10), "blue");

        // The active segments are:
        // red [2025-05-01, 2025-05-10)
        // blue [2025-05-10, ∞)

        // Delete at 2025-05-09 23:59:59.999... (one tick before boundary)
        var oneTickBefore = Utc(2025, 5, 10).AddTicks(-1);
        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.DeleteAsync<Wall>(id, oneTickBefore, CancellationToken.None));

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task BOUND03_Update_AtExactBusinessEnd_ShouldCreateContiguousSegment_ButFails()
    {
        // BUG: Cannot update at exact BusinessEnd boundary
        // EXPECTED: Should create contiguous segment blue [May 10, ∞) after red [May 1, May 10)
        // ACTUAL: Throws NotFoundException because GetAsync treats BusinessEnd as exclusive
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var wall = new Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            BusinessEnd = Utc(2025, 5, 10)
        };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);
        var id = wall.Id;

        // EXPECTED: Update at exactly May 10 should create contiguous segment
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 10), "blue");

        // EXPECTED: Should have two contiguous segments
        var actives = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity)
            .OrderBy(w => w.BusinessStart)
            .ToList();

        Assert.Equal(2, actives.Count);
        Assert.Equal("red", actives[0].Color);
        Assert.Equal(Utc(2025, 5, 1), actives[0].BusinessStart);
        Assert.Equal(Utc(2025, 5, 10), actives[0].BusinessEnd);
        
        Assert.Equal("blue", actives[1].Color);
        Assert.Equal(Utc(2025, 5, 10), actives[1].BusinessStart);
        Assert.Equal(Infinity, actives[1].BusinessEnd);
    }
}
