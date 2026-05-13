using Beetles.Application.Common.Interfaces;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Exceptions;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using Beetles.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Beetles.Application.Services;

internal sealed class WallService(IBitemporalRepository repository) : IWallService
{
    public async Task<WallResponse> CreateAsync(WallRequest request, CancellationToken cancellationToken)
    {
        if (await repository.QueryAll<Wall>().AnyAsync(e => e.Color == request.Color, cancellationToken))
        {
            throw new ConflictException();
        }

        var wall = request.ToEntity();

        await repository.InsertAsync(wall, cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);

        return wall.ToResponse();
    }

    public async Task<WallResponse> UpdateAsync(int id, WallRequest request, CancellationToken cancellationToken)
    {
        var wall = request.ToEntity();
        wall.Id = id;

        await repository.UpdateAsync(wall, cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);

        return wall.ToResponse();
    }

    public async Task DeleteAsync(int id, DateTimeOffset date, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync<Wall>(id, date, cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);
    }
}
