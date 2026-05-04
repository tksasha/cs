using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beetles.Infrastructure.EntityConfigurations;

internal sealed class WallConfiguration : IEntityTypeConfiguration<Wall>
{
    public void Configure(EntityTypeBuilder<Wall> builder)
    {
        builder.HasKey(b => new { b.Id, b.ValidFrom, b.RecordedFrom });

        builder.Property(b => b.Id).UseIdentityByDefaultColumn();

        builder.Property(b => b.ValidTo).HasDefaultValueSql("'infinity'::timestamptz");

        builder.Property(b => b.RecordedFrom).HasDefaultValueSql("now()");

        builder.Property(b => b.RecordedTo).HasDefaultValueSql("'infinity'::timestamptz");
    }
}
