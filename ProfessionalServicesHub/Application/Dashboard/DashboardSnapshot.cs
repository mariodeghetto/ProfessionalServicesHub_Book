namespace ProfessionalServicesHub.Application.Dashboard;

public sealed record DashboardSnapshot(
    int EngagementsWithOpenActivities,
    int OpenActivities,
    int OverdueActivities,
    int DeadlinesNextSevenDays,
    int DocumentsLastThirtyDays,
    IReadOnlyList<StatusCount> ActivitiesByStatus,
    IReadOnlyList<DeadlineTrendPoint> DeadlinesByDay,
    IReadOnlyList<AssigneeLoadPoint> ActivitiesByAssignee);

public sealed record StatusCount(
    string Status,
    int Count);

public sealed record DeadlineTrendPoint(
    DateTime Day,
    int Count);

public sealed record AssigneeLoadPoint(
    string Assignee,
    int Count);
