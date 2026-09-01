using Microsoft.EntityFrameworkCore;
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
    IDbContextFactory<ApplicationDbContext> contextFactory)
{
    public async Task<List<ClientListItem>> GetAllAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();

        return await db.Clients
            .AsNoTracking()
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
