using Beetles.Application.Requests;
using Beetles.Application.Responses;

namespace Beetles.Application.Common.Interfaces;

public interface IBeetleService
{
    Task<List<BeetleResponse>> GetAllAsync(
        CancellationToken cancellationToken,
        DateTimeOffset? valid = null,
        DateTimeOffset? recorded = null);

    Task<BeetleResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<BeetleResponse> CreateAsync(BeetleRequest request, CancellationToken cancellationToken);

    Task<BeetleResponse> UpdateAsync(int Id, BeetleRequest request, CancellationToken cancellationToken);

    Task<BeetleResponse> CorrectAsync(int id, CorrectBeetleRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
