using Microsoft.EntityFrameworkCore;

namespace EFDemo;

public class DatabaseContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
}
