using TuneVault.Application.Features.Notification.DTOs;

namespace TuneVault.Application.Abstractions;

/// <summary>
/// Abstraction for pushing real-time notifications to clients.
/// This allows the Application layer to not depend directly on SignalR (which is in Infrastructure).
/// </summary>
public interface INotificationPusher
{
    /// <summary>
    /// Sends a notification to the correct group (userId) of the recipient.
    /// </summary>
    /// <param name="userId">The identifier of the user receiving the notification.</param>
    /// <param name="notification">The notification data to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PushAsync(string userId, NotificationDto notification, CancellationToken ct = default);
}