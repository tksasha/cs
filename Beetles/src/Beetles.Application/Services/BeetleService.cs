using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Beetles.Application.Responses;
using Microsoft.EntityFrameworkCore;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Requests;
using Beetles.Application.Exceptions;

namespace Beetles.Application.Services;

internal class BeetleService(IRepository repository) : IBeetleService
{
    public Task<List<BeetleResponse>> GetAllAsync(CancellationToken cancellationToken)
        => repository
            .QueryAll<Beetle>()
            .Select(b => b.ToResponse())
            .ToListAsync(cancellationToken);

    private async Task<Beetle> GetAsync(int id, CancellationToken cancellationToken)
        => await repository.QueryAll<Beetle>().FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException();

    public async Task<BeetleResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
        => (await GetAsync(id, cancellationToken)).ToResponse();

    public async Task<BeetleResponse> CreateAsync(BeetleRequest request, CancellationToken cancellationToken)
    {
        bool any = await repository.QueryAll<Beetle>().AnyAsync(e => e.Name == request.Name, cancellationToken);

        if (any) throw new ConflictException();

        var beetle = await repository.InsertAsync(request.ToEntity(), cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);

        return beetle.ToResponse();
    }

    public async Task<BeetleResponse> UpdateAsync(
        int id,
        BeetleRequest request,
        CancellationToken cancellationToken)
    {
        bool any = await repository
            .QueryAll<Beetle>()
            .AnyAsync(e => e.Id != id && e.Name == request.Name, cancellationToken);

        if (any) throw new ConflictException();

        var beetle = await GetAsync(id, cancellationToken);

        beetle.Name = request.Name;

        repository.Update(beetle);

        await repository.CommitChangesAsync(cancellationToken);

        return beetle.ToResponse();
    }
}
