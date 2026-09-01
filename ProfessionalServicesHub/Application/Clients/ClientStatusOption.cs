using ProfessionalServicesHub.Domain.Clients;

namespace ProfessionalServicesHub.Application.Clients;

public sealed record ClientStatusOption(
    ClientStatus Value,
    string Text);
