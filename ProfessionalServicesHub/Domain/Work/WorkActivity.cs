namespace ProfessionalServicesHub.Domain.Work;

public sealed class WorkActivity
{
    public int Id { get; set; }

    public int EngagementId { get; set; }

    public Engagement Engagement { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Assignee { get; set; }

    public DateTime? DueDate { get; set; }

    public ActivityStatus Status { get; set; } = ActivityStatus.Planned;

    public ActivityPriority Priority { get; set; } = ActivityPriority.Normal;

    public int Rank { get; set; }
}
