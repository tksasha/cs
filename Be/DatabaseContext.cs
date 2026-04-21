using Microsoft.EntityFrameworkCore;

namespace Be;

public class DatabaseContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<User.User> Users { get; set; }
}
