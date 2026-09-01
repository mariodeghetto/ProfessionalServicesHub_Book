namespace ProfessionalServicesHub.Domain.Clients;

public enum ClientStatus
{
    Prospect = 0,
    Active = 1,
    Suspended = 2,
    Archived = 3
}

public sealed class Client
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? City { get; set; }

    public string? Email { get; set; }

    public ClientStatus Status { get; set; } = ClientStatus.Active;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
