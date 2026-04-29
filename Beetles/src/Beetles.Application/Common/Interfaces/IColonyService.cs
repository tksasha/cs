using Beetles.Application.Requests;
using Beetles.Application.Responses;

namespace Beetles.Application.Common.Interfaces;

public interface IColonyService
{
    Task<List<ColonyResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<ColonyResponse> CreateAsync(ColonyRequest request, CancellationToken cancellationToken);

    Task<ColonyResponse> UpdateAsync(int id, ColonyRequest request, CancellationToken cancellationToken);
}
