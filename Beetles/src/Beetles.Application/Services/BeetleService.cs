using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Beetles.Application.Responses;
using Microsoft.EntityFrameworkCore;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Requests;
using Beetles.Application.Exceptions;

namespace Beetles.Application.Services;

internal class BeetleService(IBitemporalRepository repository) : IBeetleService
{
    public Task<List<BeetleResponse>> GetAllAsync(CancellationToken cancellationToken)
        => repository
            .QueryAll<Beetle>()
            .Select(b => b.ToResponse())
            .ToListAsync(cancellationToken);

    public async Task<BeetleResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
        => (await repository.GetByIdAsync<Beetle>(id, cancellationToken)).ToResponse();

    private async Task ValidateNameIsUnique(string name, CancellationToken cancellationToken)
    {
        bool any = await repository.QueryAll<Beetle>()
            .AnyAsync(e => e.Name == name
                && e.RecordedTo == DateTimeOffset.MaxValue, cancellationToken);

        if (any) throw new ConflictException();
    }

    public async Task<BeetleResponse> CreateAsync(BeetleRequest request, CancellationToken cancellationToken)
    {
        await ValidateNameIsUnique(request.Name, cancellationToken);

        var beetle = await repository.InsertAsync(request.ToEntity(), cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);

        return beetle.ToResponse();
    }

    public async Task ValidateNameIsUnique(int id, string name, CancellationToken cancellationToken)
    {
        bool any = await repository
            .QueryAll<Beetle>()
            .AnyAsync(e => e.Id != id && e.Name == name, cancellationToken);

        if (any) throw new ConflictException();
    }

    public async Task<BeetleResponse> UpdateAsync(
        int id,
        BeetleRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateNameIsUnique(id, request.Name, cancellationToken);

        var currentVersion = await repository.GetByIdAsync<Beetle>(id, cancellationToken);

        var newVersion = currentVersion.CreateNewVersion();

        newVersion.Name = request.Name;
        newVersion.ValidFrom = request.ValidFrom;
        newVersion.ValidTo = request.ValidTo;

        await repository.UpdateAsync(currentVersion, newVersion, cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);

        return newVersion.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await repository.DeleteAsync<Beetle>(id, cancellationToken);

        await repository.CommitChangesAsync(cancellationToken);
    }
}
