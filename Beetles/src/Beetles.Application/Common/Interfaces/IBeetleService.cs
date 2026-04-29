using Beetles.Application.Responses;

namespace Beetles.Application.Common.Interfaces;

public interface IBeetleService
{
    Task<List<BeetleResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<BeetleResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
}
