using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Beetles.Application.Responses;
using Microsoft.EntityFrameworkCore;
using Beetles.Application.Common.Mappings;

namespace Beetles.Application.Services;

internal class BeetleService(IRepository repository) : IBeetleService
{
    public Task<List<BeetleResponse>> GetAllAsync(CancellationToken cancellationToken)
        => repository
            .QueryAll<Beetle>()
            .Select(b => b.ToResponse())
            .ToListAsync(cancellationToken);

    public async Task<BeetleResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var beetle = await repository.GetByIdAsync<Beetle>(id, cancellationToken);

        return beetle.ToResponse();
    }
}
