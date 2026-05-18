using System.Net;
using System.Net.Http.Json;

using Beetles.Application.Responses;
using Beetles.Application.Tests.Database;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Beetles.Application.Tests.Walls;

public abstract class WallApiTestBase : IAsyncLifetime
{
    private const string ConnectionStringKey = "ConnectionStrings:Development";

    protected readonly DatabaseFixture Fixture;
    protected HttpClient Client => _client;

    private WallApiFactory? _factory;
    private HttpClient _client = null!;

    protected WallApiTestBase(DatabaseFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await Fixture.ResetAsync();

        _factory = new WallApiFactory(Fixture.TimeProvider, Fixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    protected async Task<int> CreateWall(DateTimeOffset businessStart, string color, DateTimeOffset? businessEnd = null)
    {
        var response = await Client.PostAsJsonAsync("/walls/",
            new { Color = color, DateTime = businessStart });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<WallResponse>();
        Assert.NotNull(payload);
        return payload!.Id;
    }

    protected async Task UpdateWall(int wallId, DateTimeOffset businessStart, string color, DateTimeOffset? businessEnd = null)
    {
        var response = await Client.PatchAsJsonAsync($"/walls/{wallId}",
            new { Color = color, DateTime = businessStart });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    protected Task<HttpResponseMessage> PostWall(object body)
        => Client.PostAsJsonAsync("/walls/", body, CancellationToken.None);

    protected Task<HttpResponseMessage> PatchWall(int wallId, object body)
        => Client.PatchAsJsonAsync($"/walls/{wallId}", body, CancellationToken.None);

    protected Task<HttpResponseMessage> DeleteWall(int wallId, DateTimeOffset dateTime)
        => Client.DeleteAsync($"/walls/{wallId}?dateTime={Uri.EscapeDataString(dateTime.ToString("O"))}", CancellationToken.None);

    private sealed class WallApiFactory(TestTimeProvider timeProvider, string connectionString)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [ConnectionStringKey] = connectionString,
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(timeProvider);
            });
            builder.ConfigureLogging(logging => logging.ClearProviders());
        }
    }
}
