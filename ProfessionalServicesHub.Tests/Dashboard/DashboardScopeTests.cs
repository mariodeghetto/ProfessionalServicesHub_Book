using ProfessionalServicesHub.Application.Dashboard;
using ProfessionalServicesHub.Tests.Infrastructure;
using ProfessionalServicesHub.Tests.Security;

namespace ProfessionalServicesHub.Tests.Dashboard;

public sealed class DashboardScopeTests
{
    [Fact]
    public async Task Collaborator_dashboard_aggregates_only_visible_data()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        await TestData.SeedTwoEngagementsAsync(
            database.Options);

        var service = new DashboardService(
            database.Factory,
            new TestCurrentUserAccessor(
                TestUsers.Collaborator()));

        var snapshot = await service.LoadAsync(
            DateTime.Today.AddHours(12),
            DateTime.UtcNow);

        Assert.Equal(
            1,
            snapshot.EngagementsWithOpenActivities);

        Assert.Equal(
            1,
            snapshot.OpenActivities);

        Assert.Equal(
            1,
            snapshot.DocumentsLastThirtyDays);

        Assert.Equal(
            1,
            snapshot.ActivitiesByStatus
                .Single(item =>
                    item.Status == "In progress")
                .Count);
    }
}
