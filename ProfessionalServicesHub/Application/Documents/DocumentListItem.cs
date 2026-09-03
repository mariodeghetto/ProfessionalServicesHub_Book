using ProfessionalServicesHub.Domain.Documents;

namespace ProfessionalServicesHub.Application.Documents;

public sealed record DocumentListItem(
    int Id,
    string FileName,
    DocumentCategory Category,
    long SizeBytes,
    DateTimeOffset UploadedAtUtc,
    string? UploadedBy,
    string ContentType,
    string? ClientName,
    string? EngagementCode)
{
    public bool IsPdf =>
        ContentType.Equals(
            "application/pdf",
            StringComparison.OrdinalIgnoreCase);

    public string SizeDisplay => SizeBytes switch
    {
        < 1024 => $"{SizeBytes:N0} B",
        < 1024 * 1024 => $"{SizeBytes / 1024d:N1} KB",
        _ => $"{SizeBytes / (1024d * 1024d):N1} MB"
    };
}
