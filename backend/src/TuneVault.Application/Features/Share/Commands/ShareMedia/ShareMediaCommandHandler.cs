using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Notification.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

/// <summary>
/// Handler tạo share, lưu notification và đẩy realtime notification qua abstraction.
/// </summary>
public sealed class ShareMediaCommandHandler : IRequestHandler<ShareMediaCommand, string>
{
    private readonly IMediaShareCommandRepository _mediaShareRepository;
    private readonly INotificationPusher _notificationPusher;

    public ShareMediaCommandHandler(
        IMediaShareCommandRepository mediaShareRepository,
        INotificationPusher notificationPusher)
    {
        _mediaShareRepository = mediaShareRepository ?? throw new ArgumentNullException(nameof(mediaShareRepository));
        _notificationPusher = notificationPusher ?? throw new ArgumentNullException(nameof(notificationPusher));
    }

    public async Task<string> Handle(ShareMediaCommand request, CancellationToken ct)
    {
        ValidateRequired(request.SenderId, nameof(request.SenderId));
        ValidateRequired(request.ReceiverId, nameof(request.ReceiverId));
        ValidateRequired(request.SharedItemId, nameof(request.SharedItemId));
        ValidateRequired(request.ShareType, nameof(request.ShareType));

        var senderId = request.SenderId.Trim();
        var receiverId = request.ReceiverId.Trim();
        var sharedItemId = request.SharedItemId.Trim();
        var shareType = ParseShareType(request.ShareType);
        var message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim();

        ValidateNotSelfShare(senderId, receiverId);

        if (!await _mediaShareRepository.UserExistsAsync(receiverId))
            throw new DomainException("Không tìm thấy người nhận chia sẻ.");

        bool exists = false;
        switch (shareType)
        {
            case ShareType.MediaItem:
                exists = await _mediaShareRepository.TrackExistsAsync(sharedItemId, senderId);
                break;
            case ShareType.Album:
                exists = await _mediaShareRepository.AlbumExistsAsync(sharedItemId, senderId);
                break;
            case ShareType.Playlist:
                exists = await _mediaShareRepository.PlaylistExistsAsync(sharedItemId, senderId);
                break;
            default:
                throw new DomainException("Loại chia sẻ không hợp lệ.");
        }

        if (!exists)
        {
            throw new DomainException(shareType switch
            {
                ShareType.Playlist => "Playlist không tồn tại hoặc chưa đủ điều kiện để chia sẻ.",
                ShareType.Album => "Album không tồn tại hoặc chưa đủ điều kiện để chia sẻ.",
                _ => "Media không tồn tại hoặc chưa đủ điều kiện để chia sẻ."
            });
        }

        var existingShareId = await _mediaShareRepository.FindExistingShareIdAsync(
            senderId,
            receiverId,
            shareType,
            sharedItemId,
            ct);
        if (!string.IsNullOrWhiteSpace(existingShareId))
        {
            return existingShareId;
        }

        var targetType = shareType switch
        {
            ShareType.MediaItem => NotificationTargetType.Media,
            ShareType.Album => NotificationTargetType.Album,
            ShareType.Playlist => NotificationTargetType.Playlist,
            _ => throw new DomainException("Loại chia sẻ không hợp lệ.")
        };

        var (title, defaultMessage) = BuildNotificationContent(shareType);

        var (shareId, notificationId) = await _mediaShareRepository.CreateMediaShareWithNotificationAsync(
            senderId,
            receiverId,
            shareType,
            sharedItemId,
            message,
            new NotificationInsertModel
            {
                UserId = receiverId,
                SenderId = senderId,
                NotifyType = NotificationType.MediaShared,
                Title = title,
                Message = message ?? defaultMessage,
                TargetType = targetType,
                TargetId = sharedItemId
            },
            ct);

        var notification = new NotificationDto(
            Id: notificationId,
            UserId: receiverId,
            Type: NotificationType.MediaShared.ToString(),
            TargetType: (int)targetType,
            TargetId: sharedItemId,
            PayloadJson: null,
            IsRead: false,
            CreatedAt: DateTime.UtcNow);

        await _notificationPusher.PushAsync(receiverId, notification, ct);

        return shareId;
    }

    private static (string Title, string DefaultMessage) BuildNotificationContent(ShareType shareType)
    {
        return shareType switch
        {
            ShareType.Playlist => ("Playlist được chia sẻ", "Bạn vừa nhận được một playlist được chia sẻ."),
            ShareType.Album => ("Album được chia sẻ", "Bạn vừa nhận được một album được chia sẻ."),
            _ => ("Media được chia sẻ", "Bạn vừa nhận được một bài hát hoặc video được chia sẻ.")
        };
    }

    private static void ValidateNotSelfShare(string senderId, string receiverId)
    {
        if (senderId == receiverId)
            throw new DomainException("Không thể tự chia sẻ cho chính mình.");
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{GetDisplayName(parameterName)} không được để trống.");
    }

    private static string GetDisplayName(string parameterName)
    {
        return parameterName switch
        {
            nameof(ShareMediaCommand.SenderId) => "Mã người gửi",
            nameof(ShareMediaCommand.ReceiverId) => "Mã người nhận",
            nameof(ShareMediaCommand.SharedItemId) => "Mã nội dung cần chia sẻ",
            nameof(ShareMediaCommand.ShareType) => "Loại nội dung chia sẻ",
            _ => parameterName
        };
    }

    private static ShareType ParseShareType(string shareType)
    {
        var normalized = shareType.Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();

        return normalized switch
        {
            "playlist" => ShareType.Playlist,
            "album" => ShareType.Album,
            "track" or "media" or "mediaitem" or "song" or "video" => ShareType.MediaItem,
            _ => throw new DomainException("Loại nội dung chia sẻ không hợp lệ.")
        };
    }
}
