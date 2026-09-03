using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Domain.Calendar;
using ProfessionalServicesHub.Domain.Clients;
using ProfessionalServicesHub.Domain.Documents;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;
using ProfessionalServicesHub.Infrastructure.Identity;

namespace ProfessionalServicesHub.Tests.Infrastructure;

public sealed record SeededWorkData(
    int FirstClientId,
    int SecondClientId,
    int FirstEngagementId,
    int SecondEngagementId,
    int FirstDocumentId,
    int SecondDocumentId);

public static class TestData
{
    public static async Task<SeededWorkData> SeedTwoEngagementsAsync(
        DbContextOptions<ApplicationDbContext> options,
        string assignedUserId = "user-1",
        AssignmentKind assignmentKind =
            AssignmentKind.Collaborator)
    {
        await using var db =
            new ApplicationDbContext(options);

        var user = new ApplicationUser
        {
            Id = assignedUserId,
            UserName = "collaborator@example.test",
            NormalizedUserName =
                "COLLABORATOR@EXAMPLE.TEST",
            Email = "collaborator@example.test",
            NormalizedEmail =
                "COLLABORATOR@EXAMPLE.TEST",
            DisplayName = "Test Collaborator"
        };

        db.Users.Add(user);

        var firstClient = new Client
        {
            Code = "CLI-001",
            Name = "Alpine Design"
        };

        var secondClient = new Client
        {
            Code = "CLI-002",
            Name = "Blue Harbor Consulting"
        };

        db.Clients.AddRange(
            firstClient,
            secondClient);

        await db.SaveChangesAsync();

        var firstEngagement = new Engagement
        {
            ClientId = firstClient.Id,
            Code = "ENG-001",
            Title = "Accessibility review"
        };

        var secondEngagement = new Engagement
        {
            ClientId = secondClient.Id,
            Code = "ENG-002",
            Title = "Reporting modernization"
        };

        db.Engagements.AddRange(
            firstEngagement,
            secondEngagement);

        await db.SaveChangesAsync();

        db.EngagementAssignments.Add(
            new EngagementAssignment
            {
                EngagementId = firstEngagement.Id,
                UserId = assignedUserId,
                Kind = assignmentKind
            });

        db.WorkActivities.AddRange(
            new WorkActivity
            {
                EngagementId = firstEngagement.Id,
                Title = "Scoped activity",
                Status = ActivityStatus.InProgress,
                Priority = ActivityPriority.Normal,
                Rank = 10
            },
            new WorkActivity
            {
                EngagementId = secondEngagement.Id,
                Title = "Out-of-scope activity",
                Status = ActivityStatus.InProgress,
                Priority = ActivityPriority.High,
                Rank = 10
            });

        var now = DateTime.Today.AddHours(9);

        db.CalendarEntries.AddRange(
            new CalendarEntry
            {
                Subject = "Scoped meeting",
                StartTime = now,
                EndTime = now.AddHours(1),
                EngagementId = firstEngagement.Id,
                ClientId = firstClient.Id
            },
            new CalendarEntry
            {
                Subject = "Out-of-scope meeting",
                StartTime = now.AddHours(2),
                EndTime = now.AddHours(3),
                EngagementId = secondEngagement.Id,
                ClientId = secondClient.Id
            });

        var firstDocument = new BusinessDocument
        {
            OriginalFileName = "scoped.pdf",
            StorageKey = "scoped.pdf",
            ContentType = "application/pdf",
            SizeBytes = 8,
            Sha256 = new string('A', 64),
            Category = DocumentCategory.Report,
            UploadedAtUtc = DateTime.UtcNow,
            ClientId = firstClient.Id,
            EngagementId = firstEngagement.Id
        };

        var secondDocument = new BusinessDocument
        {
            OriginalFileName = "out-of-scope.pdf",
            StorageKey = "out-of-scope.pdf",
            ContentType = "application/pdf",
            SizeBytes = 8,
            Sha256 = new string('B', 64),
            Category = DocumentCategory.Report,
            UploadedAtUtc = DateTime.UtcNow,
            ClientId = secondClient.Id,
            EngagementId = secondEngagement.Id
        };

        db.Documents.AddRange(
            firstDocument,
            secondDocument);

        await db.SaveChangesAsync();

        return new SeededWorkData(
            firstClient.Id,
            secondClient.Id,
            firstEngagement.Id,
            secondEngagement.Id,
            firstDocument.Id,
            secondDocument.Id);
    }
}
