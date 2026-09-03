using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Domain.Calendar;
using ProfessionalServicesHub.Domain.Work;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Calendar;

public sealed class CalendarService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<List<ScheduleItem>> GetRangeAsync(
        DateTime from,
        DateTime to)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.CalendarEntries
            .AsNoTracking()
            .VisibleTo(db, user)
            .Where(x => x.StartTime < to && x.EndTime > from)
            .OrderBy(x => x.StartTime)
            .ThenBy(x => x.Id)
            .Select(x => new ScheduleItem
            {
                Id = x.Id,
                Subject = x.Subject,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                IsAllDay = x.IsAllDay,
                Location = x.Location,
                Description = x.Description,
                Kind = x.Kind,
                ClientId = x.ClientId,
                ClientName = x.Client != null
                    ? x.Client.Name
                    : null,
                EngagementId = x.EngagementId,
                EngagementCode = x.Engagement != null
                    ? x.Engagement.Code
                    : null,
                WorkActivityId = x.WorkActivityId,
                Assignee = x.Assignee
            })
            .ToListAsync();
    }

    public async Task<CalendarWriteResult> CreateAsync(
        ScheduleItem item)
    {
        var validation = NormalizeAndValidate(item);

        if (validation is not null)
        {
            return new(
                CalendarWriteStatus.ValidationFailed,
                validation);
        }

        var user = await currentUserAccessor.GetAsync();

        await using var db = await dbFactory.CreateDbContextAsync();

        var scopeError = await ValidateWriteScopeAsync(
            db,
            user,
            item.EngagementId);

        if (scopeError is not null)
        {
            return new(
                CalendarWriteStatus.Forbidden,
                scopeError);
        }

        var relationshipError =
            await ValidateRelationshipsAsync(db, item);

        if (relationshipError is not null)
        {
            return new(
                CalendarWriteStatus.ValidationFailed,
                relationshipError);
        }

        if (await HasConflictAsync(db, item, user))
        {
            return new(
                CalendarWriteStatus.Conflict,
                "The assignee already has an appointment in this time range.");
        }

        var entity = new CalendarEntry
        {
            Subject = item.Subject
        };

        Apply(entity, item);

        db.CalendarEntries.Add(entity);
        await db.SaveChangesAsync();

        item.Id = entity.Id;

        return new(
            CalendarWriteStatus.Success,
            Id: entity.Id);
    }

    public async Task<CalendarWriteResult> UpdateAsync(
        ScheduleItem item)
    {
        var validation = NormalizeAndValidate(item);

        if (validation is not null)
        {
            return new(
                CalendarWriteStatus.ValidationFailed,
                validation);
        }

        var user = await currentUserAccessor.GetAsync();

        await using var db = await dbFactory.CreateDbContextAsync();

        var entity = await db.CalendarEntries
            .SingleOrDefaultAsync(x => x.Id == item.Id);

        if (entity is null)
        {
            return new(
                CalendarWriteStatus.NotFound,
                "The calendar entry is no longer available.");
        }

        var existingScopeError = await ValidateWriteScopeAsync(
            db,
            user,
            entity.EngagementId);

        if (existingScopeError is not null)
        {
            return new(
                CalendarWriteStatus.Forbidden,
                existingScopeError);
        }

        var targetScopeError = await ValidateWriteScopeAsync(
            db,
            user,
            item.EngagementId);

        if (targetScopeError is not null)
        {
            return new(
                CalendarWriteStatus.Forbidden,
                targetScopeError);
        }

        var relationshipError =
            await ValidateRelationshipsAsync(db, item);

        if (relationshipError is not null)
        {
            return new(
                CalendarWriteStatus.ValidationFailed,
                relationshipError);
        }

        if (await HasConflictAsync(db, item, user))
        {
            return new(
                CalendarWriteStatus.Conflict,
                "The assignee already has an appointment in this time range.");
        }

        Apply(entity, item);
        await db.SaveChangesAsync();

        return new(
            CalendarWriteStatus.Success,
            Id: entity.Id);
    }

    public async Task<CalendarWriteResult> DeleteAsync(int id)
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db = await dbFactory.CreateDbContextAsync();

        var entity = await db.CalendarEntries
            .SingleOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return new(
                CalendarWriteStatus.NotFound,
                "The calendar entry is no longer available.");
        }

        var scopeError = await ValidateWriteScopeAsync(
            db,
            user,
            entity.EngagementId);

        if (scopeError is not null)
        {
            return new(
                CalendarWriteStatus.Forbidden,
                scopeError);
        }

        db.CalendarEntries.Remove(entity);
        await db.SaveChangesAsync();

        return new(CalendarWriteStatus.Success);
    }

    private static string? NormalizeAndValidate(
        ScheduleItem item)
    {
        item.Subject = item.Subject.Trim();
        item.Location = NormalizeOptional(item.Location);
        item.Description = NormalizeOptional(item.Description);
        item.Assignee = NormalizeOptional(item.Assignee);

        if (string.IsNullOrWhiteSpace(item.Subject))
        {
            return "Subject is required.";
        }

        if (!Enum.IsDefined(item.Kind))
        {
            return "The calendar entry type is not valid.";
        }

        if (item.Kind == CalendarEntryKind.Deadline &&
            item.IsAllDay)
        {
            item.StartTime = item.StartTime.Date;
            item.EndTime = item.StartTime.AddDays(1);
        }

        if (item.EndTime <= item.StartTime)
        {
            return "End time must be later than start time.";
        }

        return null;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static async Task<string?> ValidateWriteScopeAsync(
        ApplicationDbContext db,
        CurrentUser user,
        int? engagementId)
    {
        if (EngagementScope.HasGlobalOperationalScope(user))
        {
            return null;
        }

        if (engagementId is not int scopedEngagementId)
        {
            return
                "You do not have permission to modify general calendar entries.";
        }

        var canEdit = await db.EngagementAssignments
            .AsNoTracking()
            .AnyAsync(assignment =>
                assignment.EngagementId == scopedEngagementId &&
                assignment.UserId == user.Id &&
                assignment.Kind != AssignmentKind.Observer);

        return canEdit
            ? null
            : "You do not have permission to modify this calendar entry.";
    }

    private static async Task<string?> ValidateRelationshipsAsync(
        ApplicationDbContext db,
        ScheduleItem item)
    {
        if (item.ClientId.HasValue &&
            !await db.Clients.AnyAsync(x => x.Id == item.ClientId.Value))
        {
            return "The selected client is no longer available.";
        }

        if (item.EngagementId.HasValue)
        {
            var engagement = await db.Engagements
                .AsNoTracking()
                .Where(x => x.Id == item.EngagementId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.ClientId
                })
                .SingleOrDefaultAsync();

            if (engagement is null)
            {
                return "The selected engagement is no longer available.";
            }

            if (item.ClientId.HasValue &&
                engagement.ClientId != item.ClientId.Value)
            {
                return
                    "The selected engagement does not belong to the selected client.";
            }
        }

        if (item.WorkActivityId.HasValue)
        {
            var activity = await db.WorkActivities
                .AsNoTracking()
                .Where(x => x.Id == item.WorkActivityId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.EngagementId
                })
                .SingleOrDefaultAsync();

            if (activity is null)
            {
                return "The selected work activity is no longer available.";
            }

            if (item.EngagementId.HasValue &&
                activity.EngagementId != item.EngagementId.Value)
            {
                return
                    "The selected work activity does not belong to the selected engagement.";
            }
        }

        return null;
    }

    private static async Task<bool> HasConflictAsync(
        ApplicationDbContext db,
        ScheduleItem item,
        CurrentUser user)
    {
        if (item.Kind != CalendarEntryKind.Appointment ||
            item.IsAllDay ||
            string.IsNullOrWhiteSpace(item.Assignee))
        {
            return false;
        }

        var assignee = item.Assignee.Trim();

        return await db.CalendarEntries
            .AsNoTracking()
            .VisibleTo(db, user)
            .AnyAsync(x =>
                x.Id != item.Id &&
                x.Kind == CalendarEntryKind.Appointment &&
                !x.IsAllDay &&
                x.Assignee == assignee &&
                x.StartTime < item.EndTime &&
                x.EndTime > item.StartTime);
    }

    private static void Apply(
        CalendarEntry entity,
        ScheduleItem item)
    {
        entity.Subject = item.Subject;
        entity.StartTime = item.StartTime;
        entity.EndTime = item.EndTime;
        entity.IsAllDay = item.IsAllDay;
        entity.Location = item.Location;
        entity.Description = item.Description;
        entity.Kind = item.Kind;
        entity.ClientId = item.ClientId;
        entity.EngagementId = item.EngagementId;
        entity.WorkActivityId = item.WorkActivityId;
        entity.Assignee = item.Assignee;
    }
}
