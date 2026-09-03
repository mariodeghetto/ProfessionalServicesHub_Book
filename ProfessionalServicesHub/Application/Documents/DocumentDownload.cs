namespace ProfessionalServicesHub.Application.Documents;

public sealed record DocumentDownload(
    string FileName,
    string ContentType,
    Stream Content);
