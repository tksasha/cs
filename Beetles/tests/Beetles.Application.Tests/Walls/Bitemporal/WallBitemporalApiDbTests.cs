using System.Net;
using System.Net.Http.Json;

using Beetles.Application.Tests.Database;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

[Collection("Database")]
public sealed class WallBitemporalApiDbTests : WallApiTestBase
{
    public WallBitemporalApiDbTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Tc01_FirstKnownPaint_Red_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Infinity, "red")
        );

        Assert.True(id > 0);
    }

    [Fact]
    public async Task Tc02_RetroactiveRepaint_BlueOn03May_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 5, 3), Infinity, Utc(2025, 5, 11), Infinity, "blue")
        );
    }

    [Fact]
    public async Task Tc03_RepaintToBlack_On13Jun_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 15));
        await UpdateWall(id, Utc(2025, 6, 13), "black");

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3), Utc(2025, 5, 11), Infinity, "red"),
            Row(Utc(2025, 5, 3), Infinity, Utc(2025, 5, 11), Utc(2025, 6, 15), "blue"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 13), Utc(2025, 6, 15), Infinity, "blue"),
            Row(Utc(2025, 6, 13), Infinity, Utc(2025, 6, 15), Infinity, "black")
        );
    }

    [Fact]
    public async Task Tc04_FutureRepaint_WhiteOn01Jun_ViaApi()
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
    public async Task Tc05_Correction_BlueToLightBlue_From03May_ViaApi()
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
    public async Task Tc06_Correction_NoRepaintOn13Jun_ViaApi()
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

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 25));
        var deleteResponse = await DeleteWall(id, Utc(2025, 6, 13));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 5, 1), Infinity,         Utc(2025, 5, 10), Utc(2025, 5, 11), "red"),
            Row(Utc(2025, 5, 1), Utc(2025, 5, 3),  Utc(2025, 5, 11), Infinity,         "red"),
            Row(Utc(2025, 5, 3), Infinity,         Utc(2025, 5, 11), Utc(2025, 6, 15), "blue"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 13), Utc(2025, 6, 15), Utc(2025, 6, 18), "blue"),
            Row(Utc(2025, 6, 13), Infinity,        Utc(2025, 6, 15), Utc(2025, 6, 25), "black"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 1),  Utc(2025, 6, 18), Utc(2025, 6, 20), "blue"),
            Row(Utc(2025, 6, 1), Utc(2025, 6, 13), Utc(2025, 6, 18), Utc(2025, 6, 25), "white"),
            Row(Utc(2025, 5, 3), Utc(2025, 6, 1),  Utc(2025, 6, 20), Infinity,         "light-blue"),
            Row(Utc(2025, 6, 1), Infinity,         Utc(2025, 6, 25), Infinity,         "white")
        );
    }

    [Fact]
    public async Task Tc06_SameBusinessDayRecolor_ViaApi()
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
    public async Task BT12_PatchBeforeFirstKnownBusinessStart_BackfillsHistory_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var postResponse = await PostWall(new { Color = "redis", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<Beetles.Application.Responses.WallResponse>();
        Assert.NotNull(created);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var patchResponse = await PatchWall(created!.Id, new { Color = "redis", DateTime = Utc(2025, 4, 1) });
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 4, 1), Utc(2025, 5, 1), Utc(2025, 5, 11), Infinity, "redis"),
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Infinity, "redis")
        );
    }

    [Fact]
    public async Task BT12b_PatchBeforeFirstKnownBusinessStart_DifferentPayload_IsQueryableOnBackfilledPeriod_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var patchResponse = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 4, 1) });
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        await AssertWallsAsync(Fixture,
            Row(Utc(2025, 4, 1), Utc(2025, 5, 1), Utc(2025, 5, 11), Infinity, "blue"),
            Row(Utc(2025, 5, 1), Infinity, Utc(2025, 5, 10), Infinity, "red")
        );
    }

    [Fact]
    public async Task BT13_DuplicateUpdate_SamePayload_DoesNotChangeState_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var firstResponse = await PatchWall(id,
            new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var afterFirstUpdate = await SnapshotWallHistoryAsync(Fixture, id);

        var secondResponse = await PatchWall(id,
            new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.True(secondResponse.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict);

        var afterSecondUpdate = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Equal(afterFirstUpdate, afterSecondUpdate);
    }

    [Fact]
    public async Task BT14_DuplicateDelete_SamePayload_DoesNotChangeState_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 15));
        await UpdateWall(id, Utc(2025, 6, 13), "black");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 25));
        var firstResponse = await DeleteWall(id, Utc(2025, 6, 13));
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var afterFirstDelete = await SnapshotWallHistoryAsync(Fixture, id);

        var secondResponse = await DeleteWall(id, Utc(2025, 6, 13));
        Assert.True(secondResponse.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound or HttpStatusCode.Conflict);

        var afterSecondDelete = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Equal(afterFirstDelete, afterSecondDelete);
    }

    [Fact]
    public async Task BT17_DuplicatePatch_SamePayload_LaterSystemTime_NoSecondEffect()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var firstResponse = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var afterFirstUpdate = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var secondResponse = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.True(secondResponse.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict);

        var afterSecondUpdate = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Equal(afterFirstUpdate, afterSecondUpdate);
    }

    [Fact]
    public async Task BT18_DuplicateDelete_SamePayload_LaterSystemTime_NoSecondEffect()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 25));
        var firstResponse = await DeleteWall(id, Utc(2025, 5, 3));
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var afterFirstDelete = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 26));
        var secondResponse = await DeleteWall(id, Utc(2025, 5, 3));
        Assert.True(secondResponse.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound or HttpStatusCode.Conflict);

        var afterSecondDelete = await SnapshotWallHistoryAsync(Fixture, id);

        Assert.Equal(afterFirstDelete, afterSecondDelete);
    }

    [Fact]
    public async Task BT19_TwoWalls_InterleavedUpdates_AreIsolated()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id1 = await CreateWall(Utc(2025, 5, 1), "red");
        var id2 = await CreateWall(Utc(2025, 5, 5), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id1, Utc(2025, 5, 3), "green");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id2, Utc(2025, 5, 6), "yellow");

        var wall1 = await SnapshotWallHistoryAsync(Fixture, id1);
        var wall2 = await SnapshotWallHistoryAsync(Fixture, id2);

        Assert.All(wall1, w => Assert.True(w.Color is "red" or "green"));
        Assert.All(wall2, w => Assert.True(w.Color is "blue" or "yellow"));
    }

    [Fact]
    public async Task BT20_DeleteOneWall_DoesNotAffectOtherCurrentState()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id1 = await CreateWall(Utc(2025, 5, 1), "red");
        var id2 = await CreateWall(Utc(2025, 5, 2), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id1, Utc(2025, 5, 3), "black");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var deleted = await DeleteWall(id1, Utc(2025, 5, 3));
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var secondWallPatch = await PatchWall(id2, new { Color = "white", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, secondWallPatch.StatusCode);

        var wall2 = await SnapshotWallHistoryAsync(Fixture, id2);
        Assert.All(wall2, row => Assert.True(row.Color is "blue" or "white"));
    }

    [Fact]
    public async Task Tc12_PostSamePayloadTwice_ReturnsConflict_ViaApi()
    {
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id1 = await CreateWall(Utc(2025, 5, 1), "red");

        var secondResponse = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 1) });
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var walls = await SnapshotWallHistoryAsync(Fixture, id1);
        Assert.Single(walls);
        Assert.Equal("red", walls[0].Color);
    }

    [Fact]
    public async Task Tc13_PostAfterDeletion_SameColor_Succeeds_ViaApi()
    {
        // BUG: WallService.CreateAsync checks QueryAll (all rows, including superseded) for
        // color uniqueness. A deleted wall's color remains permanently reserved, blocking a
        // new POST with the same color.
        // EXPECTED: color uniqueness should apply only to active walls, not superseded history.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var deleteResponse = await DeleteWall(id, Utc(2025, 5, 1));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var secondCreate = await PostWall(new { Color = "red", DateTime = Utc(2025, 5, 12) });
        Assert.Equal(HttpStatusCode.Created, secondCreate.StatusCode);
    }

    [Fact]
    public async Task BT22_Patch_BackToPreviouslySupersededColor_AtSameBusinessDate_Succeeds_ViaApi()
    {
        // BUG: WallService.UpdateAsync checks QueryAll (all rows, including superseded) for the
        // same id+color+businessDate combination. A superseded row blocks re-applying the same
        // value at the same date, even though it is no longer the active state.
        // EXPECTED: uniqueness check should apply only to the current active row at that date.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 3), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        await UpdateWall(id, Utc(2025, 5, 3), "light-blue");  // correction supersedes blue

        // Re-apply blue at May 3 — should succeed since blue is no longer the active value.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        var revert = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
        Assert.Equal(HttpStatusCode.OK, revert.StatusCode);
    }

    [Fact]
    public async Task BT21_Patch_AfterFullDeletion_ReAddsWallUnderSameId_ViaApi()
    {
        // WallService.UpdateAsync calls repo.UpdateAsync directly (no GetAsync guard),
        // so patching an ID whose only version was deleted must succeed: EnsureExistsAsync
        // finds the superseded rows, FindAsync returns null (no active coverage), and
        // AppendAsync creates a fresh open-ended segment.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var deleteResponse = await DeleteWall(id, Utc(2025, 5, 1));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var patchResponse = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 5) });
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        var active = (await SnapshotWallHistoryAsync(Fixture, id))
            .Where(r => r.SystemEnd == Infinity)
            .ToList();

        Assert.Single(active);
        Assert.Equal("blue", active[0].Color);
        Assert.Equal(Utc(2025, 5, 5), active[0].BusinessStart);
        Assert.Equal(Infinity, active[0].BusinessEnd);
    }

}
