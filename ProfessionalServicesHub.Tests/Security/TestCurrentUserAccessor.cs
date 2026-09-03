using ProfessionalServicesHub.Application.Security;

namespace ProfessionalServicesHub.Tests.Security;

public sealed class TestCurrentUserAccessor(
    CurrentUser user)
    : ICurrentUserAccessor
{
    public Task<CurrentUser> GetAsync() =>
        Task.FromResult(user);
}

public static class TestUsers
{
    public static CurrentUser Administrator(
        string id = "admin-1") =>
        Create(
            id,
            "Test Administrator",
            AppRoles.Administrator);

    public static CurrentUser Coordinator(
        string id = "coordinator-1") =>
        Create(
            id,
            "Test Coordinator",
            AppRoles.Coordinator);

    public static CurrentUser Collaborator(
        string id = "user-1") =>
        Create(
            id,
            "Test Collaborator",
            AppRoles.Collaborator);

    private static CurrentUser Create(
        string id,
        string name,
        string role) =>
        new(
            id,
            name,
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                role
            });
}
