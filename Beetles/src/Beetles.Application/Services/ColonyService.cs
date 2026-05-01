using Beetles.Application.Common.Interfaces;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Exceptions;
using Beetles.Application.Requests;
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

    public async Task<ColonyResponse> CreateAsync(ColonyRequest request, CancellationToken cancellationToken)
    {
        bool any = await repository.QueryAll<Beetle>().AnyAsync(b => b.Name == request.Name, cancellationToken);

        if (any) throw new ConflictException();

        var colony = await repository.InsertAsync(request.ToEntity(), cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);

        return colony.ToResponse();
    }

    private async Task<Colony> GetAsync(int id, CancellationToken cancellationToken)
        => await repository.QueryAll<Colony>().FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException();

    public async Task<ColonyResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
        => (await GetAsync(id, cancellationToken)).ToResponse();

    public async Task<ColonyResponse> UpdateAsync(int id, ColonyRequest request, CancellationToken cancellationToken)
    {
        bool any = await repository
            .QueryAll<Colony>()
            .AnyAsync(c => c.Id != id && c.Name == request.Name, cancellationToken);

        if (any) throw new ConflictException();

        var colony = await GetAsync(id, cancellationToken);

        colony.Name = request.Name;

        repository.Update(colony);

        await repository.CommitChangesAsync(cancellationToken);

        return colony.ToResponse();
    }
}
