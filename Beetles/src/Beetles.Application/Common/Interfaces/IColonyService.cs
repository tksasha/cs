using Beetles.Application.Responses;

namespace Beetles.Application.Common.Interfaces;

public interface IColonyService
{
    Task<List<ColonyResponse>> GetAllAsync(CancellationToken cancellationToken);
}
