using EFDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace EFDemo;

public class DatabaseContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Post> Posts { get; set; }
}
