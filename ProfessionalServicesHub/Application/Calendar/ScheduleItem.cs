using ProfessionalServicesHub.Domain.Calendar;

namespace ProfessionalServicesHub.Application.Calendar;

public sealed class ScheduleItem
{
    public int Id { get; set; }

    public string Subject { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAllDay { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public CalendarEntryKind Kind { get; set; } =
        CalendarEntryKind.Appointment;

    public int? ClientId { get; set; }

    public string? ClientName { get; set; }

    public int? EngagementId { get; set; }

    public string? EngagementCode { get; set; }

    public int? WorkActivityId { get; set; }

    public string? Assignee { get; set; }
}
