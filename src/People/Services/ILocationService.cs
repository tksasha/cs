using People.Entities;

namespace People.Services;

public interface ILocationService
{
    Task<List<Location>> GetAllAsync(CancellationToken cancellationToken);
}
