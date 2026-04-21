using Microsoft.EntityFrameworkCore;

namespace Be;

public class DatabaseContext(DbContextOptions<DatabaseContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<Users.User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users.User>()
            .ToTable("users");
    }
}
