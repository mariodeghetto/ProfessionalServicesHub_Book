using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProfessionalServicesHub.Infrastructure.Data;
using ProfessionalServicesHub.Infrastructure.Health;
using ProfessionalServicesHub.Tests.Infrastructure;

namespace ProfessionalServicesHub.Tests.Health;

public sealed class DatabaseHealthCheckTests
{
    [Fact]
    public async Task Ready_database_reports_healthy()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var check =
            new DatabaseHealthCheck(
                database.Factory);

        var result = await check.CheckHealthAsync(
            new HealthCheckContext(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            HealthStatus.Healthy,
            result.Status);
    }

    [Fact]
    public async Task Factory_failure_reports_unhealthy()
    {
        var check =
            new DatabaseHealthCheck(
                new ThrowingDbContextFactory());

        var result = await check.CheckHealthAsync(
            new HealthCheckContext(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            HealthStatus.Unhealthy,
            result.Status);
    }

    private sealed class ThrowingDbContextFactory
        : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() =>
            throw new InvalidOperationException(
                "Test factory failure.");

        public Task<ApplicationDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromException<ApplicationDbContext>(
                new InvalidOperationException(
                    "Test factory failure."));
    }
}
