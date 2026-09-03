using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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

    public TestWebApplicationFactory()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(
                    $"Data Source={_databasePath}")
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
                            $"Data Source={_databasePath}",
                        ["Syncfusion:LicenseKey"] =
                            "TEST-LICENSE-KEY"
                    });
            });
    }

    public void DeleteDatabase()
    {
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
