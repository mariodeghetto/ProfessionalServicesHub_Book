using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Documents;
using ProfessionalServicesHub.Domain.Documents;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Tests.Infrastructure;
using ProfessionalServicesHub.Tests.Security;

namespace ProfessionalServicesHub.Tests.Documents;

public sealed class DocumentScopeTests
{
    [Fact]
    public async Task Out_of_scope_download_never_opens_storage()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var seeded =
            await TestData.SeedTwoEngagementsAsync(
                database.Options);

        var storage = new TrackingDocumentStorage();

        var service = new DocumentService(
            database.Factory,
            storage,
            new TestCurrentUserAccessor(
                TestUsers.Collaborator()));

        var principal = CreateCollaboratorPrincipal(
            "user-1");

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetDownloadAsync(
                seeded.SecondDocumentId,
                principal,
                TestContext.Current.CancellationToken));

        Assert.Equal(
            0,
            storage.OpenReadCalls);
    }

    [Fact]
    public async Task In_scope_download_opens_storage()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var seeded =
            await TestData.SeedTwoEngagementsAsync(
                database.Options);

        var storage = new TrackingDocumentStorage();

        var service = new DocumentService(
            database.Factory,
            storage,
            new TestCurrentUserAccessor(
                TestUsers.Collaborator()));

        var principal = CreateCollaboratorPrincipal(
            "user-1");

        var result = await service.GetDownloadAsync(
            seeded.FirstDocumentId,
            principal,
            TestContext.Current.CancellationToken);

        await using var content = result.Content;

        Assert.Equal(
            "scoped.pdf",
            result.FileName);

        Assert.Equal(
            "application/pdf",
            result.ContentType);

        Assert.Equal(
            1,
            storage.OpenReadCalls);
    }

    [Fact]
    public async Task Upload_records_current_user_as_uploader()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var storage = new TrackingDocumentStorage();

        var service = new DocumentService(
            database.Factory,
            storage,
            new TestCurrentUserAccessor(
                TestUsers.Administrator()));

        await using var content =
            new MemoryStream("%PDF-1.7"u8.ToArray());

        var documentId = await service.UploadAsync(
            content,
            "review.pdf",
            "application/pdf",
            content.Length,
            engagementId: null,
            DocumentCategory.Report,
            description: null,
            TestContext.Current.CancellationToken);

        await using var db =
            await database.Factory.CreateDbContextAsync(
                TestContext.Current.CancellationToken);

        var uploadedBy = await db.Documents
            .Where(document => document.Id == documentId)
            .Select(document => document.UploadedBy)
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            "Test Administrator",
            uploadedBy);
    }

    private static ClaimsPrincipal CreateCollaboratorPrincipal(
        string userId)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId),
                new Claim(
                    ClaimTypes.Name,
                    "Test Collaborator"),
                new Claim(
                    ClaimTypes.Role,
                    AppRoles.Collaborator)
            ],
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }

    private sealed class TrackingDocumentStorage
        : IDocumentStorage
    {
        public int OpenReadCalls { get; private set; }

        public Task<string> SaveAsync(
            Stream content,
            string extension,
            CancellationToken cancellationToken = default) =>
            Task.FromResult("unused.pdf");

        public Task<Stream> OpenReadAsync(
            string storageKey,
            CancellationToken cancellationToken = default)
        {
            OpenReadCalls++;

            Stream stream =
                new MemoryStream(
                    "%PDF-test"u8.ToArray());

            return Task.FromResult(stream);
        }

        public Task DeleteAsync(
            string storageKey,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
