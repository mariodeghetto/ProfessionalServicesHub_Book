namespace ProfessionalServicesHub.Components.Services;

public enum UiNotificationKind
{
    Success,
    Information,
    Warning,
    Error
}

public sealed record UiNotification(
    string Title,
    string Message,
    UiNotificationKind Kind,
    int Timeout = 4000);

public sealed class UiNotificationService
{
    public event Func<UiNotification, Task>? Published;

    public async Task PublishAsync(UiNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlers = Published?
            .GetInvocationList()
            .Cast<Func<UiNotification, Task>>()
            .ToArray() ?? [];

        await Task.WhenAll(
            handlers.Select(handler => handler(notification)));
    }
}
