using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Be;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseNpgsql(configuration.GetConnectionString("Development"))
            .UseSnakeCaseNamingConvention()
            .Options;

        return new DatabaseContext(options);
    }
}
