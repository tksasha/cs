using Beetles.Application.Common.Interfaces;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Responses;
using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beetles.Application.Services;

internal sealed class ColonyService(IRepository repository) : IColonyService
{
    public async Task<List<ColonyResponse>> GetAllAsync(CancellationToken cancellationToken)
        => await repository
            .QueryAll<Colony>()
            .Select(c => c.ToResponse())
            .ToListAsync(cancellationToken);
}
