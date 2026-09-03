using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Work;

public enum WorkflowMoveResult
{
    Success,
    NotFound,
    StaleState,
    InvalidTransition
}

public sealed class ActivityBoardService(
    IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<List<ActivityBoardItem>> GetBoardAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();

        return await db.WorkActivities
            .AsNoTracking()
            .OrderBy(x => x.Status)
            .ThenBy(x => x.Rank)
            .ThenBy(x => x.Id)
            .Select(x => new ActivityBoardItem
            {
                Id = x.Id,
                EngagementCode = x.Engagement.Code,
                Title = x.Title,
                Description = x.Description,
                Assignee = x.Assignee ?? "Unassigned",
                DueDate = x.DueDate,
                StatusKey = x.Status.ToString(),
                Priority = x.Priority.ToString(),
                Rank = x.Rank
            })
            .ToListAsync();
    }

    public async Task<WorkflowMoveResult> MoveAsync(
        int activityId,
        ActivityStatus expectedSource,
        ActivityStatus target)
    {
        if (!WorkflowRules.CanMove(expectedSource, target))
        {
            return WorkflowMoveResult.InvalidTransition;
        }

        await using var db = await contextFactory.CreateDbContextAsync();

        var affected = await db.WorkActivities
            .Where(x =>
                x.Id == activityId &&
                x.Status == expectedSource)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, target));

        if (affected == 1)
        {
            return WorkflowMoveResult.Success;
        }

        var exists = await db.WorkActivities
            .AsNoTracking()
            .AnyAsync(x => x.Id == activityId);

        return exists
            ? WorkflowMoveResult.StaleState
            : WorkflowMoveResult.NotFound;
    }
}
