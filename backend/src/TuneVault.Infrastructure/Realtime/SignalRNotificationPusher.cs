using Microsoft.AspNetCore.SignalR;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.Notification.DTOs;

namespace TuneVault.Infrastructure.Realtime;

/// <summary>
/// Đẩy thông báo realtime qua SignalR.
/// </summary>
public sealed class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    /// <summary>
    /// Khởi tạo dịch vụ đẩy thông báo.
    /// </summary>
    /// <param name="hubContext">Hub SignalR dùng để gửi sự kiện.</param>
    public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    /// <summary>
    /// Gửi thông báo tới group của người nhận.
    /// </summary>
    /// <param name="userId">Mã người nhận.</param>
    /// <param name="notification">Dữ liệu thông báo.</param>
    /// <param name="ct">Token hủy thao tác.</param>
    /// <returns>Task bất đồng bộ.</returns>
    public async Task PushAsync(string userId, NotificationDto notification, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification, ct);
    }
}
