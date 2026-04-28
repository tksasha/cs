using Microsoft.EntityFrameworkCore;

using People.Entities;
using People.Repositories;

namespace People.Services;

internal class LocationService(IRepository repository) : ILocationService
{
    public Task<List<Location>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAll<Location>().ToListAsync(cancellationToken);
}
