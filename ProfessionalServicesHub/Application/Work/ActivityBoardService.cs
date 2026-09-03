using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Work;

public enum WorkflowMoveResult
{
    Success,
    NotFound,
    StaleState,
    InvalidTransition,
    Forbidden
}

public sealed class ActivityBoardService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<List<ActivityBoardItem>> GetBoardAsync()
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db = await contextFactory.CreateDbContextAsync();

        return await db.WorkActivities
            .AsNoTracking()
            .VisibleTo(db, user)
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

        var user = await currentUserAccessor.GetAsync();

        await using var db = await contextFactory.CreateDbContextAsync();

        var visibleActivity = await db.WorkActivities
            .AsNoTracking()
            .VisibleTo(db, user)
            .Where(x => x.Id == activityId)
            .Select(x => new
            {
                x.Id,
                x.EngagementId
            })
            .SingleOrDefaultAsync();

        if (visibleActivity is null)
        {
            return WorkflowMoveResult.NotFound;
        }

        if (!EngagementScope.HasGlobalOperationalScope(user))
        {
            var canEdit = await db.EngagementAssignments
                .AsNoTracking()
                .AnyAsync(assignment =>
                    assignment.EngagementId ==
                        visibleActivity.EngagementId &&
                    assignment.UserId == user.Id &&
                    assignment.Kind != AssignmentKind.Observer);

            if (!canEdit)
            {
                return WorkflowMoveResult.Forbidden;
            }
        }

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
