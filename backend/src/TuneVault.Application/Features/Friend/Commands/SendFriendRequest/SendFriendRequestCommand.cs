using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Notification.DTOs;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Friend.Commands.SendFriendRequest;

/// <summary>
/// Command gửi lời mời kết bạn tới một người dùng khác.
/// </summary>
public sealed record SendFriendRequestCommand(string CurrentUserId, string ReceiverId) : IRequest<string>;

/// <summary>
/// Handler xử lý gửi lời mời kết bạn và chặn duplicate theo cả hai chiều.
/// </summary>
public sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, string>
{
    private readonly IFriendRepository _friendRepository;
    private readonly INotificationCommandRepository _notificationRepository;
    private readonly INotificationPusher _notificationPusher;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo handler gửi lời mời kết bạn.
    /// </summary>
    public SendFriendRequestCommandHandler(
        IFriendRepository friendRepository,
        INotificationCommandRepository notificationRepository,
        INotificationPusher notificationPusher,
        IUserRepository userRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _notificationPusher = notificationPusher ?? throw new ArgumentNullException(nameof(notificationPusher));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Gửi lời mời kết bạn mới nếu chưa có quan hệ hoặc lời mời trùng.
    /// </summary>
    public async Task<string> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để gửi lời mời kết bạn.");

        if (string.IsNullOrWhiteSpace(request.ReceiverId))
            throw new DomainException("Mã người nhận lời mời không được để trống.");

        if (string.Equals(request.CurrentUserId, request.ReceiverId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Bạn không thể tự gửi lời mời kết bạn cho chính mình.");

        if (!await _friendRepository.UserExistsAsync(request.ReceiverId, cancellationToken))
            throw new DomainException("Không tìm thấy người dùng cần kết bạn.");

        var existing = await _friendRepository.GetRelationshipAsync(request.CurrentUserId, request.ReceiverId, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == FriendStatus.Accepted)
                throw new DomainException("Hai người dùng này đã là bạn bè.");

            if (existing.Status == FriendStatus.Pending)
            {
                if (string.Equals(existing.RequestedById, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                    throw new DomainException("Bạn đã gửi lời mời kết bạn cho người dùng này rồi.");

                throw new DomainException("Người dùng này đã gửi lời mời kết bạn cho bạn. Hãy vào danh sách lời mời để chấp nhận.");
            }
        }

        var requestId = await _friendRepository.CreateRequestAsync(request.CurrentUserId, request.ReceiverId, cancellationToken);

        var sender = await _userRepository.GetByIdAsync(request.CurrentUserId, cancellationToken);
        var notification = new NotificationInsertModel
        {
            UserId = request.ReceiverId,
            SenderId = request.CurrentUserId,
            NotifyType = NotificationType.FriendRequest,
            Title = NotificationType.FriendRequest.ToTitle(),
            Message = sender is null
                ? "Bạn có một lời mời kết bạn mới."
                : $"{sender.DisplayName} đã gửi lời mời kết bạn cho bạn.",
        };

        var notificationId = await _notificationRepository.InsertNotificationAsync(notification);
        await _notificationPusher.PushAsync(request.ReceiverId, new NotificationDto(
            Id: notificationId,
            UserId: request.ReceiverId,
            SenderId: request.CurrentUserId,
            SenderIdDisplay: sender?.IdDisplay,
            SenderDisplayName: sender?.DisplayName,
            SenderAvatarUrl: sender?.AvatarUrl,
            Type: NotificationType.FriendRequest.ToString(),
            Title: notification.Title,
            Message: notification.Message,
            TargetType: null,
            TargetId: null,
            PayloadJson: null,
            IsRead: false,
            CreatedAt: DateTime.UtcNow), cancellationToken);

        return requestId;
    }
}
