using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProfessionalServicesHub.Infrastructure.Data;

public sealed class ApplicationDbContextDesignFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var projectDirectory = ResolveProjectDirectory();

        var databasePath = Path.Combine(
            projectDirectory,
            "Data",
            "professionalserviceshub.db");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string ResolveProjectDirectory()
    {
        var current = Directory.GetCurrentDirectory();

        if (File.Exists(Path.Combine(
                current,
                "ProfessionalServicesHub.csproj")))
        {
            return current;
        }

        var child = Path.Combine(
            current,
            "ProfessionalServicesHub");

        if (File.Exists(Path.Combine(
                child,
                "ProfessionalServicesHub.csproj")))
        {
            return child;
        }

        throw new InvalidOperationException(
            "Unable to locate the ProfessionalServicesHub project directory.");
    }
}
