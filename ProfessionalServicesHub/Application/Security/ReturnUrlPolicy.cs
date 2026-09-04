using Microsoft.AspNetCore.Http.HttpResults;

namespace ProfessionalServicesHub.Application.Security;

public static class ReturnUrlPolicy
{
    public static string GetSafeLocalUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) ||
            !RedirectHttpResult.IsLocalUrl(returnUrl))
        {
            return "/";
        }

        return returnUrl;
    }
}
