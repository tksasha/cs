using Beetles.Domain.Entities;

namespace Beetles.Application.Common.Interfaces;

public interface IBeetleService
{
    Task<List<Beetle>> GetAllAsync(CancellationToken cancellationToken);

    Task<Beetle> GetByIdAsync(int id, CancellationToken cancellationToken);
}
