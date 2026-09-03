using ProfessionalServicesHub.Domain.Clients;

namespace ProfessionalServicesHub.Domain.Work;

public sealed class Engagement
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public Client Client { get; set; } = default!;

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ICollection<WorkActivity> Activities { get; set; } =
        new List<WorkActivity>();
}
