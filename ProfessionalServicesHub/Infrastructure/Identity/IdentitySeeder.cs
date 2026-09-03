using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task EnsureIdentityAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        await using var scope = services.CreateAsyncScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result =
                await roleManager.CreateAsync(
                    new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create role '{roleName}': " +
                    FormatErrors(result.Errors));
            }
        }

        if (!environment.IsDevelopment())
        {
            return;
        }

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureDemoUserAsync(
            userManager,
            configuration["DemoIdentity:AdministratorEmail"],
            configuration["DemoIdentity:AdministratorPassword"],
            "Demo Administrator",
            AppRoles.Administrator);

        var collaborator = await EnsureDemoUserAsync(
            userManager,
            configuration["DemoIdentity:CollaboratorEmail"],
            configuration["DemoIdentity:CollaboratorPassword"],
            "Demo Collaborator",
            AppRoles.Collaborator);

        if (collaborator is not null)
        {
            await EnsureCollaboratorAssignmentAsync(
                scope.ServiceProvider,
                collaborator.Id);
        }
    }

    private static async Task<ApplicationUser?> EnsureDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        string? email,
        string? password,
        string displayName,
        string role)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName
            };

            var created =
                await userManager.CreateAsync(user, password);

            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to provision '{email}': " +
                    FormatErrors(created.Errors));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var added = await userManager.AddToRoleAsync(
                user,
                role);

            if (!added.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to assign role '{role}' to '{email}': " +
                    FormatErrors(added.Errors));
            }
        }

        return user;
    }

    private static async Task EnsureCollaboratorAssignmentAsync(
        IServiceProvider services,
        string userId)
    {
        var factory = services
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        await using var db = await factory.CreateDbContextAsync();

        var engagementId = await db.Engagements
            .Where(engagement => engagement.Code == "ENG-001")
            .Select(engagement => (int?)engagement.Id)
            .SingleOrDefaultAsync();

        if (engagementId is null)
        {
            return;
        }

        var exists = await db.EngagementAssignments
            .AnyAsync(assignment =>
                assignment.EngagementId == engagementId.Value &&
                assignment.UserId == userId);

        if (exists)
        {
            return;
        }

        db.EngagementAssignments.Add(
            new EngagementAssignment
            {
                EngagementId = engagementId.Value,
                UserId = userId,
                Kind = AssignmentKind.Collaborator
            });

        await db.SaveChangesAsync();
    }

    private static string FormatErrors(
        IEnumerable<IdentityError> errors) =>
        string.Join(
            "; ",
            errors.Select(error => error.Description));
}
