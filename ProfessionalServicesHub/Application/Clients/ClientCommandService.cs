using Microsoft.EntityFrameworkCore;
using ProfessionalServicesHub.Application.Security;
using ProfessionalServicesHub.Domain.Clients;
using ProfessionalServicesHub.Infrastructure.Data;

namespace ProfessionalServicesHub.Application.Clients;

public enum SaveClientStatus
{
    Success,
    DuplicateCode,
    NotFound,
    Forbidden
}

public sealed record SaveClientResult(
    SaveClientStatus Status,
    int? ClientId = null);

public sealed class ClientCommandService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    ICurrentUserAccessor currentUserAccessor)
{
    public async Task<ClientEditModel?> GetForEditAsync(int id)
    {
        var user = await currentUserAccessor.GetAsync();

        if (!EngagementScope.HasGlobalOperationalScope(user))
        {
            return null;
        }

        await using var db = await contextFactory.CreateDbContextAsync();

        return await db.Clients
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ClientEditModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                City = x.City,
                Email = x.Email,
                Status = x.Status
            })
            .SingleOrDefaultAsync();
    }

    public async Task<SaveClientResult> SaveAsync(ClientEditModel model)
    {
        var user = await currentUserAccessor.GetAsync();

        if (!EngagementScope.HasGlobalOperationalScope(user))
        {
            return new SaveClientResult(
                SaveClientStatus.Forbidden);
        }

        await using var db = await contextFactory.CreateDbContextAsync();

        var normalizedCode = model.Code.Trim().ToUpperInvariant();

        var duplicate = await db.Clients.AnyAsync(x =>
            x.Code == normalizedCode &&
            (!model.Id.HasValue || x.Id != model.Id.Value));

        if (duplicate)
        {
            return new SaveClientResult(SaveClientStatus.DuplicateCode);
        }

        Client client;

        if (model.Id is null)
        {
            client = new Client
            {
                Code = normalizedCode,
                Name = model.Name.Trim()
            };

            db.Clients.Add(client);
        }
        else
        {
            var existingClient = await db.Clients
                .SingleOrDefaultAsync(x => x.Id == model.Id.Value);

            if (existingClient is null)
            {
                return new SaveClientResult(SaveClientStatus.NotFound);
            }

            client = existingClient;
        }

        client.Code = normalizedCode;
        client.Name = model.Name.Trim();
        client.City = string.IsNullOrWhiteSpace(model.City)
            ? null
            : model.City.Trim();
        client.Email = string.IsNullOrWhiteSpace(model.Email)
            ? null
            : model.Email.Trim();
        client.Status = model.Status;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            await using var verificationDb =
                await contextFactory.CreateDbContextAsync();

            var duplicateAfterFailure =
                await verificationDb.Clients
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Code == normalizedCode &&
                        (!model.Id.HasValue || x.Id != model.Id.Value));

            if (duplicateAfterFailure)
            {
                return new SaveClientResult(
                    SaveClientStatus.DuplicateCode);
            }

            throw;
        }

        return new SaveClientResult(
            SaveClientStatus.Success,
            client.Id);
    }
}
