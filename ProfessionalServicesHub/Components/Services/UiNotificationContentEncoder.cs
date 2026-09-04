using System.Text.Encodings.Web;

namespace ProfessionalServicesHub.Components.Services;

public static class UiNotificationContentEncoder
{
    public static string Encode(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return HtmlEncoder.Default.Encode(content);
    }
}
