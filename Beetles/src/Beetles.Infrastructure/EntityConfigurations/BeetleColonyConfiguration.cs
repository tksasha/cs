using Beetles.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beetles.Infrastructure.EntityConfigurations;

internal sealed class BeetleColonyConfiguration : IEntityTypeConfiguration<BeetleColony>
{
    public void Configure(EntityTypeBuilder<BeetleColony> builder)
    {
        builder.HasKey(b => new { b.BeetleId, b.ColonyId, b.ValidFrom, b.RecordedFrom });

        builder.Property(b => b.ValidTo).HasDefaultValueSql("'infinity'::timestamptz");

        builder.Property(b => b.RecordedFrom).HasDefaultValueSql("now()");

        builder.Property(b => b.RecordedTo).HasDefaultValueSql("'infinity'::timestamptz");
    }
}
