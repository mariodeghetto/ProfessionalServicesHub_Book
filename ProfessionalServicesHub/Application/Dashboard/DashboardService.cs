using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Domain.Calendar;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Dashboard;

public sealed class DashboardService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<DashboardSnapshot> LoadAsync(
        DateTime nowLocal,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var today = nowLocal.Date;
        var sevenDayEnd = today.AddDays(7);
        var trendEnd = today.AddDays(14);
        var thirtyDaysAgoUtc = utcNow.AddDays(-30);

        var visibleEngagements = db.Engagements
            .AsNoTracking()
            .VisibleTo(db, user);

        var visibleEngagementIds =
            visibleEngagements.Select(engagement => engagement.Id);

        var visibleActivities = db.WorkActivities
            .AsNoTracking()
            .Where(activity =>
                visibleEngagementIds.Contains(activity.EngagementId));

        var visibleCalendarEntries = db.CalendarEntries
            .AsNoTracking()
            .VisibleTo(db, user);

        var visibleDocuments = db.Documents
            .AsNoTracking()
            .VisibleTo(db, user);

        var engagementsWithOpenActivities =
            await visibleEngagements.CountAsync(
                engagement => engagement.Activities.Any(
                    activity =>
                        activity.Status != ActivityStatus.Completed),
                cancellationToken);

        var openActivities =
            await visibleActivities.CountAsync(
                activity =>
                    activity.Status != ActivityStatus.Completed,
                cancellationToken);

        var overdueActivities =
            await visibleActivities.CountAsync(
                activity =>
                    activity.Status != ActivityStatus.Completed &&
                    activity.DueDate != null &&
                    activity.DueDate < today,
                cancellationToken);

        var deadlinesNextSevenDays =
            await visibleCalendarEntries.CountAsync(
                entry =>
                    entry.Kind == CalendarEntryKind.Deadline &&
                    entry.StartTime >= today &&
                    entry.StartTime < sevenDayEnd,
                cancellationToken);

        var documentsLastThirtyDays =
            await visibleDocuments.CountAsync(
                document =>
                    !document.IsArchived &&
                    document.UploadedAtUtc >= thirtyDaysAgoUtc,
                cancellationToken);

        var rawStatusCounts =
            await visibleActivities
                .GroupBy(activity => activity.Status)
                .Select(group => new
                {
                    Status = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);

        var countByStatus =
            rawStatusCounts.ToDictionary(
                item => item.Status,
                item => item.Count);

        var activitiesByStatus = new List<StatusCount>
        {
            new(
                "Planned",
                countByStatus.GetValueOrDefault(
                    ActivityStatus.Planned)),
            new(
                "In progress",
                countByStatus.GetValueOrDefault(
                    ActivityStatus.InProgress)),
            new(
                "Waiting",
                countByStatus.GetValueOrDefault(
                    ActivityStatus.Waiting)),
            new(
                "Completed",
                countByStatus.GetValueOrDefault(
                    ActivityStatus.Completed))
        };

        var deadlineDates =
            await visibleCalendarEntries
                .Where(entry =>
                    entry.Kind == CalendarEntryKind.Deadline &&
                    entry.StartTime >= today &&
                    entry.StartTime < trendEnd)
                .Select(entry => entry.StartTime)
                .ToListAsync(cancellationToken);

        var deadlineCountByDay =
            deadlineDates
                .GroupBy(value => value.Date)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count());

        var deadlinesByDay =
            Enumerable.Range(0, 14)
                .Select(offset => today.AddDays(offset))
                .Select(
                    day => new DeadlineTrendPoint(
                        day,
                        deadlineCountByDay.GetValueOrDefault(day)))
                .ToList();

        var rawAssigneeLoad =
            await visibleActivities
                .Where(activity =>
                    activity.Status != ActivityStatus.Completed)
                .GroupBy(
                    activity =>
                        activity.Assignee ?? "Unassigned")
                .Select(group => new
                {
                    Assignee = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);

        var activitiesByAssignee =
            rawAssigneeLoad
                .Select(item => new AssigneeLoadPoint(
                    string.IsNullOrWhiteSpace(item.Assignee)
                        ? "Unassigned"
                        : item.Assignee,
                    item.Count))
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.Assignee)
                .ToList();

        return new DashboardSnapshot(
            engagementsWithOpenActivities,
            openActivities,
            overdueActivities,
            deadlinesNextSevenDays,
            documentsLastThirtyDays,
            activitiesByStatus,
            deadlinesByDay,
            activitiesByAssignee);
    }
}
