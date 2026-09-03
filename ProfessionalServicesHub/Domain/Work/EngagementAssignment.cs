namespace ProfessionalServicesHub.Domain.Work;

public enum AssignmentKind
{
    Responsible = 1,
    Collaborator = 2,
    Observer = 3
}

public sealed class EngagementAssignment
{
    public int EngagementId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public AssignmentKind Kind { get; set; }
}
