using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Domain.Documents;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Documents;

public sealed class DocumentService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IDocumentStorage storage,
    ICurrentUserAccessor currentUserAccessor)
{
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;
    public const string AllowedExtensionsCsv =
        ".pdf,.docx,.xlsx,.png,.jpg,.jpeg";

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg"
        };

    private static readonly IReadOnlyDictionary<string, string>
        CanonicalContentTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".docx"] =
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".xlsx"] =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg"
            };

    public static bool IsAllowedFileName(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        return !string.IsNullOrWhiteSpace(safeFileName) &&
            AllowedExtensions.Contains(Path.GetExtension(safeFileName));
    }

    public static bool IsAllowedFileSize(long sizeBytes) =>
        sizeBytes > 0 && sizeBytes <= MaxFileSizeBytes;

    public async Task<List<EngagementDocumentOption>> GetEngagementOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var items = await db.Engagements
            .AsNoTracking()
            .VisibleTo(db, user)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Title,
                ClientName = x.Client.Name
            })
            .ToListAsync(cancellationToken);

        return items
            .Select(x => new EngagementDocumentOption(
                x.Id,
                $"{x.Code} - {x.Title} ({x.ClientName})"))
            .ToList();
    }

    public async Task<List<DocumentListItem>> GetDocumentsAsync(
        int? engagementId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var query = db.Documents
            .AsNoTracking()
            .VisibleTo(db, user)
            .Where(x => !x.IsArchived);

        if (engagementId is int selectedEngagementId)
        {
            query = query.Where(
                x => x.EngagementId == selectedEngagementId);
        }

        return await query
            .OrderByDescending(x => x.UploadedAtUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => new DocumentListItem(
                x.Id,
                x.OriginalFileName,
                x.Category,
                x.SizeBytes,
                x.UploadedAtUtc,
                x.UploadedBy,
                x.ContentType,
                x.Client != null ? x.Client.Name : null,
                x.Engagement != null ? x.Engagement.Code : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> UploadAsync(
        Stream content,
        string originalFileName,
        string? declaredContentType,
        long declaredSizeBytes,
        int? engagementId,
        DocumentCategory category,
        string? description,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var safeFileName = Path.GetFileName(originalFileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            throw new InvalidOperationException(
                "The original file name is not valid.");
        }

        var extension = Path.GetExtension(safeFileName);
        if (!IsAllowedFileName(safeFileName))
        {
            throw new InvalidOperationException(
                "This file type is not allowed.");
        }

        if (!IsAllowedFileSize(declaredSizeBytes))
        {
            throw new InvalidOperationException(
                "The file size is not valid.");
        }

        var normalizedDescription =
            string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();

        if (normalizedDescription?.Length > 1000)
        {
            throw new InvalidOperationException(
                "The description cannot exceed 1,000 characters.");
        }

        using var buffered = new MemoryStream();
        await content.CopyToAsync(buffered, cancellationToken);

        if (buffered.Length <= 0 ||
            buffered.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                "The uploaded content size is not valid.");
        }

        var bytes = buffered.ToArray();
        if (!HasExpectedSignature(extension, bytes))
        {
            throw new InvalidOperationException(
                "The file content does not match the selected file type.");
        }

        var sha256 = Convert.ToHexString(SHA256.HashData(bytes));
        var contentType = ResolveContentType(
            extension,
            declaredContentType);

        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        int? clientId = null;

        if (engagementId is int selectedEngagementId)
        {
            var engagement = await db.Engagements
                .AsNoTracking()
                .VisibleTo(db, user)
                .Where(x => x.Id == selectedEngagementId)
                .Select(x => new
                {
                    x.Id,
                    x.ClientId
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (engagement is null)
            {
                throw new InvalidOperationException(
                    "The selected engagement is not available.");
            }

            if (!EngagementScope.HasGlobalOperationalScope(user))
            {
                var canEdit = await db.EngagementAssignments
                    .AsNoTracking()
                    .AnyAsync(
                        assignment =>
                            assignment.EngagementId ==
                                selectedEngagementId &&
                            assignment.UserId == user.Id &&
                            assignment.Kind !=
                                Domain.Work.AssignmentKind.Observer,
                        cancellationToken);

                if (!canEdit)
                {
                    throw new InvalidOperationException(
                        "You do not have permission to add documents to this engagement.");
                }
            }

            clientId = engagement.ClientId;
        }
        else if (!EngagementScope.HasGlobalOperationalScope(user))
        {
            throw new InvalidOperationException(
                "You do not have permission to add general documents.");
        }

        buffered.Position = 0;
        var storageKey = await storage.SaveAsync(
            buffered,
            extension,
            cancellationToken);

        try
        {
            var document = new BusinessDocument
            {
                OriginalFileName = safeFileName,
                StorageKey = storageKey,
                ContentType = contentType,
                SizeBytes = buffered.Length,
                Sha256 = sha256,
                Category = category,
                Description = normalizedDescription,
                UploadedAtUtc = DateTime.UtcNow,
                ClientId = clientId,
                EngagementId = engagementId
            };

            db.Documents.Add(document);
            await db.SaveChangesAsync(cancellationToken);
            return document.Id;
        }
        catch
        {
            await storage.DeleteAsync(
                storageKey,
                CancellationToken.None);
            throw;
        }
    }

    public async Task<Stream> OpenAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var storageKey = await db.Documents
            .AsNoTracking()
            .VisibleTo(db, user)
            .Where(x => x.Id == documentId && !x.IsArchived)
            .Select(x => x.StorageKey)
            .SingleOrDefaultAsync(cancellationToken);

        if (storageKey is null)
        {
            throw new KeyNotFoundException(
                "The requested document is not available.");
        }

        return await storage.OpenReadAsync(
            storageKey,
            cancellationToken);
    }

    public async Task<DocumentDownload> GetDownloadAsync(
        int documentId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var user = CurrentUser.FromPrincipal(principal);

        return await GetDownloadForUserAsync(
            documentId,
            user,
            cancellationToken);
    }

    private async Task<DocumentDownload> GetDownloadForUserAsync(
        int documentId,
        CurrentUser user,
        CancellationToken cancellationToken)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var document = await db.Documents
            .AsNoTracking()
            .VisibleTo(db, user)
            .Where(x => x.Id == documentId && !x.IsArchived)
            .Select(x => new
            {
                x.OriginalFileName,
                x.ContentType,
                x.StorageKey
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException(
                "The requested document is not available.");
        }

        var content = await storage.OpenReadAsync(
            document.StorageKey,
            cancellationToken);

        return new DocumentDownload(
            document.OriginalFileName,
            document.ContentType,
            content);
    }

    public async Task<bool> ArchiveAsync(
        int documentId,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var document = await db.Documents
            .AsNoTracking()
            .VisibleTo(db, user)
            .Where(x => x.Id == documentId && !x.IsArchived)
            .Select(x => new
            {
                x.Id,
                x.EngagementId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            return false;
        }

        if (!EngagementScope.HasGlobalOperationalScope(user))
        {
            if (document.EngagementId is not int engagementId)
            {
                return false;
            }

            var canEdit = await db.EngagementAssignments
                .AsNoTracking()
                .AnyAsync(
                    assignment =>
                        assignment.EngagementId == engagementId &&
                        assignment.UserId == user.Id &&
                        assignment.Kind !=
                            Domain.Work.AssignmentKind.Observer,
                    cancellationToken);

            if (!canEdit)
            {
                throw new UnauthorizedAccessException(
                    "You do not have permission to archive this document.");
            }
        }

        var affected = await db.Documents
            .Where(x => x.Id == documentId && !x.IsArchived)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    x => x.IsArchived,
                    true),
                cancellationToken);

        return affected == 1;
    }

    private static string ResolveContentType(
        string extension,
        string? declaredContentType)
    {
        if (CanonicalContentTypes.TryGetValue(
                extension,
                out var canonicalContentType))
        {
            return canonicalContentType;
        }

        return string.IsNullOrWhiteSpace(declaredContentType)
            ? "application/octet-stream"
            : declaredContentType.Trim();
    }

    private static bool HasExpectedSignature(
        string extension,
        byte[] bytes)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => StartsWith(bytes, "%PDF-"u8),
            ".png" => StartsWith(
                bytes,
                new byte[]
                {
                    0x89, 0x50, 0x4E, 0x47,
                    0x0D, 0x0A, 0x1A, 0x0A
                }),
            ".jpg" or ".jpeg" => StartsWith(
                bytes,
                new byte[] { 0xFF, 0xD8, 0xFF }),
            ".docx" => IsExpectedOfficePackage(bytes, "word/"),
            ".xlsx" => IsExpectedOfficePackage(bytes, "xl/"),
            _ => false
        };
    }

    private static bool StartsWith(
        byte[] bytes,
        ReadOnlySpan<byte> signature)
    {
        return bytes.AsSpan().StartsWith(signature);
    }

    private static bool IsExpectedOfficePackage(
        byte[] bytes,
        string contentRoot)
    {
        try
        {
            using var stream =
                new MemoryStream(bytes, writable: false);
            using var archive =
                new ZipArchive(
                    stream,
                    ZipArchiveMode.Read,
                    leaveOpen: false);

            return archive.GetEntry("[Content_Types].xml") is not null &&
                archive.Entries.Any(
                    x => x.FullName.StartsWith(
                        contentRoot,
                        StringComparison.OrdinalIgnoreCase));
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
}
