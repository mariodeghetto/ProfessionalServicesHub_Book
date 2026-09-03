using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Work;

public sealed record EngagementListItem(
    int Id,
    string Code,
    string Title,
    string ClientName);

public sealed class EngagementQueryService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<List<EngagementListItem>> GetVisibleAsync(
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Engagements
            .AsNoTracking()
            .VisibleTo(db, user)
            .OrderBy(engagement => engagement.Code)
            .Select(engagement => new EngagementListItem(
                engagement.Id,
                engagement.Code,
                engagement.Title,
                engagement.Client.Name))
            .ToListAsync(cancellationToken);
    }
}
