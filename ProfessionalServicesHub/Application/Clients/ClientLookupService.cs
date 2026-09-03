using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Clients;

public sealed record ClientLookupItem(
    int Id,
    string DisplayName);

public sealed class ClientLookupService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<IReadOnlyList<ClientLookupItem>> SearchAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var normalized = text.Trim();

        if (normalized.Length < 2)
        {
            return [];
        }

        var user = await currentUserAccessor.GetAsync();

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Clients
            .AsNoTracking()
            .VisibleTo(db, user)
            .Where(client =>
                client.Name.Contains(normalized) ||
                client.Code.Contains(normalized))
            .OrderBy(client => client.Name)
            .ThenBy(client => client.Code)
            .Take(20)
            .Select(client => new ClientLookupItem(
                client.Id,
                client.Code + " - " + client.Name))
            .ToListAsync(cancellationToken);
    }
}
