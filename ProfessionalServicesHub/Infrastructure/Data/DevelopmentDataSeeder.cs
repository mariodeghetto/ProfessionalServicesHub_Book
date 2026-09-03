using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Domain.Clients;
using ProfessionalServicesHub.Domain.Work;

namespace ProfessionalServicesHub.Infrastructure.Data;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IDbContextFactory<ApplicationDbContext> factory)
    {
        await using var db = await factory.CreateDbContextAsync();

        if (await db.Clients.AnyAsync())
        {
            return;
        }

        db.Clients.AddRange(
            new Client
            {
                Code = "CLI-001",
                Name = "Alpine Design",
                City = "Denver",
                Email = "info@alpinedesign.example",
                Status = ClientStatus.Active
            },
            new Client
            {
                Code = "CLI-002",
                Name = "Blue Harbor Consulting",
                City = "Boston",
                Email = "office@blueharbor.example",
                Status = ClientStatus.Active
            },
            new Client
            {
                Code = "CLI-003",
                Name = "Cedar Labs",
                City = "Seattle",
                Email = "contact@cedarlabs.example",
                Status = ClientStatus.Prospect
            },
            new Client
            {
                Code = "CLI-004",
                Name = "Delta Advisory",
                City = "Chicago",
                Email = "hello@deltaadvisory.example",
                Status = ClientStatus.Active
            },
            new Client
            {
                Code = "CLI-005",
                Name = "Evergreen Partners",
                City = "Portland",
                Email = "admin@evergreen.example",
                Status = ClientStatus.Suspended
            },
            new Client
            {
                Code = "CLI-006",
                Name = "Frontier Analytics",
                City = "Austin",
                Email = "team@frontieranalytics.example",
                Status = ClientStatus.Active
            },
            new Client
            {
                Code = "CLI-007",
                Name = "Granite Works",
                City = "Phoenix",
                Email = "info@graniteworks.example",
                Status = ClientStatus.Prospect
            },
            new Client
            {
                Code = "CLI-008",
                Name = "Harbor Point",
                City = "San Diego",
                Email = "office@harborpoint.example",
                Status = ClientStatus.Active
            },
            new Client
            {
                Code = "CLI-009",
                Name = "Ironwood Services",
                City = "Atlanta",
                Email = "contact@ironwood.example",
                Status = ClientStatus.Archived
            },
            new Client
            {
                Code = "CLI-010",
                Name = "Juniper Group",
                City = "New York",
                Email = "info@junipergroup.example",
                Status = ClientStatus.Active
            },
            new Client
            {
                Code = "CLI-011",
                Name = "Keystone Studio",
                City = "Philadelphia",
                Email = "team@keystonestudio.example",
                Status = ClientStatus.Active
            },
            new Client
            {
                Code = "CLI-012",
                Name = "Lighthouse Advisors",
                City = "Miami",
                Email = "office@lighthouse.example",
                Status = ClientStatus.Prospect
            });

        await db.SaveChangesAsync();
    }

    public static async Task SeedWorkAsync(
        IDbContextFactory<ApplicationDbContext> factory)
    {
        await using var db = await factory.CreateDbContextAsync();

        if (!await db.Engagements.AnyAsync())
        {
            var alpineId = await db.Clients
                .Where(x => x.Code == "CLI-001")
                .Select(x => x.Id)
                .SingleAsync();

            var blueHarborId = await db.Clients
                .Where(x => x.Code == "CLI-002")
                .Select(x => x.Id)
                .SingleAsync();

            db.Engagements.AddRange(
                new Engagement
                {
                    ClientId = alpineId,
                    Code = "ENG-001",
                    Title = "Website accessibility review"
                },
                new Engagement
                {
                    ClientId = blueHarborId,
                    Code = "ENG-002",
                    Title = "Reporting modernization"
                });

            await db.SaveChangesAsync();
        }

        if (await db.WorkActivities.AnyAsync())
        {
            return;
        }

        var engagements = await db.Engagements
            .Where(x => x.Code == "ENG-001" || x.Code == "ENG-002")
            .ToDictionaryAsync(x => x.Code);

        if (!engagements.TryGetValue("ENG-001", out var accessibilityReview) ||
            !engagements.TryGetValue("ENG-002", out var reportingModernization))
        {
            throw new InvalidOperationException(
                "The development engagements required by the activity seed are missing.");
        }

        var today = DateTime.Today;

        db.WorkActivities.AddRange(
            new WorkActivity
            {
                EngagementId = accessibilityReview.Id,
                Title = "Collect current pages",
                Assignee = "Alex Morgan",
                DueDate = today.AddDays(-1),
                Status = ActivityStatus.InProgress,
                Priority = ActivityPriority.High,
                Rank = 10
            },
            new WorkActivity
            {
                EngagementId = accessibilityReview.Id,
                Title = "Review keyboard navigation",
                Assignee = "Alex Morgan",
                DueDate = today.AddDays(2),
                Status = ActivityStatus.Planned,
                Priority = ActivityPriority.Normal,
                Rank = 20
            },
            new WorkActivity
            {
                EngagementId = accessibilityReview.Id,
                Title = "Wait for brand assets",
                Assignee = "Jordan Lee",
                DueDate = today.AddDays(4),
                Status = ActivityStatus.Waiting,
                Priority = ActivityPriority.Normal,
                Rank = 10
            },
            new WorkActivity
            {
                EngagementId = reportingModernization.Id,
                Title = "Inventory data sources",
                Assignee = "Jordan Lee",
                DueDate = today.AddDays(1),
                Status = ActivityStatus.InProgress,
                Priority = ActivityPriority.Critical,
                Rank = 10
            },
            new WorkActivity
            {
                EngagementId = reportingModernization.Id,
                Title = "Prepare migration checklist",
                Assignee = null,
                DueDate = today.AddDays(5),
                Status = ActivityStatus.Planned,
                Priority = ActivityPriority.High,
                Rank = 30
            });

        await db.SaveChangesAsync();
    }
}
