using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ProfessionalServicesHub.Infrastructure.Data;

public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var workingDirectory = Directory.GetCurrentDirectory();

        var projectDirectory =
            File.Exists(Path.Combine(workingDirectory, "ProfessionalServicesHub.csproj"))
                ? workingDirectory
                : Path.Combine(workingDirectory, "ProfessionalServicesHub");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration
            .GetConnectionString("ProfessionalServicesHub")
            ?? throw new InvalidOperationException(
                "The ProfessionalServicesHub connection string is not configured.");

        var connectionStringBuilder =
            new SqliteConnectionStringBuilder(connectionString);

        if (!Path.IsPathRooted(connectionStringBuilder.DataSource))
        {
            connectionStringBuilder.DataSource =
                Path.Combine(projectDirectory, connectionStringBuilder.DataSource);
        }

        var databaseDirectory =
            Path.GetDirectoryName(connectionStringBuilder.DataSource);

        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlite(connectionStringBuilder.ToString());

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
