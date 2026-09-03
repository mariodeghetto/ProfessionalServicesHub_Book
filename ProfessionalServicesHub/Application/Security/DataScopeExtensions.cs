using ProfessionalServicesHub.Domain.Calendar;
using ProfessionalServicesHub.Domain.Documents;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Security;

public static class DataScopeExtensions
{
    public static IQueryable<CalendarEntry> VisibleTo(
        this IQueryable<CalendarEntry> query,
        ApplicationDbContext db,
        CurrentUser user)
    {
        if (EngagementScope.HasGlobalOperationalScope(user))
        {
            return query;
        }

        return query.Where(entry =>
            entry.EngagementId != null &&
            db.EngagementAssignments.Any(assignment =>
                assignment.EngagementId == entry.EngagementId &&
                assignment.UserId == user.Id));
    }

    public static IQueryable<BusinessDocument> VisibleTo(
        this IQueryable<BusinessDocument> query,
        ApplicationDbContext db,
        CurrentUser user)
    {
        if (EngagementScope.HasGlobalOperationalScope(user))
        {
            return query;
        }

        return query.Where(document =>
            document.EngagementId != null &&
            db.EngagementAssignments.Any(assignment =>
                assignment.EngagementId == document.EngagementId &&
                assignment.UserId == user.Id));
    }
}
