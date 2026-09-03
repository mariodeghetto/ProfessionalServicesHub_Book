using ProfessionalServicesHub.Application.Documents;

namespace ProfessionalServicesHub.Infrastructure.Documents;

public sealed class LocalDocumentStorage : IDocumentStorage
{
    private readonly string _root;
    private readonly string _rootPrefix;
    private readonly StringComparison _pathComparison;

    public LocalDocumentStorage(IWebHostEnvironment environment)
    {
        _root = Path.GetFullPath(
            Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "documents"));

        Directory.CreateDirectory(_root);

        _rootPrefix =
            _root.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;

        _pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var safeExtension = NormalizeExtension(extension);
        var storageKey = $"{Guid.NewGuid():N}{safeExtension}";
        var fullPath = ResolveStoragePath(storageKey);

        await using var target = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            options:
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

        await content.CopyToAsync(target, cancellationToken);
        return storageKey;
    }

    public Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = ResolveStoragePath(storageKey);

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options:
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = ResolveStoragePath(storageKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveStoragePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) ||
            !string.Equals(
                Path.GetFileName(storageKey),
                storageKey,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The document storage key is not valid.");
        }

        var fullPath = Path.GetFullPath(
            Path.Combine(_root, storageKey));

        if (!fullPath.StartsWith(
                _rootPrefix,
                _pathComparison))
        {
            throw new InvalidOperationException(
                "The document storage key resolves outside the storage root.");
        }

        return fullPath;
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension) ||
            !extension.StartsWith(".", StringComparison.Ordinal) ||
            extension.Length > 10 ||
            extension.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0 ||
            extension.Contains(Path.DirectorySeparatorChar) ||
            extension.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new InvalidOperationException(
                "The document extension is not valid.");
        }

        return extension.ToLowerInvariant();
    }
}
