using ProfessionalServicesHub.Domain.Work;

namespace ProfessionalServicesHub.Application.Work;

public static class WorkflowRules
{
    public static bool CanMove(ActivityStatus from, ActivityStatus to)
    {
        if (from == to)
        {
            return true;
        }

        return (from, to) switch
        {
            (ActivityStatus.Planned, ActivityStatus.InProgress) => true,
            (ActivityStatus.InProgress, ActivityStatus.Waiting) => true,
            (ActivityStatus.InProgress, ActivityStatus.Completed) => true,
            (ActivityStatus.Waiting, ActivityStatus.InProgress) => true,
            (ActivityStatus.Waiting, ActivityStatus.Completed) => true,
            (ActivityStatus.Completed, ActivityStatus.InProgress) => true,
            _ => false
        };
    }
}
