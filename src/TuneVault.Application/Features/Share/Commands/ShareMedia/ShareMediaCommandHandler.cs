using MediatR;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Features.Notification.Commands;
using TuneVault.Application.Features.Notification.DTOs;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

/// <summary>
/// Handler tạo share, lưu notification và đẩy realtime notification qua abstraction.
/// </summary>
public sealed class ShareMediaCommandHandler : IRequestHandler<ShareMediaCommand, string>
{
    private readonly IMediaShareCommandRepository _mediaShareRepository;
    private readonly INotificationCommandRepository _notificationRepository;
    private readonly INotificationPusher _notificationPusher;

    public ShareMediaCommandHandler(
        IMediaShareCommandRepository mediaShareRepository,
        INotificationCommandRepository notificationRepository,
        INotificationPusher notificationPusher)
    {
        _mediaShareRepository = mediaShareRepository ?? throw new ArgumentNullException(nameof(mediaShareRepository));
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
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
        var shareType = NormalizeShareType(request.ShareType);
        var message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim();

        ValidateNotSelfShare(senderId, receiverId);

        if (!await _mediaShareRepository.UserExistsAsync(receiverId))
            throw new DomainException("Không tìm thấy người nhận chia sẻ.");

        bool exists = false;
        switch (shareType)
        {
            case "Track":
                exists = await _mediaShareRepository.TrackExistsAsync(sharedItemId, senderId);
                break;
            case "Album":
                exists = await _mediaShareRepository.AlbumExistsAsync(sharedItemId, senderId);
                break;
            case "Playlist":
                exists = await _mediaShareRepository.PlaylistExistsAsync(sharedItemId, senderId);
                break;
            default:
                throw new DomainException("Loại chia sẻ không hợp lệ.");
        }

        if (!exists)
        {
            throw new DomainException(shareType == "Playlist"
                ? "Không tìm thấy playlist cần chia sẻ."
                : "Không tìm thấy media cần chia sẻ.");
        }

        var shareId = await _mediaShareRepository.CreateMediaShareAsync(
            senderId,
            receiverId,
            shareType,
            sharedItemId,
            message);

        var targetType = shareType switch
        {
            "Track" => 1,
            "Album" => 2,
            "Playlist" => 3,
            _ => (int?)null
        };

        var notificationId = await _notificationRepository.InsertNotificationAsync(new NotificationInsertModel
        {
            UserId = receiverId,
            SenderId = senderId,
            NotifyType = 3,
            Title = "Nội dung được chia sẻ",
            Message = message ?? (shareType == "Playlist"
                ? "Bạn nhận được một playlist mới."
                : "Bạn nhận được một bài hát hoặc video mới."),
            TargetType = targetType,
            TargetId = sharedItemId
        });

        var notification = new NotificationDto(
            Id: notificationId,
            UserId: receiverId,
            Type: "MediaShared",
            TargetType: targetType,
            TargetId: sharedItemId,
            PayloadJson: null,
            IsRead: false,
            CreatedAt: DateTime.UtcNow);

        await _notificationPusher.PushAsync(receiverId, notification, ct);

        return shareId;
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

    private static string NormalizeShareType(string shareType)
    {
        return shareType.Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant() switch
            {
                "playlist" => "Playlist",
                "album" => "Album",
                "track" or "media" or "mediaitem" or "song" or "video" => "Track",
                _ => string.Empty
            };
    }
}
