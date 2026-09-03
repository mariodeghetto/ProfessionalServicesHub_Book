using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using ProfessionalServicesHub.Application.Security;

namespace ProfessionalServicesHub.Components.Security;

public sealed class BlazorCurrentUserAccessor(
    AuthenticationStateProvider authenticationStateProvider)
    : ICurrentUserAccessor
{
    public async Task<CurrentUser> GetAsync()
    {
        var state =
            await authenticationStateProvider.GetAuthenticationStateAsync();

        var principal = state.User;

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
