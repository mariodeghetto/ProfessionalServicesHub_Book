using ProfessionalServicesHub.Domain.Clients;
using ProfessionalServicesHub.Domain.Work;

namespace ProfessionalServicesHub.Domain.Documents;

public sealed class BusinessDocument
{
    public int Id { get; set; }
    public required string OriginalFileName { get; set; }
    public required string StorageKey { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
    public required string Sha256 { get; set; }
    public DocumentCategory Category { get; set; } = DocumentCategory.General;
    public string? Description { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string? UploadedBy { get; set; }
    public bool IsArchived { get; set; }
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public int? EngagementId { get; set; }
    public Engagement? Engagement { get; set; }
    public int? WorkActivityId { get; set; }
    public WorkActivity? WorkActivity { get; set; }
}
