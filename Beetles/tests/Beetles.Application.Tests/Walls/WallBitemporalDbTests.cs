using Beetles.Application.Common.Interfaces;
using Beetles.Application.Exceptions;
using Beetles.Application.Tests.Database;
using Beetles.Domain.Entities;
using Beetles.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

[Collection("Database")]
public sealed class WallBitemporalDbTests : WallDbTestBase
{
    public WallBitemporalDbTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Tc01_FirstKnownPaint_Red()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Infinity, "red")
        );

        Assert.True(id > 0);
    }

    [Fact]
    public async Task Tc02_RetroactiveRepaint_BlueOn03May()
    {
        await CreateDefaultWallThenBlue();

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 5, 3), Infinity, Utc(2025, 5, 11), Infinity, "blue")
        );
    }

    [Fact]
    public async Task Tc03_RepaintToBlack_On13Jun()
    {
        await CreateDefaultWallThenBlueThenBlack();

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 5, 3), Infinity, Utc(2025, 5, 11), Utc(2025, 6, 15), "blue"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 13), Utc(2025, 6, 15), Infinity, "blue"),
            Row(Utc(2025, 6, 13), Infinity, Utc(2025, 6, 15), Infinity, "black")
        );
    }

    [Fact]
    public async Task Tc04_FutureRepaint_WhiteOn01Jun()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 15));
        await UpdateWall(id, Utc(2025, 6, 13), "black");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 18));
        await UpdateWall(id, Utc(2025, 6, 1), "white");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 5, 3), Infinity, Utc(2025, 5, 11), Utc(2025, 6, 15), "blue"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 13), Utc(2025, 6, 15), Utc(2025, 6, 18), "blue"),
            Row(Utc(2025, 6, 13), Infinity, Utc(2025, 6, 15), Infinity, "black"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 1), Utc(2025, 6, 18), Infinity, "blue"),
            Row(Utc(2025, 6, 1), Utc(2025, 6, 13), Utc(2025, 6, 18), Infinity, "white")
        );
    }

    [Fact]
    public async Task Tc05_Correction_BlueToLightBlue_From03May()
    {
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
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 5, 3), Infinity, Utc(2025, 5, 11), Utc(2025, 6, 15), "blue"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 13), Utc(2025, 6, 15), Utc(2025, 6, 18), "blue"),
            Row(Utc(2025, 6, 13), Infinity, Utc(2025, 6, 15), Infinity, "black"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 1), Utc(2025, 6, 18), Utc(2025, 6, 20), "blue"),
            Row(Utc(2025, 6, 1), Utc(2025, 6, 13), Utc(2025, 6, 18), Infinity, "white"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 1), Utc(2025, 6, 20), Infinity, "light-blue")
        );
    }

    [Fact]
    public async Task Tc06_SameBusinessDayRecolor()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 1), "green");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 11), Infinity, "green")
        );
    }

    [Fact]
    public async Task Tc07_UpdateNonExistentWall_ThrowsNotFound()
    {
        var wall = new Wall
        {
            Id = 999,
            Color = "ghost",
            BusinessStart = Utc(2025, 5, 1),
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.UpdateAsync(wall, CancellationToken.None));
    }

    [Fact]
    public async Task Tc08_PointInTimeQueryAfterUpdates()
    {
        var id = await CreateDefaultWallThenBlue();

        var on02May = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 2), CancellationToken.None);
        Assert.Equal("red", on02May.Color);

        var on04May = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 4), CancellationToken.None);
        Assert.Equal("blue", on04May.Color);

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("blue", current.Color);
    }

    [Fact]
    public async Task Tc09_Boundary_FromInclusive_ToExclusive()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var wall = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1), BusinessEnd = Utc(2025, 5, 10) };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);
        var id = wall.Id;

        var atStart = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 1), CancellationToken.None);
        Assert.Equal("red", atStart.Color);

        var inside = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 5), CancellationToken.None);
        Assert.Equal("red", inside.Color);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            Repo.GetAsync<Wall>(id, Utc(2025, 5, 10), CancellationToken.None));

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Utc(2025, 5, 10), Utc(2025, 5, 10), Infinity, "red")
        );
    }

    [Fact]
    public async Task Tc10_DeleteCancel_DisappearsFromCurrent()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        int id;
        using (var scope = Fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();
            id = await WallDbHelper.CreateWall(repo, Utc(2025, 5, 1), "red");
        }

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        using (var scope = Fixture.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var active = await context.Walls.FirstAsync(w => w.Id == id && w.SystemEnd == Infinity, CancellationToken.None);
            active.SystemEnd = Utc(2025, 5, 11);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        using (var scope = Fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();
            await Assert.ThrowsAsync<NotFoundException>(() =>
                repo.GetAsync<Wall>(id, CancellationToken.None));
        }

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red")
        );
    }

    [Fact]
    public async Task Tc12_AuditReplay_HistoricalSystemState()
    {
        var id = await CreateDefaultWallThenBlue();

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.Equal(3, all.Count);

        var superseded = all.First(w => w.SystemEnd != Infinity && w.BusinessEnd == Infinity);
        Assert.Equal("red", superseded.Color);
        Assert.Equal(Utc(2025, 5, 1), superseded.BusinessStart);
        Assert.Equal(Utc(2025, 5, 11), superseded.SystemEnd);

        var atTxTimeBefore = all.Where(w => w.Id == id && w.SystemStart <= Utc(2025, 5, 10) && w.SystemEnd > Utc(2025, 5, 10)).ToList();
        Assert.Single(atTxTimeBefore);
        Assert.Equal("red", atTxTimeBefore[0].Color);

        var atTxTimeAfter = all.Where(w => w.Id == id && w.SystemStart <= Utc(2025, 5, 12) && w.SystemEnd > Utc(2025, 5, 12)).ToList();
        Assert.Equal(2, atTxTimeAfter.Count);
    }

    [Fact]
    public async Task B04_SeparateWalls_DontInterfere()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id1 = await CreateWall(Utc(2025, 5, 1), "red");
        var id2 = await CreateWall(Utc(2025, 5, 5), "blue");

        Assert.NotEqual(id1, id2);

        var wall1 = await Repo.GetAsync<Wall>(id1, CancellationToken.None);
        var wall2 = await Repo.GetAsync<Wall>(id2, CancellationToken.None);
        Assert.Equal("red", wall1.Color);
        Assert.Equal("blue", wall2.Color);
    }

    [Fact]
    public async Task B05_Id_StableAcrossUpdates()
    {
        var id = await CreateDefaultWallThenBlue();

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        Assert.NotEmpty(all);
        Assert.All(all, w => Assert.Equal(id, w.Id));
    }

    [Fact]
    public async Task B06_SystemStart_Monotonic()
    {
        var id = await CreateDefaultWallThenBlueThenBlack();

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).OrderBy(w => w.SystemStart).ToList();
        for (var i = 1; i < all.Count; i++)
            Assert.True(all[i - 1].SystemStart <= all[i].SystemStart,
                $"SystemStart not monotonic: {all[i - 1].SystemStart:o} > {all[i].SystemStart:o}");
    }

    [Fact]
    public async Task B07_PreviousSystemEnd_Equals_NextSystemStart()
    {
        var id = await CreateDefaultWallThenBlue();

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        var ended = all.Where(w => w.SystemEnd != Infinity);
        foreach (var endedRow in ended)
            Assert.Contains(all, r => r.SystemStart == endedRow.SystemEnd);
    }

    [Fact]
    public async Task BT05_EarlierBusinessStart_BackfillsHistory()
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

        var current = await Repo.GetAsync<Wall>(id, CancellationToken.None);
        Assert.Equal("red", current.Color);
        Assert.Equal(Utc(2025, 5, 5), current.BusinessStart);

        var backfilled = await Repo.GetAsync<Wall>(id, Utc(2025, 5, 1), CancellationToken.None);
        Assert.Equal("blue", backfilled.Color);
    }

    [Fact]
    public async Task DB02_BusinessStart_LessThan_BusinessEnd()
    {
        var id = await CreateDefaultWallThenBlue();

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        foreach (var w in all)
        {
            var be = w.BusinessEnd ?? Infinity;
            Assert.True(w.BusinessStart < be,
                $"BusinessStart {w.BusinessStart:o} should be < BusinessEnd {be:o} for Id={w.Id}, TxId={w.TransactionId}");
        }
    }

    [Fact]
    public async Task DB03_SystemStart_LessThan_SystemEnd()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        foreach (var w in all)
            Assert.True(w.SystemStart < w.SystemEnd,
                $"SystemStart {w.SystemStart:o} should be < SystemEnd {w.SystemEnd:o} for Id={w.Id}, TxId={w.TransactionId}");
    }

    [Fact]
    public async Task DB04_No_ExclusionConstraintViolations()
    {
        var id = await CreateDefaultWallThenBlueThenBlack();

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == id).ToList();
        for (var i = 0; i < all.Count; i++)
        {
            for (var j = i + 1; j < all.Count; j++)
            {
                var a = all[i];
                var b = all[j];
                bool businessOverlap = a.BusinessStart < (b.BusinessEnd ?? Infinity) && b.BusinessStart < (a.BusinessEnd ?? Infinity);
                bool systemOverlap = a.SystemStart < b.SystemEnd && b.SystemStart < a.SystemEnd;
                Assert.False(businessOverlap && systemOverlap,
                    $"Exclusion violation between TxId {a.TransactionId} and {b.TransactionId}: business and system periods both overlap");
            }
        }
    }

    [Fact]
    public async Task DB09_Patch_IncreasesRowCount()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        long count;
        int id;
        using (var scope = Fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();
            id = await WallDbHelper.CreateWall(repo, Utc(2025, 5, 1), "red");
            count = repo.QueryAll<Wall>().LongCount();
        }

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        using (var scope = Fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();
            await WallDbHelper.UpdateWall(repo, id, Utc(2025, 5, 3), "blue");
            var newCount = repo.QueryAll<Wall>().LongCount();
            Assert.True(newCount > count, $"Row count should increase after PATCH (was {count}, now {newCount})");
            count = newCount;
        }

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 15));
        using (var scope = Fixture.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();
            await WallDbHelper.UpdateWall(repo, id, Utc(2025, 6, 13), "black");
            var newCount = repo.QueryAll<Wall>().LongCount();
            Assert.True(newCount > count, $"Row count should increase after second PATCH (was {count}, now {newCount})");
        }
    }

    [Fact]
    public async Task ST12_SystemEnd_DefaultsToInfinity()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var wall = new Wall { Color = "red", BusinessStart = Utc(2025, 5, 1) };
        await Repo.InsertAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        var all = Repo.QueryAll<Wall>().Where(w => w.Id == wall.Id).ToList();
        foreach (var w in all)
            Assert.Equal(Infinity, w.SystemEnd);
    }

    [Fact]
    public async Task BT13_DuplicateUpdate_SamePayload_PreservesRepositoryTemporalStructure()
    {
        var id = await CreateDefaultWallThenBlue();

        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        await AssertCurrentColor(id, "blue");
        AssertActiveRowsDoNotOverlap(id);
    }

    [Fact]
    public async Task BT14_DuplicateDelete_SamePayload_DoesNotChangeState()
    {
        var id = await CreateDefaultWallThenBlueThenBlack();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 25));
        await DeleteWall(id, Utc(2025, 6, 13));

        var afterFirstDelete = SnapshotWallHistory(id);

        try
        {
            await DeleteWall(id, Utc(2025, 6, 13));
        }
        catch (Exception)
        {
            // Any exception is acceptable here; we only require no second effect.
        }

        var afterSecondDelete = SnapshotWallHistory(id);

        Assert.Equal(afterFirstDelete, afterSecondDelete);
    }

    [Fact]
    public async Task DB10_OneCurrentVersionPerId_WhenNotDeleted()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 5, 5), "green");

        var current = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.SystemEnd == Infinity && w.BusinessEnd == Infinity)
            .ToList();

        Assert.Single(current);
        Assert.Equal("green", current[0].Color);
    }

    [Fact]
    public async Task DB12_ReplayHistoricalEvent_PreservesCurrentState()
    {
        var id = await CreateDefaultWallThenBlue();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        await AssertCurrentColor(id, "blue");
    }

    [Fact]
    public async Task DB13_RetroactiveDelete_Replay_NoAdditionalRows()
    {
        var id = await CreateDefaultWallThenBlue();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, Utc(2025, 5, 3));

        var first = SnapshotWallHistory(id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        try
        {
            await DeleteWall(id, Utc(2025, 5, 3));
        }
        catch (Exception)
        {
        }

        var second = SnapshotWallHistory(id);
        Assert.Equal(first, second);
    }

    private List<WallRow> SnapshotWallHistory(int id)
        => Repo.QueryAll<Wall>()
            .Where(w => w.Id == id)
            .ToList()
            .Select(w => Row(
                w.BusinessStart,
                w.BusinessEnd ?? Infinity,
                w.SystemStart,
                w.SystemEnd,
                w.Color))
            .OrderBy(r => r.BusinessStart)
            .ThenBy(r => r.BusinessEnd)
            .ThenBy(r => r.SystemStart)
            .ThenBy(r => r.SystemEnd)
            .ThenBy(r => r.Color)
            .ToList();

    [Fact]
    public async Task SP09_UpdateWithLaterBusinessStart_AfterEarlierBusinessEnd()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var wall = new Wall { Id = id, Color = "blue", BusinessStart = Utc(2025, 6, 1) };
        await Repo.UpdateAsync(wall, CancellationToken.None);
        await Repo.CommitChangesAsync(CancellationToken.None);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 6, 1), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 6, 1), Infinity, Utc(2025, 5, 11), Infinity, "blue")
        );
    }
}
