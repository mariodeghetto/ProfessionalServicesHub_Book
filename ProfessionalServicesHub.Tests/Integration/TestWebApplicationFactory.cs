using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Tests.Integration;

public sealed class TestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private const string ConnectionStringEnvironmentKey =
        "ConnectionStrings__ProfessionalServicesHub";

    private readonly string _databasePath =
        Path.Combine(
            Path.GetTempPath(),
            $"psh-web-{Guid.NewGuid():N}.db");

    private readonly string _connectionString;

    private readonly string? _previousConnectionString;

    public TestWebApplicationFactory()
    {
        _connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Pooling = false
            }
            .ToString();

        _previousConnectionString =
            Environment.GetEnvironmentVariable(
                ConnectionStringEnvironmentKey);

        Environment.SetEnvironmentVariable(
            ConnectionStringEnvironmentKey,
            _connectionString);

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connectionString)
                .Options;

        using var db =
            new ApplicationDbContext(options);

        db.Database.Migrate();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            if (disposing)
            {
                Environment.SetEnvironmentVariable(
                    ConnectionStringEnvironmentKey,
                    _previousConnectionString);
            }
        }
    }

    public void DeleteDatabase()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
