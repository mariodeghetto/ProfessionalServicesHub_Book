namespace ProfessionalServicesHub.Application.Documents;

public interface IDocumentStorage
{
    Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
