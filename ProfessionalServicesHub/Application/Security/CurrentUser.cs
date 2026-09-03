namespace ProfessionalServicesHub.Application.Security;

public sealed record CurrentUser(
    string Id,
    string? Name,
    IReadOnlySet<string> Roles)
{
    public bool IsInRole(string role) =>
        Roles.Contains(role);
}

public interface ICurrentUserAccessor
{
    Task<CurrentUser> GetAsync();
}
