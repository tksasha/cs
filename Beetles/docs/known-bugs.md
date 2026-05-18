# Known bugs

## 1. PATCH response is built from the request, not the persisted row

After a correction, the response can contain wrong temporal metadata.

Minimal scenario:

1. Create wall `red` effective `2025-05-01`.
2. Patch it to `blue` effective `2025-05-03`.

Expected:

- response `SystemStart = 2025-05-11T00:00:00Z` when that is the update system time;
- if the patch fills a gap, response `BusinessEnd` matches the bounded persisted row.

Actual:

- `SystemStart` can be `0001-01-01T00:00:00Z`;
- gap-fill response can return `BusinessEnd = infinity`.

Small test cases:

```csharp
[Fact]
public async Task Patch_ResponseReflectsPersistedSystemStart()
{
    var id = await CreateWall(Utc(2025, 5, 1), "red");
    var response = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });
    var payload = await response.Content.ReadFromJsonAsync<WallResponse>();

    Assert.Equal(Utc(2025, 5, 11), payload!.SystemStart);
}
```

Related failing coverage:

- `WallApiContractTests.C35_Patch_ResponseBody_ReflectsSystemStartAtUpdateTime`
- `WallApiDatabaseScenarioTests.E2E02_ResponsePayloads_MatchPersistedRows_AfterPatch`
- `WallApiDatabaseScenarioTests.E2E08b_UpdateInsideGap_ResponsePayload_MatchesBoundedPersistedRow`

## 2. Deleting a middle business event can overlap later history

Deleting an event between an earlier state and a later active state can violate PostgreSQL exclusion constraint `no_overlap_walls`.

Minimal scenario:

1. Create wall `red` effective `2025-05-01`.
2. Update to `blue` effective `2025-05-03`.
3. Update to `black` effective `2025-06-13`.
4. Delete the middle event at `2025-05-03`.

Expected:

| Business interval | Color |
| --- | --- |
| `[2025-05-01, 2025-06-13)` | `red` |
| `[2025-06-13, infinity)` | `black` |

Actual:

The delete path appends restored `red` to `infinity`, overlapping later `black`.

Small test case:

```csharp
[Fact]
public async Task DeleteMiddleEvent_RevertsOnlyUntilNextEvent()
{
    var id = await CreateWall(Utc(2025, 5, 1), "red");
    await UpdateWall(id, Utc(2025, 5, 3), "blue");
    await UpdateWall(id, Utc(2025, 6, 13), "black");

    await DeleteWall(id, Utc(2025, 5, 3));

    var active = ActiveWallRows(id);
    Assert.Equal("red", active[0].Color);
    Assert.Equal(Utc(2025, 6, 13), active[0].BusinessEnd);
    Assert.Equal("black", active[1].Color);
}
```

Related failing coverage:

- `WallBitemporalCompleteCoverageDbTests.DEL09_Delete_MiddleEvent_WithLaterEvents_RevertsOnlyUntilNextEvent`
- `WallApiDatabaseScenarioTests.E2E10_DeleteMiddleEvent_WithLaterEvents_RevertsToCorrectBoundedSegment`
- `WallBitemporalRandomizedModelTests.MODEL01_RandomizedMutations_MatchReferenceTimeline_AndPreserveInvariants`

Why the model test matters:

- the same overlap bug is reached through many generated mutation sequences, not only the small hand-written case above;
- failures consistently end at a middle-event delete that tries to create overlapping active intervals;
- this makes `MODEL01` useful as broad regression coverage after the targeted fix is implemented.

## 3. Duplicate detection checks historical rows, not only active rows

The API rejects valid changes when the same color existed in superseded history.

Minimal scenarios:

1. Reapply a previously superseded color at the same business date.
2. Create a new wall with a color that was used by a deleted wall.

Expected:

- both operations succeed if there is no active conflicting row.

Actual:

- both return `409 Conflict`.

Small test cases:

```csharp
[Fact]
public async Task Patch_BackToSupersededColor_Succeeds()
{
    var id = await CreateWall(Utc(2025, 5, 1), "red");
    await UpdateWall(id, Utc(2025, 5, 3), "blue");
    await UpdateWall(id, Utc(2025, 5, 3), "green");

    var response = await PatchWall(id, new { Color = "blue", DateTime = Utc(2025, 5, 3) });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

```csharp
[Fact]
public async Task Post_AfterDeletion_CanReuseColor()
{
    var id = await CreateWall(Utc(2025, 5, 1), "red");
    await DeleteWall(id, Utc(2025, 5, 1));

    var response = await PostWall(new { Color = "red", DateTime = Utc(2025, 6, 1) });

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
}
```

Related failing coverage:

- `WallBitemporalApiDbTests.BT22_Patch_BackToPreviouslySupersededColor_AtSameBusinessDate_Succeeds_ViaApi`
- `WallBitemporalApiDbTests.Tc13_PostAfterDeletion_SameColor_Succeeds_ViaApi`
