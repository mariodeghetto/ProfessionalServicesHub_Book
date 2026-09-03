using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Clients;

public sealed record ClientListItem(
    int Id,
    string Code,
    string Name,
    string? City,
    string? Email,
    string Status,
    DateTime CreatedOn);

public sealed class ClientQueryService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<List<ClientListItem>> GetAllAsync()
    {
        var user = await currentUserAccessor.GetAsync();

        await using var db = await contextFactory.CreateDbContextAsync();

        return await db.Clients
            .AsNoTracking()
            .VisibleTo(db, user)
            .OrderBy(x => x.Name)
            .Select(x => new ClientListItem(
                x.Id,
                x.Code,
                x.Name,
                x.City,
                x.Email,
                x.Status.ToString(),
                x.CreatedOn))
            .ToListAsync();
    }
}
