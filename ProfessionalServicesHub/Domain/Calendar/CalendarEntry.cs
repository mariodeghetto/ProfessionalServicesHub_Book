using ProfessionalServicesHub.Domain.Clients;
using ProfessionalServicesHub.Domain.Work;

namespace ProfessionalServicesHub.Domain.Calendar;

public sealed class CalendarEntry
{
    public int Id { get; set; }

    public required string Subject { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAllDay { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public CalendarEntryKind Kind { get; set; } =
        CalendarEntryKind.Appointment;

    public int? ClientId { get; set; }

    public Client? Client { get; set; }

    public int? EngagementId { get; set; }

    public Engagement? Engagement { get; set; }

    public int? WorkActivityId { get; set; }

    public WorkActivity? WorkActivity { get; set; }

    public string? Assignee { get; set; }
}
