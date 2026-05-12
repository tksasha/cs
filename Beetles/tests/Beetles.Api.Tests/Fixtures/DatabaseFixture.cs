using Beetles.Application.Common.Interfaces;
using Beetles.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Moq;

using Testcontainers.PostgreSql;

namespace Beetles.Api.Tests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer PostgreSqlContainer = new PostgreSqlBuilder("postgres:18").Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    private IServiceScope? _scope;

    public DatabaseContext DatabaseContext { get; private set; } = null!;

    public Mock<TimeProvider> TimeProviderMock { get; } = new();

    public IBitemporalRepository BitemporalRepository { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await PostgreSqlContainer.StartAsync(CancellationToken.None);

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Development", PostgreSqlContainer.GetConnectionString());

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton(TimeProviderMock.Object);
            });
        });

        _scope = Factory.Services.CreateScope();

        var serviceProvider = _scope.ServiceProvider;

        DatabaseContext = serviceProvider.GetRequiredService<DatabaseContext>();

        await DatabaseContext.Database.MigrateAsync(CancellationToken.None);

        BitemporalRepository = serviceProvider.GetRequiredService<IBitemporalRepository>();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();

        await Factory.DisposeAsync();

        await PostgreSqlContainer.StopAsync(CancellationToken.None);
    }
}
