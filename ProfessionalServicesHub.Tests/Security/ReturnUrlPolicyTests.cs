using ProfessionalServicesHub.Application.Security;

namespace ProfessionalServicesHub.Tests.Security;

public sealed class ReturnUrlPolicyTests
{
    [Fact]
    public void GetSafeLocalUrl_accepts_only_local_urls()
    {
        Assert.Equal(
            "/clients?status=active",
            ReturnUrlPolicy.GetSafeLocalUrl(
                "/clients?status=active"));

        Assert.Equal(
            "/",
            ReturnUrlPolicy.GetSafeLocalUrl(null));

        Assert.Equal(
            "/",
            ReturnUrlPolicy.GetSafeLocalUrl(
                "https://evil.example"));

        Assert.Equal(
            "/",
            ReturnUrlPolicy.GetSafeLocalUrl(
                "//evil.example"));

        Assert.Equal(
            "/",
            ReturnUrlPolicy.GetSafeLocalUrl(
                @"/\evil.example"));

        Assert.Equal(
            "/",
            ReturnUrlPolicy.GetSafeLocalUrl(
                "../clients"));
    }
}
