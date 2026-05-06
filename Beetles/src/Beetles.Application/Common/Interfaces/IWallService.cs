using Beetles.Application.Requests;
using Beetles.Application.Responses;

namespace Beetles.Application.Common.Interfaces;

public interface IWallService
{
    Task<WallResponse> CreateAsync(WallRequest request, CancellationToken cancellationToken);

    Task<WallResponse> UpdateAsync(int id, WallRequest request, CancellationToken cancellationToken);
}
