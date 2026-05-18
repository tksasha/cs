using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;

namespace Beetles.Application.Tests.Walls;

internal static class WallDbHelper
{
    internal static async Task<int> CreateWall(
        IBitemporalRepository repo,
        DateTimeOffset businessStart,
        string color,
        DateTimeOffset? businessEnd = null)
    {
        var wall = new Wall
        {
            Color = color,
            BusinessStart = businessStart,
            BusinessEnd = businessEnd ?? WallTestHelpers.Infinity,
        };

        await repo.InsertAsync(wall, CancellationToken.None);
        await repo.CommitChangesAsync(CancellationToken.None);

        return wall.Id;
    }

    internal static async Task UpdateWall(
        IBitemporalRepository repo,
        int wallId,
        DateTimeOffset businessStart,
        string color,
        DateTimeOffset? businessEnd = null)
    {
        var wall = new Wall
        {
            Id = wallId,
            Color = color,
            BusinessStart = businessStart,
            BusinessEnd = businessEnd,
        };

        await repo.UpdateAsync(wall, CancellationToken.None);
        await repo.CommitChangesAsync(CancellationToken.None);
    }

    internal static async Task DeleteWall(
        IBitemporalRepository repo,
        int wallId,
        DateTimeOffset date)
    {
        await repo.DeleteAsync<Wall>(wallId, date, CancellationToken.None);
        await repo.CommitChangesAsync(CancellationToken.None);
    }
}
