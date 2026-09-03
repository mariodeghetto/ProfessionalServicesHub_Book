using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Tests.Integration;

public sealed class TestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databasePath =
        Path.Combine(
            Path.GetTempPath(),
            $"psh-web-{Guid.NewGuid():N}.db");

    private readonly string _connectionString;

    public TestWebApplicationFactory()
    {
        _connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource = _databasePath,
                Pooling = false
            }
            .ToString();

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

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:ProfessionalServicesHub"] =
                            _connectionString
                    });
            });
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
