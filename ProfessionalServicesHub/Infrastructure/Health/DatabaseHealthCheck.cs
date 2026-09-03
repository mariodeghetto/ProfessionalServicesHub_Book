using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Infrastructure.Health;

public sealed class DatabaseHealthCheck(
    IDbContextFactory<ApplicationDbContext> dbFactory)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db =
                await dbFactory.CreateDbContextAsync(cancellationToken);

            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(
                    "Database unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Database unavailable.",
                exception);
        }
    }
}
