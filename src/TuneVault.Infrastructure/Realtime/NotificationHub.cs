using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TuneVault.Infrastructure.Realtime;

/// <summary>
/// SignalR Hub dùng để đẩy thông báo real-time tới client.
/// Mỗi user kết nối sẽ join vào một group có tên = UserId của họ.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
    /// <summary>
    /// Tự động đưa connection vào group user hiện tại khi kết nối bằng JWT hợp lệ.
    /// </summary>
    /// <returns>Task bất đồng bộ của SignalR.</returns>
    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Cho phép client tham gia group riêng theo UserId để nhận thông báo cá nhân.
    /// </summary>
    /// <param name="userId">Mã định danh người dùng hiện tại (lấy từ JWT ở client).</param>
    public async Task JoinUserGroup(string userId)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId) || currentUserId != userId)
        {
            throw new HubException("Bạn không có quyền tham gia nhóm thông báo này.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, currentUserId);
    }

    private string? GetCurrentUserId() =>
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Context.User?.FindFirstValue("sub");
}
