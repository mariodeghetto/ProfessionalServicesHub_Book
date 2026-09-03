using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Tests.Migrations;

public sealed class MigrationTests
{
    [Fact]
    public async Task Empty_database_migrates_to_latest_schema()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync(
            TestContext.Current.CancellationToken);

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
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
}
