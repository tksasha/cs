using System.Net;
using System.Net.Http.Json;

using Beetles.Application.Responses;
using Beetles.Application.Tests.Database;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// End-to-end API coverage that verifies HTTP responses and persisted history together.
/// These tests intentionally go through the API and then inspect the database.
/// </summary>
[Collection("Database")]
public sealed class WallApiDatabaseScenarioTests : WallApiTestBase
{
    public WallApiDatabaseScenarioTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task E2E01_CreateThenFuturePatch_PersistsExpectedBitemporalRows()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var create = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(created);
        Assert.Equal("red", created!.Color);
        Assert.Equal(Utc(2025, 5, 10), created.SystemStart);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Infinity, "red")
        );

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));

        var patch = await PatchWall(created.Id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 5, 3), Infinity, Utc(2025, 5, 11), Infinity, "blue")
        );
    }

    [Fact]
    public async Task E2E02_ResponsePayloads_MatchPersistedRows_AfterPatch()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var patch = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var payload = await patch.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);

        var persisted = (await SnapshotWallHistoryAsync(Fixture, id))
            .Single(r => r.Color == "blue" && r.SystemEnd == Infinity);

        Assert.Equal(id, payload!.Id);
        Assert.Equal(persisted.Color, payload.Color);
        Assert.Equal(persisted.BusinessStart, payload.BusinessStart);
        Assert.Equal(persisted.BusinessEnd, payload.BusinessEnd ?? Infinity);
        Assert.Equal(persisted.SystemStart, payload.SystemStart);
        Assert.Equal(persisted.SystemEnd, payload.SystemEnd);
    }

    [Fact]
    public async Task E2E03_RetroactivePatch_DoesNotChangeCurrentColorButChangesPointInTimeHistory()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 10), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 20), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var patch = await PatchWall(id, new { Color = "green", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var rows = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Contains(rows, r =>
            r.Color == "green"
            && r.BusinessStart == Utc(2025, 5, 1)
            && r.BusinessEnd == Utc(2025, 5, 10)
            && r.SystemEnd == Infinity);

        Assert.Contains(rows, r =>
            r.Color == "blue"
            && r.BusinessStart == Utc(2025, 5, 20)
            && r.BusinessEnd == Infinity
            && r.SystemEnd == Infinity);
    }

    [Fact]
    public async Task E2E04_DuplicateDeleteThroughApi_DoesNotMutateHistory()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var first = await DeleteWall(id, Utc(2025, 5, 3));
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var afterFirst = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        var second = await DeleteWall(id, Utc(2025, 5, 3));
        Assert.True(
            second.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound or HttpStatusCode.Conflict,
            $"Expected duplicate DELETE to be idempotent or explicitly rejected, got {(int)second.StatusCode}");

        var afterSecond = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Equal(afterFirst, afterSecond);
    }

    [Fact]
    public async Task E2E05_FailedPatch_DoesNotCreatePartialHistoryRows()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");
        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var failed = await PatchWall(999999, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.NotFound, failed.StatusCode);

        var after = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Equal(before, after);
    }

    [Fact]
    public async Task E2E06_IntradayDelete_RevertsPersistedCurrentState()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start10am = Utc(2025, 5, 1).AddHours(10);
        var start2pm = Utc(2025, 5, 1).AddHours(14);
        var id = await CreateWall(start10am, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, start2pm, "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var delete = await DeleteWall(id, start2pm);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var rows = await SnapshotWallHistoryAsync(Fixture, id);
        var current = rows.Single(r => r.BusinessEnd == Infinity && r.SystemEnd == Infinity);

        Assert.Equal("red", current.Color);
        Assert.Equal(start10am, current.BusinessStart);
        Assert.Equal(Infinity, current.BusinessEnd);
    }

    [Fact]
    public async Task E2E07_DeleteNearBoundary_DoesNotMutateHistoryOrCreateOverlap()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 10), "blue");

        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var delete = await DeleteWall(id, Utc(2025, 5, 10).AddTicks(-1));
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);

        var active = after
            .Where(r => r.SystemEnd == Infinity)
            .OrderBy(r => r.BusinessStart)
            .ToList();

        for (var i = 1; i < active.Count; i++)
            Assert.True(active[i - 1].BusinessEnd <= active[i].BusinessStart);
    }

    [Fact]
    public async Task E2E08_UpdateInsideGapAfterDeletedFirstEvent_FillsOnlyGap()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var delete = await DeleteWall(id, Utc(2025, 5, 1));
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        var patch = await PatchWall(id, new { Color = "green", DateTime = Utc(2025, 5, 2) });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var rows = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Contains(rows, r =>
            r.Color == "green"
            && r.BusinessStart == Utc(2025, 5, 2)
            && r.BusinessEnd == Utc(2025, 5, 3)
            && r.SystemEnd == Infinity);

        Assert.Contains(rows, r =>
            r.Color == "blue"
            && r.BusinessStart == Utc(2025, 5, 3)
            && r.BusinessEnd == Infinity
            && r.SystemEnd == Infinity);

        var active = rows
            .Where(r => r.SystemEnd == Infinity)
            .OrderBy(r => r.BusinessStart)
            .ToList();

        for (var i = 1; i < active.Count; i++)
            Assert.True(active[i - 1].BusinessEnd <= active[i].BusinessStart);
    }

    [Fact]
    public async Task E2E08b_UpdateInsideGap_ResponsePayload_MatchesBoundedPersistedRow()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await DeleteWall(id, Utc(2025, 5, 1));

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        var patch = await PatchWall(id, new { Color = "green", DateTime = Utc(2025, 5, 2) });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var payload = await patch.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);

        var persisted = (await SnapshotWallHistoryAsync(Fixture, id))
            .Single(r => r.Color == "green" && r.SystemEnd == Infinity);

        Assert.Equal(persisted.BusinessEnd, payload!.BusinessEnd ?? Infinity);
        Assert.Equal(persisted.SystemStart, payload.SystemStart);
        Assert.Equal(persisted.SystemEnd, payload.SystemEnd);
    }

    [Fact]
    public async Task E2E09_DeleteAdjacentMicrosecondEvent_RevertsWithoutOverlap()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var firstEvent = DateTimeOffset.Parse("2025-05-09T23:59:59.999999Z");
        var secondEvent = DateTimeOffset.Parse("2025-05-10T00:00:00Z");
        var id = await CreateWall(firstEvent, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, secondEvent, "blue");

        var beforeDelete = await SnapshotWallHistoryAsync(Fixture, id);
        var activeBeforeDelete = beforeDelete
            .Where(r => r.SystemEnd == Infinity)
            .OrderBy(r => r.BusinessStart)
            .ToList();

        Assert.Equal(2, activeBeforeDelete.Count);
        Assert.Equal("red", activeBeforeDelete[0].Color);
        Assert.Equal(firstEvent, activeBeforeDelete[0].BusinessStart);
        Assert.Equal(secondEvent, activeBeforeDelete[0].BusinessEnd);
        Assert.Equal("blue", activeBeforeDelete[1].Color);
        Assert.Equal(secondEvent, activeBeforeDelete[1].BusinessStart);
        Assert.Equal(Infinity, activeBeforeDelete[1].BusinessEnd);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var delete = await DeleteWall(id, secondEvent);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var rows = await SnapshotWallHistoryAsync(Fixture, id);
        var current = rows.Single(r => r.BusinessEnd == Infinity && r.SystemEnd == Infinity);

        Assert.Equal("red", current.Color);
        Assert.Equal(firstEvent, current.BusinessStart);

        var active = rows
            .Where(r => r.SystemEnd == Infinity)
            .OrderBy(r => r.BusinessStart)
            .ToList();

        Assert.Single(active);
        Assert.Equal(firstEvent, active[0].BusinessStart);
        Assert.Equal(Infinity, active[0].BusinessEnd);
    }

    [Fact]
    public async Task E2E10_DeleteMiddleEvent_WithLaterEvents_RevertsToCorrectBoundedSegment()
    {
        // BUG: DeleteAsync restores the prior segment open-ended (BusinessEnd = ∞),
        // which overlaps with the next active segment → exclusion constraint → 409.
        // EXPECTED: restored segment BusinessEnd should be bounded to the next event's
        // BusinessStart so black [May3, May8) + green [May8, ∞) — no overlap.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 3), "black");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 7), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 5, 8), "green");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        var delete = await DeleteWall(id, Utc(2025, 5, 7));

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var active = (await SnapshotWallHistoryAsync(Fixture, id))
            .Where(r => r.SystemEnd == Infinity)
            .OrderBy(r => r.BusinessStart)
            .ToList();

        Assert.Equal(2, active.Count);
        Assert.Equal("black", active[0].Color);
        Assert.Equal(Utc(2025, 5, 3), active[0].BusinessStart);
        Assert.Equal(Utc(2025, 5, 8), active[0].BusinessEnd);
        Assert.Equal("green", active[1].Color);
        Assert.Equal(Utc(2025, 5, 8), active[1].BusinessStart);
        Assert.Equal(Infinity, active[1].BusinessEnd);
    }

    [Fact]
    public async Task E2E11_ReapplySupersededColor_AtSameBusinessDate_Returns409_AndLeavesHistoryUnchanged()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 5, 3), "green");

        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        var patch = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });

        Assert.Equal(HttpStatusCode.Conflict, patch.StatusCode);

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }
}
