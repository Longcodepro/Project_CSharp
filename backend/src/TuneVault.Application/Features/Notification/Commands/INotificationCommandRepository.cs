namespace TuneVault.Application.Features.Notification.Commands;

public interface INotificationCommandRepository
{
    Task<string> InsertNotificationAsync(NotificationInsertModel notification);
}
