using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Beetles.Application.Common.Interfaces;
using Beetles.Application.Tests.Database;

using Microsoft.Extensions.DependencyInjection;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// API-level tests for edge case bugs - these tests should fail to expose implementation issues.
/// These are HTTP API versions of the failing tests in WallBitemporalEdgeCasesDbTests.
/// </summary>
[Collection("Database")]
public sealed class WallBitemporalEdgeCasesApiTests : WallApiTestBase
{
    public WallBitemporalEdgeCasesApiTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    // =====================================================================
    // BUG #1: Intraday Delete Fails - API Version
    // =====================================================================

    [Fact]
    public async Task API_INTRA02_DeleteSameDayUpdate_ShouldSucceed_ButReturns404()
    {
        // BUG: DELETE request for same-day update fails with 404
        // EXPECTED: 204 No Content - delete should revert to earlier state
        // ACTUAL: 404 Not Found - DeleteAsync throws NotFoundException
        
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start10am = Utc(2025, 5, 1).AddHours(10);
        var start2pm = Utc(2025, 5, 1).AddHours(14);

        // Create wall at 10am with red
        var id = await CreateWall(start10am, "red");

        // Update to blue at 2pm (same day)
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, start2pm, "blue");

        // EXPECTED: DELETE at 2pm should succeed (revert blue back to red)
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var deleteResponse = await DeleteWall(id, start2pm);

