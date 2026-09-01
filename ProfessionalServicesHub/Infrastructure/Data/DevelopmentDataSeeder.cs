using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Domain.Clients;

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
}
