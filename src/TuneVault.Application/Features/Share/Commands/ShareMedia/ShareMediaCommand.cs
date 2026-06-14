using TuneVault.Application.Features.Notification.Commands;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

public interface IMediaShareCommandRepository
{
    Task<string> CreateMediaShareAsync(
        string senderId,
        string receiverId,
        string shareType,
        string sharedItemId);

    Task<bool> TrackExistsAsync(string mediaItemId);

    Task<bool> AlbumExistsAsync(string albumId);

    Task<bool> PlaylistExistsAsync(string playlistId);

    Task<bool> MarkShareAsReadAsync(string shareId, string receiverId);
}

public sealed class ShareMediaCommand
{
    private readonly IMediaShareCommandRepository _mediaShareRepository;
    private readonly MarkNotificationAsReadCommand _notificationCommand;

    public ShareMediaCommand(
        IMediaShareCommandRepository mediaShareRepository,
        MarkNotificationAsReadCommand notificationCommand)
    {
        _mediaShareRepository = mediaShareRepository;
        _notificationCommand = notificationCommand;
    }

    public async Task<string> ShareTrackAsync(string senderId, string receiverId, string mediaItemId)
    {
        ValidateRequired(senderId, nameof(senderId));
        ValidateRequired(receiverId, nameof(receiverId));
        ValidateRequired(mediaItemId, nameof(mediaItemId));

        senderId = senderId.Trim();
        receiverId = receiverId.Trim();
        mediaItemId = mediaItemId.Trim();

        ValidateNotSelfShare(senderId, receiverId);

        var exists = await _mediaShareRepository.TrackExistsAsync(mediaItemId);
        if (!exists)
            throw new InvalidOperationException("Track không tồn tại");

        return await CreateShareWithNotificationAsync(
            senderId,
            receiverId,
            "Track",
            mediaItemId);
    }

    public async Task<string> ShareAlbumAsync(string senderId, string receiverId, string albumId)
    {
        ValidateRequired(senderId, nameof(senderId));
        ValidateRequired(receiverId, nameof(receiverId));
        ValidateRequired(albumId, nameof(albumId));

        senderId = senderId.Trim();
        receiverId = receiverId.Trim();
        albumId = albumId.Trim();

        ValidateNotSelfShare(senderId, receiverId);

        var exists = await _mediaShareRepository.AlbumExistsAsync(albumId);
        if (!exists)
            throw new InvalidOperationException("Album không tồn tại");

        return await CreateShareWithNotificationAsync(
            senderId,
            receiverId,
            "Album",
            albumId);
    }

    public async Task<string> SharePlaylistAsync(string senderId, string receiverId, string playlistId)
    {
        ValidateRequired(senderId, nameof(senderId));
        ValidateRequired(receiverId, nameof(receiverId));
        ValidateRequired(playlistId, nameof(playlistId));

        senderId = senderId.Trim();
        receiverId = receiverId.Trim();
        playlistId = playlistId.Trim();

        ValidateNotSelfShare(senderId, receiverId);

        var exists = await _mediaShareRepository.PlaylistExistsAsync(playlistId);
        if (!exists)
            throw new InvalidOperationException("Playlist không tồn tại");

        return await CreateShareWithNotificationAsync(
            senderId,
            receiverId,
            "Playlist",
            playlistId);
    }

    public async Task<bool> MarkAsReadAsync(string shareId, string receiverId)
    {
        ValidateRequired(shareId, nameof(shareId));
        ValidateRequired(receiverId, nameof(receiverId));

        return await _mediaShareRepository.MarkShareAsReadAsync(
            shareId.Trim(),
            receiverId.Trim());
    }

    private async Task<string> CreateShareWithNotificationAsync(
        string senderId,
        string receiverId,
        string shareType,
        string sharedItemId)
    {
        var shareId = await _mediaShareRepository.CreateMediaShareAsync(
            senderId,
            receiverId,
            shareType,
            sharedItemId);

        await _notificationCommand.CreateMediaSharedNotificationAsync(
            senderId,
            receiverId,
            shareType,
            sharedItemId,
            shareId);

        return shareId;
    }

    private static void ValidateNotSelfShare(string senderId, string receiverId)
    {
        if (senderId == receiverId)
            throw new InvalidOperationException("Không thể tự chia sẻ cho chính mình");
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
    }
}