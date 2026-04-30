using Beetles.Application.Requests;
using Beetles.Application.Responses;

namespace Beetles.Application.Common.Interfaces;

public interface IBeetleColonyService
{
    Task<BeetleColonyResponse> CreateAsync(BeetleColonyRequest request, CancellationToken cancellationToken);
}
