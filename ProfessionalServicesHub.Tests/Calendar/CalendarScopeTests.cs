using ProfessionalServicesHub.Application.Calendar;
using ProfessionalServicesHub.Tests.Infrastructure;
using ProfessionalServicesHub.Tests.Security;

namespace ProfessionalServicesHub.Tests.Calendar;

public sealed class CalendarScopeTests
{
    [Fact]
    public async Task Collaborator_receives_only_calendar_entries_in_scope()
    {
        await using var database =
            await TestDatabase.CreateAsync();

        await TestData.SeedTwoEngagementsAsync(
            database.Options);

        var service = new CalendarService(
            database.Factory,
            new TestCurrentUserAccessor(
                TestUsers.Collaborator()));

        var from = DateTime.Today;
        var to = from.AddDays(1);

        var rows = await service.GetRangeAsync(
            from,
            to);

        var row = Assert.Single(rows);
        Assert.Equal("Scoped meeting", row.Subject);
        Assert.Equal("ENG-001", row.EngagementCode);
    }
}
