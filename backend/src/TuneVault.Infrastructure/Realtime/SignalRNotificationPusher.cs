using Microsoft.AspNetCore.SignalR;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.Notification.DTOs;

namespace TuneVault.Infrastructure.Realtime;

/// <summary>
/// Implementation of INotificationPusher using SignalR.
/// </summary>
public sealed class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRNotificationPusher"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context.</param>
    public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    /// <summary>
    /// Sends a notification to the correct group (userId) of the recipient.
    /// </summary>
    /// <param name="userId">The identifier of the user receiving the notification.</param>
    /// <param name="notification">The notification data to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PushAsync(string userId, NotificationDto notification, CancellationToken ct = default)
    {
        // Bước 1: Gửi thông báo tới group của người dùng.
        // Tên event là "ReceiveNotification" để client lắng nghe.
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification, ct);
    }
}