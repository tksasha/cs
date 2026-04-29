
using System;

using Microsoft.EntityFrameworkCore;

using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;

namespace Beetles.Application.Services;

internal class BeetleService(IRepository repository) : IBeetleService
{
    public Task<List<Beetle>> GetAllAsync(CancellationToken cancellationToken)
        => repository.QueryAll<Beetle>().ToListAsync(cancellationToken);

    public Task<Beetle> GetByIdAsync(int id, CancellationToken cancellationToken)
        => repository.GetByIdAsync<Beetle>(id, cancellationToken);
}
