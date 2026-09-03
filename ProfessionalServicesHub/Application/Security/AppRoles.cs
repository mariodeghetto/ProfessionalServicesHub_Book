namespace ProfessionalServicesHub.Application.Security;

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string Coordinator = "Coordinator";
    public const string Collaborator = "Collaborator";

    public static readonly string[] All =
    [
        Administrator,
        Coordinator,
        Collaborator
    ];
}
