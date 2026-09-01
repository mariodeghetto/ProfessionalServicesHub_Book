using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ProfessionalServicesHub.Application.Clients;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class BusinessEmailAddressAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();
    private static readonly IdnMapping IdnMapping = new();

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

        return HasValidPublicDomain(domain);
    }

    private static bool HasValidPublicDomain(string domain)
    {
        string asciiDomain;

        try
        {
            asciiDomain = IdnMapping.GetAscii(domain);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (asciiDomain.Length > 253)
        {
            return false;
        }

        var labels = asciiDomain.Split('.');

        if (labels.Length < 2)
        {
            return false;
        }

        foreach (var label in labels)
        {
            if (label.Length is < 1 or > 63)
            {
                return false;
            }

            if (!char.IsLetterOrDigit(label[0]) ||
                !char.IsLetterOrDigit(label[^1]))
            {
                return false;
            }

            if (label.Any(character =>
                    !char.IsLetterOrDigit(character) &&
                    character != '-'))
            {
                return false;
            }
        }

        return labels[^1].Length >= 2;
    }
}
