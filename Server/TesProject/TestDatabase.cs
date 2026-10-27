using DatabaseConnection.DBContext;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TesProject;

public enum DbKind
{
    Sqlite,
    Postgres
}

// A throw-away database for ONE test, so tests never share data.
//
//  Sqlite   - in memory, fast, always available (also on CI). Good for the service's logic.
//             It is not PostgreSQL, so it cannot catch PostgreSQL-specific problems
//             (for example the UTC rule for "timestamp with time zone").
//  Postgres - only used when the environment variable EXAMPLANNER_TEST_PG holds a connection string
//             to a PostgreSQL server where the tests may create and drop databases, for example:
//                 Host=localhost;Port=5432;Username=postgres;Password=...;Database=postgres
//             Each test gets its own database (examplanner_test_<random>), built with the project's
//             real migrations and dropped afterwards. Your own ExamPlanner database is never used.
public sealed class TestDatabase : IAsyncDisposable
{
    public const string PostgresVariable = "EXAMPLANNER_TEST_PG";

    private SqliteConnection? _sqlite;
    private string? _adminConnectionString;
    private string? _databaseName;

    public DbContextOptions<AppDbContext> Options { get; private set; } = null!;

    // Feeds [Theory] + [MemberData]: Sqlite always, Postgres only when it is configured.
    public static IEnumerable<object[]> Kinds()
    {
        yield return [DbKind.Sqlite];

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(PostgresVariable)))
            yield return [DbKind.Postgres];
    }

    public static async Task<TestDatabase> CreateAsync(DbKind kind)
    {
        var database = new TestDatabase();

        if (kind == DbKind.Sqlite)
            await database.StartSqliteAsync();
        else
            await database.StartPostgresAsync();

        return database;
    }

    // A new context on the same database. The service gets a fresh one per import,
    // like a real request does, so no change tracker state is shared between imports.
    public AppDbContext NewContext() => new(Options);

    private async Task StartSqliteAsync()
    {
        // An in-memory SQLite database lives only as long as its connection is open.
        _sqlite = new SqliteConnection("DataSource=:memory:");
        await _sqlite.OpenAsync();

        Options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_sqlite).Options;

        await using var context = NewContext();
        await context.Database.EnsureCreatedAsync();
    }

    private async Task StartPostgresAsync()
    {
        _adminConnectionString = Environment.GetEnvironmentVariable(PostgresVariable)!;
        _databaseName = "examplanner_test_" + Guid.NewGuid().ToString("N");

        await using (var admin = new NpgsqlConnection(_adminConnectionString))
        {
            await admin.OpenAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", admin);
            await create.ExecuteNonQueryAsync();
        }

        var connectionString = new NpgsqlConnectionStringBuilder(_adminConnectionString) { Database = _databaseName };
        Options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString.ConnectionString).Options;

        // The real migrations, so the test also proves the schema matches the model.
        await using var context = NewContext();
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_sqlite is not null)
            await _sqlite.DisposeAsync();

        if (_databaseName is not null)
        {
            NpgsqlConnection.ClearAllPools(); // otherwise the database is "in use" and cannot be dropped

            await using var admin = new NpgsqlConnection(_adminConnectionString);
            await admin.OpenAsync();
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }
}
