using Microsoft.AspNetCore.Identity;
using ProfessionalServicesHub.Application.Security;

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

        var email =
            configuration["DemoIdentity:AdministratorEmail"];

        var password =
            configuration["DemoIdentity:AdministratorPassword"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = "Demo Administrator"
            };

            var created =
                await userManager.CreateAsync(user, password);

            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    "Unable to provision the demo administrator: " +
                    FormatErrors(created.Errors));
            }
        }

        if (!await userManager.IsInRoleAsync(
                user,
                AppRoles.Administrator))
        {
            var added = await userManager.AddToRoleAsync(
                user,
                AppRoles.Administrator);

            if (!added.Succeeded)
            {
                throw new InvalidOperationException(
                    "Unable to assign the Administrator role: " +
                    FormatErrors(added.Errors));
            }
        }
    }

    private static string FormatErrors(
        IEnumerable<IdentityError> errors) =>
        string.Join(
            "; ",
            errors.Select(error => error.Description));
}
