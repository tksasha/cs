using Beetles.Infrastructure;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Beetles.Api.Tests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private static readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder("postgres:18").Build();

    public readonly WebApplicationFactory<Program> Factory = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:Development", PostgreSqlContainer.GetConnectionString()));

    public async Task InitializeAsync()
    {
        await PostgreSqlContainer.StartAsync(CancellationToken.None);

        using var scope = Factory.Services.CreateScope();

        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        await databaseContext.Database.MigrateAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();

        await PostgreSqlContainer.StopAsync(CancellationToken.None);
    }
}
