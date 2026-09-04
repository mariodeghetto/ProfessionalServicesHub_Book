using ProfessionalServicesHub.Components.Services;

namespace ProfessionalServicesHub.Tests.Presentation;

public sealed class UiNotificationContentEncoderTests
{
    [Fact]
    public void Encode_neutralizes_html_markup()
    {
        var encoded =
            UiNotificationContentEncoder.Encode(
                "<script>alert('x')</script>&");

        Assert.DoesNotContain(
            "<script>",
            encoded,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "&lt;script&gt;",
            encoded,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "&amp;",
            encoded,
            StringComparison.Ordinal);
    }
}
