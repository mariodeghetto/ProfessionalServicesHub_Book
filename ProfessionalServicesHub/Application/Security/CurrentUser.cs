using System.Security.Claims;

namespace ProfessionalServicesHub.Application.Security;

public sealed record CurrentUser(
    string Id,
    string? Name,
    IReadOnlySet<string> Roles)
{
    public bool IsInRole(string role) =>
        Roles.Contains(role);

    public static CurrentUser FromPrincipal(
        ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException();
        }

        var id = principal.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

        var roles = principal
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new CurrentUser(
            id,
            principal.Identity.Name,
            roles);
    }
}

public interface ICurrentUserAccessor
{
    Task<CurrentUser> GetAsync();
}
