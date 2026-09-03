using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Security;

public static class EngagementScope
{
    public static IQueryable<Engagement> VisibleTo(
        this IQueryable<Engagement> query,
        ApplicationDbContext db,
        CurrentUser user)
    {
        if (HasGlobalOperationalScope(user))
        {
            return query;
        }

        return query.Where(engagement =>
            db.EngagementAssignments.Any(assignment =>
                assignment.EngagementId == engagement.Id &&
                assignment.UserId == user.Id));
    }

    public static IQueryable<WorkActivity> VisibleTo(
        this IQueryable<WorkActivity> query,
        ApplicationDbContext db,
        CurrentUser user)
    {
        if (HasGlobalOperationalScope(user))
        {
            return query;
        }

        return query.Where(activity =>
            db.EngagementAssignments.Any(assignment =>
                assignment.EngagementId == activity.EngagementId &&
                assignment.UserId == user.Id));
    }

    public static bool HasGlobalOperationalScope(
        CurrentUser user) =>
        user.IsInRole(AppRoles.Administrator) ||
        user.IsInRole(AppRoles.Coordinator);
}
