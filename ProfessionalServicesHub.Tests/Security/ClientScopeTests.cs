using ProfessionalServicesHub.Application.Clients;
using ProfessionalServicesHub.Tests.Infrastructure;

namespace ProfessionalServicesHub.Tests.Security;

public sealed class ClientScopeTests
{
    [Fact]
    public async Task Collaborator_sees_only_clients_reachable_from_visible_engagements()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        await TestData.SeedTwoEngagementsAsync(
            database.Options);

        var service = new ClientQueryService(
            database.Factory,
            new TestCurrentUserAccessor(
                TestUsers.Collaborator()));

        var rows = await service.GetAllAsync();

        var row = Assert.Single(rows);
        Assert.Equal("CLI-001", row.Code);
        Assert.Equal("Alpine Design", row.Name);
    }
}
