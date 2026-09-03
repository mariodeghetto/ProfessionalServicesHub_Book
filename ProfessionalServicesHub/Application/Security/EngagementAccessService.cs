using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Security;

public sealed class EngagementAccessService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<bool> CanReadAsync(
        int engagementId,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        if (EngagementScope.HasGlobalOperationalScope(user))
        {
            return true;
        }

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.EngagementAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.EngagementId == engagementId &&
                    assignment.UserId == user.Id,
                cancellationToken);
    }

    public async Task<bool> CanEditAsync(
        int engagementId,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        if (EngagementScope.HasGlobalOperationalScope(user))
        {
            return true;
        }

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.EngagementAssignments
            .AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.EngagementId == engagementId &&
                    assignment.UserId == user.Id &&
                    assignment.Kind != AssignmentKind.Observer,
                cancellationToken);
    }
}
