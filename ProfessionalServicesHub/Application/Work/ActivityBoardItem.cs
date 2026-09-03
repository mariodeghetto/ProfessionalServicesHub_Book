using ProfessionalServicesHub.Domain.Work;

namespace ProfessionalServicesHub.Application.Work;

public sealed class ActivityBoardItem
{
    public int Id { get; init; }

    public string EngagementCode { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string Assignee { get; init; } = "Unassigned";

    public DateTime? DueDate { get; init; }

    public string StatusKey { get; set; } = string.Empty;

    public string Priority { get; init; } = string.Empty;

    public int Rank { get; set; }

    public bool IsOverdue =>
        DueDate.HasValue &&
        DueDate.Value.Date < DateTime.Today &&
        StatusKey != nameof(ActivityStatus.Completed);
}
