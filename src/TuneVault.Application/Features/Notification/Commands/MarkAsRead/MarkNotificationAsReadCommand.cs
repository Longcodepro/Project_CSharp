using System;
using System.Threading.Tasks;

namespace TuneVault.Application.Features.Notification.Commands
{
    public interface INotificationCommandRepository
    {
        Task<string> InsertNotificationAsync(NotificationInsertModel notification);

        Task<bool> MarkAsReadAsync(string notificationId, string userId);

        Task<bool> DeleteAsync(string notificationId, string userId);

        Task<int> DeleteAllAsync(string userId);

        Task<UserBrief?> GetUserBriefAsync(string userId);

        Task<string?> GetMediaTitleAsync(string mediaItemId);

        Task<string?> GetAlbumTitleAsync(string albumId);

        Task<string?> GetPlaylistTitleAsync(string playlistId);
    }

    public sealed class MarkNotificationAsReadCommand
    {
        private readonly INotificationCommandRepository _notificationRepository;

        public MarkNotificationAsReadCommand(INotificationCommandRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<string> CreateNotificationAsync(
            string userId,
            string type,
            string message,
            string? senderId = null,
            string? customTitle = null)
        {
            ValidateRequired(userId, nameof(userId));
            ValidateRequired(type, nameof(type));
            ValidateRequired(message, nameof(message));

            var notification = new NotificationInsertModel
            {
                UserId = userId.Trim(),
                SenderId = NormalizeNullable(senderId),
                NotifyType = ToNotificationType(type),
                Title = Limit(customTitle ?? GetNotificationTitle(type), 200),
                Message = Limit(message.Trim(), 500)
            };

            return await _notificationRepository.InsertNotificationAsync(notification);
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            ValidateRequired(notificationId, nameof(notificationId));
            ValidateRequired(userId, nameof(userId));

            return await _notificationRepository.MarkAsReadAsync(
                notificationId.Trim(),
                userId.Trim());
        }

        public async Task<bool> DeleteAsync(string notificationId, string userId)
        {
            ValidateRequired(notificationId, nameof(notificationId));
            ValidateRequired(userId, nameof(userId));

            return await _notificationRepository.DeleteAsync(
                notificationId.Trim(),
                userId.Trim());
        }

        public async Task<int> DeleteAllAsync(string userId)
        {
            ValidateRequired(userId, nameof(userId));

            return await _notificationRepository.DeleteAllAsync(userId.Trim());
        }

        public async Task<string> CreateMediaSharedNotificationAsync(
            string senderId,
            string receiverId,
            string shareType,
            string sharedItemId,
            string shareId)
        {
            ValidateRequired(senderId, nameof(senderId));
            ValidateRequired(receiverId, nameof(receiverId));
            ValidateRequired(shareType, nameof(shareType));
            ValidateRequired(sharedItemId, nameof(sharedItemId));
            ValidateRequired(shareId, nameof(shareId));

            var sender = await _notificationRepository.GetUserBriefAsync(senderId.Trim());
            var sharedItem = await GetSharedItemBriefAsync(shareType.Trim(), sharedItemId.Trim());

            var senderName = string.IsNullOrWhiteSpace(sender?.DisplayName)
                ? "Một người dùng"
                : sender.DisplayName;

            var safeTitle = string.IsNullOrWhiteSpace(sharedItem.Title)
                ? sharedItemId.Trim()
                : sharedItem.Title;

            var message = $"{senderName} đã chia sẻ {sharedItem.KindText} \"{safeTitle}\" với bạn.";

            return await CreateNotificationAsync(
                receiverId,
                "MediaShared",
                message,
                senderId);
        }

        public async Task<string> CreateNewFollowerNotificationAsync(
            string followerId,
            string followeeId)
        {
            ValidateRequired(followerId, nameof(followerId));
            ValidateRequired(followeeId, nameof(followeeId));

            var follower = await _notificationRepository.GetUserBriefAsync(followerId.Trim());

            var followerName = string.IsNullOrWhiteSpace(follower?.DisplayName)
                ? "Một người dùng"
                : follower.DisplayName;

            var followerIdDisplay = string.IsNullOrWhiteSpace(follower?.IdDisplay)
                ? null
                : follower.IdDisplay;

            var displayText = followerIdDisplay == null
                ? followerName
                : $"{followerName} ({followerIdDisplay})";

            var message = $"{displayText} đã theo dõi bạn.";

            return await CreateNotificationAsync(
                followeeId,
                "NewFollower",
                message,
                followerId);
        }

        public async Task<string> CreateArtistNewMediaNotificationAsync(
            string userId,
            string artistId,
            string mediaItemId,
            string title)
        {
            ValidateRequired(userId, nameof(userId));
            ValidateRequired(artistId, nameof(artistId));
            ValidateRequired(mediaItemId, nameof(mediaItemId));
            ValidateRequired(title, nameof(title));

            var artist = await _notificationRepository.GetUserBriefAsync(artistId.Trim());

            var artistName = string.IsNullOrWhiteSpace(artist?.DisplayName)
                ? "Nghệ sĩ bạn theo dõi"
                : artist.DisplayName;

            var message = $"{artistName} vừa đăng \"{title.Trim()}\".";

            return await CreateNotificationAsync(
                userId,
                "ArtistNewMedia",
                message,
                artistId);
        }

        public async Task<string> CreateSystemAlertAsync(
            string userId,
            string title,
            string message,
            string? senderId = null)
        {
            ValidateRequired(userId, nameof(userId));
            ValidateRequired(title, nameof(title));
            ValidateRequired(message, nameof(message));

            return await CreateNotificationAsync(
                userId,
                "SystemAlert",
                message,
                senderId,
                title);
        }

        private async Task<SharedItemBrief> GetSharedItemBriefAsync(string shareType, string sharedItemId)
        {
            if (string.Equals(shareType, "Track", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(shareType, "MediaItem", StringComparison.OrdinalIgnoreCase))
            {
                var title = await _notificationRepository.GetMediaTitleAsync(sharedItemId);
                return new SharedItemBrief("bài hát", title);
            }

            if (string.Equals(shareType, "Album", StringComparison.OrdinalIgnoreCase))
            {
                var title = await _notificationRepository.GetAlbumTitleAsync(sharedItemId);
                return new SharedItemBrief("album", title);
            }

            if (string.Equals(shareType, "Playlist", StringComparison.OrdinalIgnoreCase))
            {
                var title = await _notificationRepository.GetPlaylistTitleAsync(sharedItemId);
                return new SharedItemBrief("playlist", title);
            }

            return new SharedItemBrief("nội dung", null);
        }

        private static byte ToNotificationType(string type)
        {
            return type.Trim().ToLowerInvariant() switch
            {
                "newfollower" => 1,
                "friendrequest" => 2,
                "mediashared" => 3,
                "systemalert" => 4,
                "friendaccepted" => 5,
                "artistnewmedia" => 6,
                _ => 4
            };
        }

        private static string GetNotificationTitle(string type)
        {
            return type.Trim().ToLowerInvariant() switch
            {
                "newfollower" => "Người theo dõi mới",
                "friendrequest" => "Lời mời kết bạn",
                "mediashared" => "Nội dung được chia sẻ",
                "systemalert" => "Thông báo hệ thống",
                "friendaccepted" => "Lời mời kết bạn đã được chấp nhận",
                "artistnewmedia" => "Nghệ sĩ vừa đăng nội dung mới",
                _ => "Thông báo"
            };
        }

        private static void ValidateRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
        }

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string Limit(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length <= maxLength
                ? value
                : value[..maxLength];
        }
    }

    public sealed class NotificationInsertModel
    {
        public string UserId { get; set; } = string.Empty;

        public string? SenderId { get; set; }

        public byte NotifyType { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    public sealed class UserBrief
    {
        public string? DisplayName { get; set; }

        public string? IdDisplay { get; set; }

        public string? AvatarUrl { get; set; }
    }

    public sealed class SharedItemBrief
    {
        public string KindText { get; }

        public string? Title { get; }

        public SharedItemBrief(string kindText, string? title)
        {
            KindText = kindText;
            Title = title;
        }
    }
}