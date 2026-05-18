using System.Net.Http;
using System.Text.Json;

using Beetles.Application.Tests.Database;
using Beetles.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Beetles.Application.Tests.Walls;

internal static class WallTestHelpers
{
    internal static readonly DateTimeOffset Infinity = DateTimeOffset.MaxValue;

    internal static DateTimeOffset Utc(int year, int month, int day)
        => new(year, month, day, 0, 0, 0, TimeSpan.Zero);

    internal static WallRow Row(
        DateTimeOffset businessStart,
        DateTimeOffset businessEnd,
        DateTimeOffset systemStart,
        DateTimeOffset systemEnd,
        string color)
        => new(businessStart, businessEnd, systemStart, systemEnd, color);

    internal static async Task AssertWallsAsync(
        DatabaseFixture fixture,
        params WallRow[] expected)
    {
        using var scope = fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var rows = await context.Walls.AsNoTracking().ToListAsync();

        var actual = rows
            .Select(w => new WallRow(
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

        var expectedSorted = expected
            .OrderBy(r => r.BusinessStart)
            .ThenBy(r => r.BusinessEnd)
            .ThenBy(r => r.SystemStart)
            .ThenBy(r => r.SystemEnd)
            .ThenBy(r => r.Color)
            .ToList();

        Assert.Equal(expectedSorted.Count, actual.Count);
        Assert.Equal(expectedSorted, actual);

        AssertNoActiveBusinessOverlaps(rows);
    }

    internal static async Task<List<WallRow>> SnapshotWallHistoryAsync(DatabaseFixture fixture, int id)
    {
        using var scope = fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var rows = await context.Walls
            .AsNoTracking()
            .Where(w => w.Id == id)
            .ToListAsync();

        return rows
            .Select(w => new WallRow(
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
    }

    internal static async Task<JsonElement> AssertProblemDetailsAsync(HttpResponseMessage response, int statusCode, string title)
    {
        Assert.Equal(statusCode, (int)response.StatusCode);

        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement.Clone();

        Assert.True(root.TryGetProperty("title", out var actualTitle));
        Assert.Equal(title, actualTitle.GetString());

        Assert.True(root.TryGetProperty("status", out var actualStatus));
        Assert.Equal(statusCode, actualStatus.GetInt32());

        Assert.True(root.TryGetProperty("instance", out _));

        return root;
    }

    internal static async Task AssertValidationProblemContainsAsync(HttpResponseMessage response, string message)
    {
        var root = await AssertProblemDetailsAsync(response, 422, "Validation Failed");

        Assert.True(root.TryGetProperty("errors", out var errors));

        var messages = errors
            .EnumerateObject()
            .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString() ?? string.Empty))
            .ToList();

        Assert.Contains(messages, m => m.Contains(message, StringComparison.Ordinal));
    }

    private static void AssertNoActiveBusinessOverlaps(IReadOnlyCollection<Beetles.Domain.Entities.Wall> rows)
    {
        var activeRows = rows
            .Where(w => w.SystemEnd == Infinity)
            .GroupBy(w => w.Id);

        foreach (var group in activeRows)
        {
            var ordered = group
                .OrderBy(w => w.BusinessStart)
                .ThenBy(w => w.BusinessEnd ?? Infinity)
                .ToList();

            for (var i = 1; i < ordered.Count; i++)
            {
                var previousEnd = ordered[i - 1].BusinessEnd ?? Infinity;
                Assert.True(previousEnd <= ordered[i].BusinessStart,
                    $"Active business overlap for entity {group.Key}: {ordered[i - 1].BusinessStart:o}-{previousEnd:o} overlaps {ordered[i].BusinessStart:o}-{(ordered[i].BusinessEnd ?? Infinity):o}.");
            }
        }
    }
}

internal sealed record WallRow(
    DateTimeOffset BusinessStart,
    DateTimeOffset BusinessEnd,
    DateTimeOffset SystemStart,
    DateTimeOffset SystemEnd,
    string Color);
