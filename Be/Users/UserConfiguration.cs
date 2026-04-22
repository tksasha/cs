using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Be.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => new { u.Id, u.ValidFrom, u.RecordedFrom });

        builder.Property(u => u.ValidTo).HasDefaultValueSql("'infinity'::timestamptz");

        builder.Property(u => u.RecordedFrom).HasDefaultValueSql("now()");

        builder.Property(u => u.RecordedTo).HasDefaultValueSql("'infinity'::timestamptz");
    }
}
