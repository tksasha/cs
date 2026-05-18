using Beetles.Application.Exceptions;
using Beetles.Application.Tests.Database;
using Beetles.Domain.Entities;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// Deterministic randomized regression against a small business-time reference model.
/// The model tracks only the currently known business timeline; the repository is also
/// checked for structural invariants after every generated mutation.
/// </summary>
[Collection("Database")]
public sealed class WallBitemporalRandomizedModelTests : WallDbTestBase
{
    private static readonly string[] Colors = ["red", "blue", "green", "yellow", "black", "white"];

    public WallBitemporalRandomizedModelTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Theory]
    [InlineData(101)]
    [InlineData(102)]
    [InlineData(103)]
    [InlineData(104)]
    [InlineData(105)]
    [InlineData(201)]
    [InlineData(202)]
    [InlineData(203)]
    [InlineData(204)]
    [InlineData(205)]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(303)]
    [InlineData(304)]
    [InlineData(305)]
    [InlineData(401)]
    [InlineData(402)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(405)]
    [InlineData(501)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    [InlineData(505)]
    [InlineData(601)]
    [InlineData(602)]
    [InlineData(603)]
    [InlineData(604)]
    [InlineData(605)]
    [InlineData(701)]
    [InlineData(702)]
    [InlineData(703)]
    [InlineData(704)]
    [InlineData(705)]
    [InlineData(801)]
    [InlineData(802)]
    [InlineData(803)]
    [InlineData(804)]
    [InlineData(805)]
    [InlineData(901)]
    [InlineData(902)]
    [InlineData(903)]
    [InlineData(904)]
    [InlineData(905)]
    [InlineData(1001)]
    [InlineData(1002)]
    [InlineData(1003)]
    [InlineData(1004)]
    [InlineData(1005)]
    public async Task MODEL01_RandomizedMutations_MatchReferenceTimeline_AndPreserveInvariants(int seed)
    {
        var random = new Random(seed);
        var model = new SortedDictionary<DateTimeOffset, string>();
        var operations = new List<string>();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 1));
        var id = await CreateWall(BusinessDate(1), "red");
        model[BusinessDate(1)] = "red";
        await AssertMatchesModel(id, model);

        for (var step = 1; step <= 100; step++)
        {
            Fixture.TimeProvider.SetUtcNow(Utc(2025, 6, 1).AddDays(step));

            var shouldDelete = model.Count > 0 && random.NextDouble() < 0.35;
            if (shouldDelete)
            {
                var eventDate = model.Keys.ElementAt(random.Next(model.Count));
                operations.Add($"DELETE {eventDate:O}");
                try
                {
                    await DeleteWall(id, eventDate);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Seed {seed}, step {step}. Operations: {string.Join(" | ", operations)}",
                        ex);
                }
                model.Remove(eventDate);
            }
            else
            {
                var eventDate = BusinessDate(random.Next(1, 16));
                var color = Colors[random.Next(Colors.Length)];
                operations.Add($"PATCH {eventDate:O} {color}");
                try
                {
                    await UpdateWall(id, eventDate, color);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Seed {seed}, step {step}. Operations: {string.Join(" | ", operations)}",
                        ex);
                }
                model[eventDate] = color;
            }

            await AssertMatchesModel(id, model);
        }
    }

    [Theory]
    [InlineData(2101)]
    [InlineData(2202)]
    [InlineData(2303)]
    [InlineData(2404)]
    [InlineData(2505)]
    [InlineData(2606)]
    [InlineData(2707)]
    [InlineData(2808)]
    [InlineData(2909)]
    [InlineData(3001)]
    public async Task MODEL02_RandomizedUpdatesOnly_MatchReferenceTimeline_AndPreserveInvariants(int seed)
    {
        var random = new Random(seed);
        var model = new SortedDictionary<DateTimeOffset, string>();

        Fixture.TimeProvider.SetUtcNow(Utc(2025, 8, 1));
        var id = await CreateWall(BusinessDate(1), "red");
        model[BusinessDate(1)] = "red";
        await AssertMatchesModel(id, model);

        for (var step = 1; step <= 100; step++)
        {
            Fixture.TimeProvider.SetUtcNow(Utc(2025, 8, 1).AddDays(step));

            var eventDate = BusinessDate(random.Next(1, 16));
            var color = Colors[random.Next(Colors.Length)];

            await UpdateWall(id, eventDate, color);
            model[eventDate] = color;

            await AssertMatchesModel(id, model);
        }
    }

    private async Task AssertMatchesModel(int id, SortedDictionary<DateTimeOffset, string> model)
    {
        AssertActiveRowsDoNotOverlap(id);

        var currentRows = Repo.QueryAll<Wall>()
            .Where(w => w.Id == id && w.BusinessEnd == Infinity && w.SystemEnd == Infinity)
            .ToList();

        if (model.Count == 0)
        {
            Assert.Empty(currentRows);
        }
        else
        {
            Assert.Single(currentRows);
            Assert.Equal(model.Last().Value, currentRows[0].Color);
        }

        foreach (var probe in ProbeDates())
        {
            var expected = ExpectedColorAt(model, probe);

            if (expected is null)
            {
                await Assert.ThrowsAsync<NotFoundException>(() =>
                    Repo.GetAsync<Wall>(id, probe, CancellationToken.None));
            }
            else
            {
                var actual = await Repo.GetAsync<Wall>(id, probe, CancellationToken.None);
                Assert.Equal(expected, actual.Color);
            }
        }
    }

    private static IEnumerable<DateTimeOffset> ProbeDates()
    {
        for (var day = 1; day <= 16; day++)
        {
            yield return BusinessDate(day);
            yield return BusinessDate(day).AddHours(12);
        }
    }

    private static string? ExpectedColorAt(
        SortedDictionary<DateTimeOffset, string> model,
        DateTimeOffset date)
    {
        string? color = null;

        foreach (var entry in model)
        {
            if (entry.Key > date)
                break;

            color = entry.Value;
        }

        return color;
    }

    private static DateTimeOffset BusinessDate(int day)
        => Utc(2025, 5, day);
}
