using Microsoft.EntityFrameworkCore;

namespace Be;

public class DatabaseContext(DbContextOptions<DatabaseContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<User.Model> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User.Model>()
            .ToTable("users");
    }
}
