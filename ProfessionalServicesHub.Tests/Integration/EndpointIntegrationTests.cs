using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProfessionalServicesHub.Tests.Integration;

public sealed class EndpointIntegrationTests
{
    [Fact]
    public async Task Health_endpoints_report_live_and_ready()
    {
        var factory =
            new TestWebApplicationFactory();

        try
        {
            using (factory)
            {
                using var client =
                    factory.CreateClient();

                var live =
                    await client.GetAsync(
                        "/health/live");

                var ready =
                    await client.GetAsync(
                        "/health/ready");

                Assert.Equal(
                    HttpStatusCode.OK,
                    live.StatusCode);

                Assert.Equal(
                    HttpStatusCode.OK,
                    ready.StatusCode);
            }
        }
        finally
        {
            factory.DeleteDatabase();
        }
    }

    [Fact]
    public async Task Anonymous_document_download_is_challenged()
    {
        var factory =
            new TestWebApplicationFactory();

        try
        {
            using (factory)
            {
                using var client =
                    factory.CreateClient(
                        new WebApplicationFactoryClientOptions
                        {
                            AllowAutoRedirect = false
                        });

                var response =
                    await client.GetAsync(
                        "/documents/42/download");

                Assert.Equal(
                    HttpStatusCode.Redirect,
                    response.StatusCode);

                Assert.NotNull(
                    response.Headers.Location);

                Assert.Contains(
                    "/account/login",
                    response.Headers.Location!
                        .OriginalString,
                    StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            factory.DeleteDatabase();
        }
    }
}