        // EXPECTED: 204 No Content
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // EXPECTED: current persisted state reverts to red.
        var rows = await SnapshotWallHistoryAsync(Fixture, id);
        var current = rows.Single(r => r.BusinessEnd == Infinity && r.SystemEnd == Infinity);
        Assert.Equal("red", current.Color);
    }

    [Fact]
    public async Task API_INTRA04_DeleteOneMinuteAfterCreate_Returns404_AndLeavesHistoryUnchanged()
    {
        // Option A: 12:01 is not an exact recorded event timestamp.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start12pm = Utc(2025, 5, 1).AddHours(12);
        
        // Create wall at 12:00pm
        var id = await CreateWall(start12pm, "red");
        var before = await SnapshotWallHistoryAsync(Fixture, id);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var deleteTime = start12pm.AddMinutes(1);
        var deleteResponse = await DeleteWall(id, deleteTime);

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }

    // =====================================================================
    // BUG #2: Delete at Boundary Violates Constraint - API Version
    // =====================================================================

    [Fact]
    public async Task API_BOUND02_DeleteOneTickBeforeBusinessEnd_Returns404_AndLeavesHistoryUnchanged()
    {
        // Option A: one tick before the next event is not itself a recorded event.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 10), "blue");

        // State: red [May 1, May 10), blue [May 10, ∞)
        
        // EXPECTED: DELETE at one tick before May 10 should succeed
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var oneTickBefore = Utc(2025, 5, 10).AddTicks(-1);
        var before = await SnapshotWallHistoryAsync(Fixture, id);
        var deleteResponse = await DeleteWall(id, oneTickBefore);

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var after = await SnapshotWallHistoryAsync(Fixture, id);
        Assert.Equal(before, after);
    }

    // =====================================================================
    // BUG #3: Cannot Update at Exact BusinessEnd - API Version
    // =====================================================================

    [Fact]
    public async Task API_BOUND03_UpdateAtExactBusinessEnd_ShouldSucceed_ButReturns404()
    {
        // BUG: PATCH at exact BusinessEnd of finite segment fails with 404
        // EXPECTED: 200 OK - should create contiguous segment
        // ACTUAL: 404 Not Found - GetAsync can't find row at exact BusinessEnd
        
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        // Manually create finite segment via direct database access
        // (API doesn't expose BusinessEnd setting, so we test the scenario that could occur)
        using var scope = Fixture.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();
        
        var wall = new Domain.Entities.Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            BusinessEnd = Utc(2025, 5, 10)
        };
        await repo.InsertAsync(wall, CancellationToken.None);
        await repo.CommitChangesAsync(CancellationToken.None);
        var id = wall.Id;

        // EXPECTED: PATCH at exactly May 10 should create new contiguous segment
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var patchResponse = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 10) });

        // EXPECTED: 200 OK
        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        // EXPECTED: Should have blue segment starting at May 10
        var wall2 = await patchResponse.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(wall2);
        Assert.Equal("blue", wall2.GetProperty("color").GetString());
    }

    // =====================================================================
    // Additional API-level validations for bug scenarios
    // =====================================================================

    [Fact]
    public async Task API_INTRA_MultipleSameDayOperations_ShouldAllSucceed()
    {
        // This test verifies that the API can handle multiple PATCH operations on the same day
        // All operations should succeed (doesn't trigger the delete bug)
        
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start8am = Utc(2025, 5, 1).AddHours(8);
        var start10am = Utc(2025, 5, 1).AddHours(10);
        var start2pm = Utc(2025, 5, 1).AddHours(14);
        var start6pm = Utc(2025, 5, 1).AddHours(18);

        var id = await CreateWall(start8am, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var response1 = await PatchWall(id, new { Color = "blue", DateTime = start10am });
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var response2 = await PatchWall(id, new { Color = "green", DateTime = start2pm });
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 13));
        var response3 = await PatchWall(id, new { Color = "yellow", DateTime = start6pm });
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        // Final response should have yellow
        var wall = await response3.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(wall);
        Assert.Equal("yellow", wall.GetProperty("color").GetString());
    }

    [Fact]
    public async Task API_DeleteSameDayUpdate_Returns204()
    {
        // Verifies that the API returns proper error response (not just generic 404)
        // when hitting the intraday delete bug
        
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var start10am = Utc(2025, 5, 1).AddHours(10);
        var start2pm = Utc(2025, 5, 1).AddHours(14);

        var id = await CreateWall(start10am, "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, start2pm, "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var deleteResponse = await DeleteWall(id, start2pm);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task API_ErrorResponse_UpdateAtBusinessEnd_ReturnsOk()
    {
        // Verifies error response format for BusinessEnd boundary bug
        
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        using var scope = Fixture.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBitemporalRepository>();
        
        var wall = new Domain.Entities.Wall
        {
            Color = "red",
            BusinessStart = Utc(2025, 5, 1),
            BusinessEnd = Utc(2025, 5, 10)
        };
        await repo.InsertAsync(wall, CancellationToken.None);
        await repo.CommitChangesAsync(CancellationToken.None);
        var id = wall.Id;

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        var patchResponse = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 10) });

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
    }

    [Fact]
    public async Task API_ErrorResponse_DeleteAtBoundary_Returns404WithProblemDetails()
    {
        // Verifies error response format for boundary exclusion constraint bug

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));
        await UpdateWall(id, Utc(2025, 5, 10), "blue");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 12));
        var oneTickBefore = Utc(2025, 5, 10).AddTicks(-1);
        var deleteResponse = await DeleteWall(id, oneTickBefore);

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        // Verify ProblemDetails format
        var contentType = deleteResponse.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/problem+json", contentType);
    }

    // =====================================================================
    // RACE — True concurrent HTTP requests
    // =====================================================================

    [Fact]
    public async Task RACE01_ConcurrentPatch_SameWall_SameBusinessDate_NoServerErrors()
    {
        // Three concurrent PATCHes on the same wall at the same business date with different
        // colors. The service-level conflict check uses different color values so all three
        // pass it. At the DB layer the exclusion constraint serializes conflicting writes.
        // Invariant: no 500s, database ends in a consistent state with exactly one current row.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));
        var id = await CreateWall(Utc(2025, 5, 1), "red");

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 11));

        var tasks = new[]
        {
            PatchWall(id, new { Color = "blue",   DateTime = Utc(2025, 5, 3) }),
            PatchWall(id, new { Color = "green",  DateTime = Utc(2025, 5, 3) }),
            PatchWall(id, new { Color = "yellow", DateTime = Utc(2025, 5, 3) }),
        };

        var responses = await Task.WhenAll(tasks);

        // No response must be a server error.
        Assert.DoesNotContain(responses, r => (int)r.StatusCode >= 500);

        // At least one must have succeeded.
        Assert.Contains(responses, r => r.StatusCode == HttpStatusCode.OK);

        // Database invariant: exactly one current row.
        var current = await SnapshotWallHistoryAsync(Fixture, id);
        var active = current.Where(r => r.BusinessEnd == Infinity && r.SystemEnd == Infinity).ToList();
        Assert.Single(active);
    }

    [Fact]
    public async Task RACE02_ConcurrentPost_SameColor_NoServerErrors()
    {
        // Five concurrent POSTs with the same color. At least one should create the wall;
        // duplicates should be rejected as conflicts. No request should produce a server error.
        Fixture.TimeProvider.SetUtcNow(Utc(2025, 5, 10));

        var tasks = Enumerable.Range(0, 5)
            .Select(_ => PostWall(new { Color = "race-unique-color", DateTime = Utc(2025, 5, 1) }))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        Assert.DoesNotContain(responses, r => (int)r.StatusCode >= 500);
        Assert.Contains(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.All(responses, r => Assert.True(
            r.StatusCode is HttpStatusCode.Created or HttpStatusCode.Conflict,
            $"Unexpected status {(int)r.StatusCode}"));
    }
}
