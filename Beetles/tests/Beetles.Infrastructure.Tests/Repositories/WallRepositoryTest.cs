using Beetles.Application.Common.Interfaces;
using Beetles.Domain.Entities;
using Beetles.Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Beetles.Infrastructure.Tests.Repositories;

[Collection("Database Collection")]
public sealed class WallRepositoryTest(DatabaseFixture fixture) : IAsyncLifetime
{
    private readonly DatabaseContext _databaseContext = fixture.DatabaseContext;

    private readonly Mock<TimeProvider> _timeProviderMock = fixture.TimeProviderMock;

    private readonly IBitemporalRepository _repository = fixture.BitemporalRepository;

    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public async Task InitializeAsync()
    {
        await _databaseContext.Database.ExecuteSqlRawAsync("TRUNCATE walls", _cancellationToken);
    }

    public async Task DisposeAsync()
    { }

    [Fact]
    public async Task ShouldInsertAsync()
    {
        var now = DateTimeOffset.Parse("2025-05-10T00:00:00Z");

        _timeProviderMock.Setup(p => p.GetUtcNow()).Returns(now);

        var businessStart = DateTimeOffset.Parse("2025-05-01T00:00:00Z");

        var wall = new Wall { Color = "red", BusinessStart = businessStart };

        await _repository.InsertAsync(wall, _cancellationToken);

        await _repository.CommitChangesAsync(_cancellationToken);

        var inserted = _repository.QueryAll<Wall>()
            .FirstOrDefault();

        Assert.Multiple(
            () => Assert.NotNull(inserted),
            () => Assert.Equal("red", inserted?.Color),
            () => Assert.Equal(businessStart, inserted?.BusinessStart),
            () => Assert.Equal(DateTimeOffset.MaxValue, inserted?.BusinessEnd),
            () => Assert.Equal(now, inserted?.SystemStart),
            () => Assert.Equal(DateTimeOffset.MaxValue, inserted?.SystemEnd)
        );

        _timeProviderMock.Verify(p => p.GetUtcNow(), Times.Once());
    }
}
