using System.ComponentModel.DataAnnotations;
using ProfessionalServicesHub.Domain.Clients;

namespace ProfessionalServicesHub.Application.Clients;

public sealed class ClientEditModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Client code is required.")]
    [StringLength(20, ErrorMessage = "Client code can contain at most 20 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Client name is required.")]
    [StringLength(200, ErrorMessage = "Client name can contain at most 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "City can contain at most 100 characters.")]
    public string? City { get; set; }

    [BusinessEmailAddress]
    [StringLength(254, ErrorMessage = "Email can contain at most 254 characters.")]
    public string? Email { get; set; }

    public ClientStatus Status { get; set; } = ClientStatus.Active;
}
