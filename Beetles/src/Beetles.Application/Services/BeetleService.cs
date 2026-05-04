using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Beetles.Application.Responses;
using Microsoft.EntityFrameworkCore;
using Beetles.Application.Common.Mappings;
using Beetles.Application.Requests;
using Beetles.Application.Exceptions;

namespace Beetles.Application.Services;

internal class BeetleService(IBitemporalRepository bitemporalRepository, IRepository repository) : IBeetleService
{
    public Task<List<BeetleResponse>> GetAllAsync(
        CancellationToken cancellationToken,
        DateTimeOffset? valid = null,
        DateTimeOffset? recorded = null)
        => bitemporalRepository
            .QueryAll<Beetle>(valid, recorded)
            .Include(b => b.Colony)
            .Select(b => b.ToResponse())
            .ToListAsync(cancellationToken);

    public async Task<BeetleResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
        => (await bitemporalRepository.GetByIdAsync<Beetle>(id, cancellationToken)).ToResponse();

    private async Task ValidateNameIsUnique(string name, CancellationToken cancellationToken)
    {
        bool any = await bitemporalRepository.QueryAll<Beetle>()
            .AnyAsync(e => e.Name == name
                && e.RecordedTo == DateTimeOffset.MaxValue, cancellationToken);

        if (any) throw new ConflictException();
    }

    private Task<Colony> GetColonyAsync(int id, CancellationToken cancellationToken)
        => repository.GetByIdAsync<Colony>(id, cancellationToken);

    public async Task<BeetleResponse> CreateAsync(BeetleRequest request, CancellationToken cancellationToken)
    {
        await ValidateNameIsUnique(request.Name, cancellationToken);

        var beetle = request.ToEntity();

        beetle.Colony = await GetColonyAsync(request.ColonyId, cancellationToken);

        await bitemporalRepository.InsertAsync(beetle, cancellationToken);

        await bitemporalRepository.CommitChangesAsync(cancellationToken);

        return beetle.ToResponse();
    }

    private async Task ValidateNameIsUnique(int id, string name, CancellationToken cancellationToken)
    {
        bool any = await bitemporalRepository
            .QueryAll<Beetle>()
            .AnyAsync(e => e.Id != id
                && e.Name == name
                && e.RecordedTo == DateTimeOffset.MaxValue, cancellationToken);

        if (any) throw new ConflictException();
    }

    public async Task<BeetleResponse> UpdateAsync(
        int id,
        BeetleRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateNameIsUnique(id, request.Name, cancellationToken);

        var currentVersion = await bitemporalRepository.GetByIdAsync<Beetle>(id, cancellationToken);

        var newVersion = currentVersion.CreateNewVersion();

        newVersion.Name = request.Name;
        newVersion.ValidFrom = request.ValidFrom;
        newVersion.Colony = await GetColonyAsync(request.ColonyId, cancellationToken);

        await bitemporalRepository.UpdateAsync(currentVersion, newVersion, cancellationToken);

        await bitemporalRepository.CommitChangesAsync(cancellationToken);

        return newVersion.ToResponse();
    }

    public async Task<BeetleResponse> CorrectAsync(
        int id,
        CorrectBeetleRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateNameIsUnique(id, request.Name, cancellationToken);

        var currentVersion = await bitemporalRepository.GetByIdAsync<Beetle>(id, cancellationToken);

        var newVersion = currentVersion.CreateNewVersion();

        newVersion.Name = request.Name;
        newVersion.Colony = await GetColonyAsync(request.ColonyId, cancellationToken);

        await bitemporalRepository.CorrectAsync(currentVersion, newVersion, cancellationToken);

        await bitemporalRepository.CommitChangesAsync(cancellationToken);

        return newVersion.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await bitemporalRepository.DeleteAsync<Beetle>(id, cancellationToken);

        await bitemporalRepository.CommitChangesAsync(cancellationToken);
    }
}
