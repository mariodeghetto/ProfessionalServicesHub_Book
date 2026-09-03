using ProfessionalServicesHub.Application.Work;
using ProfessionalServicesHub.Domain.Work;

namespace ProfessionalServicesHub.Tests.Work;

public sealed class WorkflowRulesTests
{
    [Theory]
    [InlineData(
        ActivityStatus.Planned,
        ActivityStatus.InProgress)]
    [InlineData(
        ActivityStatus.InProgress,
        ActivityStatus.Waiting)]
    [InlineData(
        ActivityStatus.InProgress,
        ActivityStatus.Completed)]
    [InlineData(
        ActivityStatus.Waiting,
        ActivityStatus.InProgress)]
    [InlineData(
        ActivityStatus.Waiting,
        ActivityStatus.Completed)]
    [InlineData(
        ActivityStatus.Completed,
        ActivityStatus.InProgress)]
    public void CanMove_AllowsSupportedTransitions(
        ActivityStatus from,
        ActivityStatus to)
    {
        Assert.True(
            WorkflowRules.CanMove(from, to));
    }

    [Theory]
    [InlineData(
        ActivityStatus.Planned,
        ActivityStatus.Completed)]
    [InlineData(
        ActivityStatus.Planned,
        ActivityStatus.Waiting)]
    [InlineData(
        ActivityStatus.Completed,
        ActivityStatus.Planned)]
    public void CanMove_RejectsUnsupportedTransitions(
        ActivityStatus from,
        ActivityStatus to)
    {
        Assert.False(
            WorkflowRules.CanMove(from, to));
    }

    [Theory]
    [InlineData(ActivityStatus.Planned)]
    [InlineData(ActivityStatus.InProgress)]
    [InlineData(ActivityStatus.Waiting)]
    [InlineData(ActivityStatus.Completed)]
    public void CanMove_AllowsSameState(
        ActivityStatus status)
    {
        Assert.True(
            WorkflowRules.CanMove(status, status));
    }
}
