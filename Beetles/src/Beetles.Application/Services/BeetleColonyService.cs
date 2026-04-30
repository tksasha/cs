using Beetles.Application.Common.Interfaces;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Requests;
using Beetles.Application.Responses;
using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beetles.Application.Services;

internal sealed class BeetleColonyService(IBitemporalRepository repository) : IBeetleColonyService
{
    public async Task<BeetleColonyResponse> CreateAsync(
        BeetleColonyRequest request, CancellationToken cancellationToken)
    {
        var beetleColony = request.ToEntity();

        await repository.InsertAsync(beetleColony, cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);

        return beetleColony.ToResponse();
    }

    public async Task<List<BeetleColonyResponse>> GetAllAsync(CancellationToken cancellationToken)
        => await repository
            .QueryAll<BeetleColony>()
            .Include(e => e.Beetle)
            .Include(e => e.Colony)
            .Select(e => e.ToResponse())
            .ToListAsync(cancellationToken);
}
