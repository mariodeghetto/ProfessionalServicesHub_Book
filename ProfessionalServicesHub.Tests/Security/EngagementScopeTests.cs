using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Application.Work;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Tests.Infrastructure;

namespace ProfessionalServicesHub.Tests.Security;

public sealed class EngagementScopeTests
{
    [Fact]
    public async Task Collaborator_sees_only_assigned_engagements()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        await TestData.SeedTwoEngagementsAsync(
            database.Options);

        var service = new EngagementQueryService(
            database.Factory,
            new TestCurrentUserAccessor(
                TestUsers.Collaborator()));

        var rows = await service.GetVisibleAsync();

        var row = Assert.Single(rows);
        Assert.Equal("ENG-001", row.Code);
    }

    [Fact]
    public async Task Administrator_sees_all_engagements()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        await TestData.SeedTwoEngagementsAsync(
            database.Options);

        var service = new EngagementQueryService(
            database.Factory,
            new TestCurrentUserAccessor(
                TestUsers.Administrator()));

        var rows = await service.GetVisibleAsync();

        Assert.Equal(2, rows.Count);
        Assert.Equal(
            new[] { "ENG-001", "ENG-002" },
            rows.Select(row => row.Code).ToArray());
    }

    [Fact]
    public async Task Observer_can_read_but_cannot_edit_assignment()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        var seeded =
            await TestData.SeedTwoEngagementsAsync(
                database.Options,
                assignmentKind: AssignmentKind.Observer);

        var service = new EngagementAccessService(
            database.Factory,
            new TestCurrentUserAccessor(
                TestUsers.Collaborator()));

        Assert.True(
            await service.CanReadAsync(
                seeded.FirstEngagementId));

        Assert.False(
            await service.CanEditAsync(
                seeded.FirstEngagementId));
    }
}
