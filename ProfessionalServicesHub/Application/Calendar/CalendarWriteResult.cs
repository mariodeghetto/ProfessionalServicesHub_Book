namespace ProfessionalServicesHub.Application.Calendar;

public enum CalendarWriteStatus
{
    Success,
    ValidationFailed,
    NotFound,
    Conflict
}

public sealed record CalendarWriteResult(
    CalendarWriteStatus Status,
    string? Error = null,
    int? Id = null)
{
    public bool Success => Status == CalendarWriteStatus.Success;
}
