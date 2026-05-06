using Beetles.Application.Common.Interfaces;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using Beetles.Domain.Entities;

namespace Beetles.Application.Services;

public sealed class WallService(IBitemporalRepository repository) : IWallService
{
    public async Task<WallResponse> CreateAsync(WallRequest request, CancellationToken cancellationToken)
    {
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
}
