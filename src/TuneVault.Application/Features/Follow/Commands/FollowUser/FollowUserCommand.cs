using TuneVault.Application.Features.Notification.Commands;

namespace TuneVault.Application.Features.Follow.Commands
{
    /// <summary>
    /// Interface repository riêng cho SQL Follow.
    /// Để chung trong file FollowUserCommand.cs để không cần tạo file mới.
    /// </summary>
    public interface IFollowSqlRepository
    {
        Task<bool> FollowAsync(string followerId, string followeeId);

        Task<bool> UnfollowAsync(string followerId, string followeeId);

        Task<bool> IsFollowingAsync(string followerId, string followeeId);

        Task<IEnumerable<dynamic>> GetFollowingAsync(string followerId);

        Task<IEnumerable<dynamic>> GetFollowersAsync(string followeeId);

        Task<int> CountFollowersAsync(string followeeId);
    }

    /// <summary>
    /// Command xử lý nghiệp vụ Follow.
    /// Không viết SQL trực tiếp ở đây.
    /// </summary>
    public sealed class FollowUserCommand
    {
        private readonly IFollowSqlRepository _followRepository;
        private readonly MarkNotificationAsReadCommand _markNotificationAsReadCommand;

        public FollowUserCommand(
            IFollowSqlRepository followRepository,
            MarkNotificationAsReadCommand markNotificationAsReadCommand)
        {
            _followRepository = followRepository;
            _markNotificationAsReadCommand = markNotificationAsReadCommand;
        }

        public async Task<bool> ExecuteAsync(string followerId, string followeeId)
        {
            ValidateRequired(followerId, nameof(followerId));
            ValidateRequired(followeeId, nameof(followeeId));

            followerId = followerId.Trim();
            followeeId = followeeId.Trim();

            if (followerId == followeeId)
                throw new InvalidOperationException("Không thể tự follow chính mình.");

            var result = await _followRepository.FollowAsync(followerId, followeeId);

            if (!result)
                return false;

            await _markNotificationAsReadCommand.CreateNewFollowerNotificationAsync(
                followerId,
                followeeId);

            return true;
        }

        private static void ValidateRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
        }
    }
}