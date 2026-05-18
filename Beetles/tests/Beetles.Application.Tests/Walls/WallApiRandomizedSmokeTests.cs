using System.Net;

using Beetles.Application.Tests.Database;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// Small deterministic API smoke pass over mixed mutations. This intentionally stays
/// lighter than the repository model test because each step crosses the HTTP boundary.
/// </summary>
[Collection("Database")]
public sealed class WallApiRandomizedSmokeTests : WallApiTestBase
{
    private static readonly string[] Colors = ["red", "blue", "green", "yellow", "black", "white"];

    public WallApiRandomizedSmokeTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Theory]
    [InlineData(1101)]
    [InlineData(1202)]
    [InlineData(1303)]
    [InlineData(1404)]
    [InlineData(1505)]
    public async Task API_MODEL01_RandomizedMutations_ReturnExpectedClientResponses_AndPreserveNoOverlap(int seed)
    {
        var random = new Random(seed);
        var knownEvents = new SortedSet<DateTimeOffset>();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 7, 1));
        var id = await CreateWall(BusinessDate(1), "red");
        knownEvents.Add(BusinessDate(1));

        for (var step = 1; step <= 30; step++)
        {
            Fixture.TimeProvider.SetUtcNow(Utc(2025, 7, 1).AddDays(step));

            var shouldDelete = knownEvents.Count > 0 && random.NextDouble() < 0.30;
            if (shouldDelete)
            {
                var eventDate = knownEvents.ElementAt(random.Next(knownEvents.Count));
                var response = await DeleteWall(id, eventDate);

                Assert.True(
                    response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.Conflict,
                    $"Unexpected DELETE status {(int)response.StatusCode} for seed {seed}, step {step}.");

                if (response.StatusCode == HttpStatusCode.NoContent)
                    knownEvents.Remove(eventDate);
            }
            else
            {
                var eventDate = BusinessDate(random.Next(1, 16));
                var color = Colors[random.Next(Colors.Length)];
                var response = await PatchWall(id, new { Color = color, DateTime = eventDate });

                Assert.True(
                    response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict,
                    $"Unexpected PATCH status {(int)response.StatusCode} for seed {seed}, step {step}.");

                if (response.StatusCode == HttpStatusCode.OK)
                    knownEvents.Add(eventDate);
            }

            var rows = await SnapshotWallHistoryAsync(Fixture, id);
            var active = rows
                .Where(r => r.SystemEnd == Infinity)
                .OrderBy(r => r.BusinessStart)
                .ToList();

            for (var i = 1; i < active.Count; i++)
                Assert.True(active[i - 1].BusinessEnd <= active[i].BusinessStart);
        }
    }

    private static DateTimeOffset BusinessDate(int day)
        => Utc(2025, 5, day);
}
