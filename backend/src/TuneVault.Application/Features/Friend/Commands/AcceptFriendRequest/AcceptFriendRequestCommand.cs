using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Notification.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Friend.Commands.AcceptFriendRequest;

/// <summary>
/// Command chấp nhận một lời mời kết bạn.
/// </summary>
public sealed record AcceptFriendRequestCommand(string CurrentUserId, string RequestId) : IRequest;

/// <summary>
/// Handler chấp nhận lời mời kết bạn nếu người dùng hiện tại là người nhận.
/// </summary>
public sealed class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand>
{
    private readonly IFriendRepository _friendRepository;
    private readonly INotificationCommandRepository _notificationRepository;
    private readonly INotificationPusher _notificationPusher;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo handler chấp nhận lời mời.
    /// </summary>
    public AcceptFriendRequestCommandHandler(
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
    /// Chấp nhận lời mời đang pending.
    /// </summary>
    public async Task Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var relation = await _friendRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (relation is null)
            throw new DomainException("Không tìm thấy lời mời kết bạn.");

        if (relation.Status != FriendStatus.Pending)
            throw new DomainException("Lời mời kết bạn này không còn ở trạng thái chờ xử lý.");

        if (!string.Equals(relation.RequestedToId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Bạn không có quyền chấp nhận lời mời kết bạn này.");

        await _friendRepository.AcceptRequestAsync(request.RequestId, cancellationToken);

        var accepter = await _userRepository.GetByIdAsync(request.CurrentUserId, cancellationToken);
        var notification = new NotificationInsertModel
        {
            UserId = relation.RequestedById,
            SenderId = request.CurrentUserId,
            NotifyType = NotificationType.FriendAccepted,
            Title = NotificationType.FriendAccepted.ToTitle(),
            Message = accepter is null
                ? "Lời mời kết bạn của bạn đã được chấp nhận."
                : $"{accepter.DisplayName} đã chấp nhận lời mời kết bạn của bạn.",
        };

        var notificationId = await _notificationRepository.InsertNotificationAsync(notification);
        await _notificationPusher.PushAsync(relation.RequestedById, new NotificationDto(
            Id: notificationId,
            UserId: relation.RequestedById,
            SenderId: request.CurrentUserId,
            SenderIdDisplay: accepter?.IdDisplay,
            SenderDisplayName: accepter?.DisplayName,
            SenderAvatarUrl: accepter?.AvatarUrl,
            Type: NotificationType.FriendAccepted.ToString(),
            Title: notification.Title,
            Message: notification.Message,
            TargetType: null,
            TargetId: null,
            PayloadJson: null,
            IsRead: false,
            CreatedAt: DateTime.UtcNow), cancellationToken);
    }
}
