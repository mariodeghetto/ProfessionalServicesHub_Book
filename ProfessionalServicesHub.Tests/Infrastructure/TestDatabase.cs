using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Tests.Infrastructure;

public sealed class TestDatabase : IAsyncDisposable
{
    private TestDatabase(
        SqliteConnection connection,
        DbContextOptions<ApplicationDbContext> options)
    {
        Connection = connection;
        Options = options;
        Factory = new TestDbContextFactory(options);
    }

    public SqliteConnection Connection { get; }

    public DbContextOptions<ApplicationDbContext> Options { get; }

    public IDbContextFactory<ApplicationDbContext> Factory { get; }

    public static async Task<TestDatabase> CreateAsync()
    {
        var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var db =
            new ApplicationDbContext(options);

        await db.Database.EnsureCreatedAsync();

        return new TestDatabase(
            connection,
            options);
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}

public sealed class TestDbContextFactory(
    DbContextOptions<ApplicationDbContext> options)
    : IDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext() =>
        new(options);

    public Task<ApplicationDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateDbContext());
}
