using System.ComponentModel.DataAnnotations;

namespace ProfessionalServicesHub.Application.Clients;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class BusinessEmailAddressAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    public BusinessEmailAddressAttribute()
    {
        ErrorMessage = "Enter a valid email address with a public domain.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is not string email || string.IsNullOrWhiteSpace(email))
        {
            return true;
        }

        email = email.Trim();

        if (!EmailAddressValidator.IsValid(email))
        {
            return false;
        }

        var atIndex = email.LastIndexOf('@');

        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return false;
        }

        var domain = email[(atIndex + 1)..];
        var lastDotIndex = domain.LastIndexOf('.');

        return lastDotIndex > 0 &&
               lastDotIndex < domain.Length - 2;
    }
}
