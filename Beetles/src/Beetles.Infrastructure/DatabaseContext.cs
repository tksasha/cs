using Beetles.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace Beetles.Infrastructure;

public sealed class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Beetle> Beetles { get; set; }
    public DbSet<Colony> Colonies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}
