using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beetles.Infrastructure.EntityConfigurations;

internal sealed class WallConfiguration : IEntityTypeConfiguration<Wall>
{
    public void Configure(EntityTypeBuilder<Wall> builder)
    {
        builder.HasKey(b => new { b.Id, b.BusinessStart, b.BusinessEnd });

        builder.Property(b => b.Id).UseIdentityByDefaultColumn();
    }
}
