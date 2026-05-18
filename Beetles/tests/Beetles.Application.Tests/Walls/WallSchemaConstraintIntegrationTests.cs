using Beetles.Application.Tests.Database;
using Beetles.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

using static Beetles.Application.Tests.Walls.WallTestHelpers;

namespace Beetles.Application.Tests.Walls;

/// <summary>
/// Integration coverage for the migrated PostgreSQL schema. These tests bypass the repository
/// only to verify database defaults, nullability, exclusion constraints, and transaction rollback.
/// </summary>
[Collection("Database")]
public sealed class WallSchemaConstraintIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public WallSchemaConstraintIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SCH01_RawInsert_UsesInfinityDefaults_ForBusinessEndAndSystemEnd()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO walls (color, business_start)
            VALUES ('red', TIMESTAMPTZ '2025-05-01T00:00:00Z')
            RETURNING id, business_end, system_end;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());

        Assert.True(reader.GetInt32(0) > 0);
        Assert.Equal(Infinity, reader.GetFieldValue<DateTimeOffset>(1));
        Assert.Equal(Infinity, reader.GetFieldValue<DateTimeOffset>(2));
    }

    [Fact]
    public async Task SCH02_RawInsert_NullColor_IsRejectedByDatabase()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO walls (color, business_start)
            VALUES (NULL, TIMESTAMPTZ '2025-05-01T00:00:00Z');
            """;

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            command.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.NotNullViolation, exception.SqlState);
    }

    [Fact]
    public async Task SCH03_RawInsert_OverlappingBusinessAndSystemRange_ForSameId_IsRejected()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using (var first = connection.CreateCommand())
        {
            first.CommandText = """
                INSERT INTO walls (id, color, business_start, system_start)
                VALUES (77, 'red', TIMESTAMPTZ '2025-05-01T00:00:00Z', TIMESTAMPTZ '2025-05-10T00:00:00Z');
                """;
            await first.ExecuteNonQueryAsync();
        }

        await using var second = connection.CreateCommand();
        second.CommandText = """
            INSERT INTO walls (id, color, business_start, system_start)
            VALUES (77, 'blue', TIMESTAMPTZ '2025-05-03T00:00:00Z', TIMESTAMPTZ '2025-05-10T00:00:00Z');
            """;

        var exception = await Assert.ThrowsAsync<PostgresException>(() =>
            second.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.ExclusionViolation, exception.SqlState);
    }

    [Fact]
    public async Task SCH04_RawTransaction_FailedConstraint_RollsBackAllRows()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var first = connection.CreateCommand())
        {
            first.Transaction = transaction;
            first.CommandText = """
                INSERT INTO walls (id, color, business_start, system_start)
                VALUES (88, 'red', TIMESTAMPTZ '2025-05-01T00:00:00Z', TIMESTAMPTZ '2025-05-10T00:00:00Z');
                """;
            await first.ExecuteNonQueryAsync();
        }

        await using (var second = connection.CreateCommand())
        {
            second.Transaction = transaction;
            second.CommandText = """
                INSERT INTO walls (id, color, business_start, system_start)
                VALUES (88, 'blue', TIMESTAMPTZ '2025-05-03T00:00:00Z', TIMESTAMPTZ '2025-05-10T00:00:00Z');
                """;

            await Assert.ThrowsAsync<PostgresException>(() => second.ExecuteNonQueryAsync());
        }

        await transaction.RollbackAsync();

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var rows = await context.Walls.AsNoTracking().Where(w => w.Id == 88).ToListAsync();

        Assert.Empty(rows);
    }

    [Fact]
    public async Task SCH05_RawInsert_SameIdAdjacentBusinessRanges_IsAccepted()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO walls (id, color, business_start, business_end, system_start)
            VALUES
                (99, 'red', TIMESTAMPTZ '2025-05-01T00:00:00Z', TIMESTAMPTZ '2025-05-10T00:00:00Z', TIMESTAMPTZ '2025-05-10T00:00:00Z'),
                (99, 'blue', TIMESTAMPTZ '2025-05-10T00:00:00Z', 'infinity'::timestamptz, TIMESTAMPTZ '2025-05-10T00:00:00Z');
            """;

        var inserted = await command.ExecuteNonQueryAsync();

        Assert.Equal(2, inserted);
    }
}
