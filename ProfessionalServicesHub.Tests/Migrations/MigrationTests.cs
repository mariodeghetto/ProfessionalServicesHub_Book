using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Tests.Migrations;

public sealed class MigrationTests
{
    [Fact]
    public async Task Empty_database_migrates_to_latest_schema()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"psh-book-{Guid.NewGuid():N}.db");

        try
        {
            var options =
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(
                        $"Data Source={databasePath}")
                    .Options;

            await using var db =
                new ApplicationDbContext(options);

            await db.Database.MigrateAsync(
                TestContext.Current.CancellationToken);

            var applied =
                await db.Database.GetAppliedMigrationsAsync(
                    TestContext.Current.CancellationToken);

            Assert.Contains(
                applied,
                migration =>
                    migration.EndsWith(
                        "AddIdentityAndAccessScope",
                        StringComparison.Ordinal));
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
