namespace TuneVault.Application.Features.Follow.Commands
{
    /// <summary>
    /// Command xử lý nghiệp vụ Unfollow.
    /// Không viết SQL trực tiếp ở đây.
    /// </summary>
    public sealed class UnFollowUserCommand
    {
        private readonly IFollowSqlRepository _followRepository;

        public UnFollowUserCommand(IFollowSqlRepository followRepository)
        {
            _followRepository = followRepository;
        }

        public async Task<bool> ExecuteAsync(string followerId, string followeeId)
        {
            ValidateRequired(followerId, nameof(followerId));
            ValidateRequired(followeeId, nameof(followeeId));

            followerId = followerId.Trim();
            followeeId = followeeId.Trim();

            if (followerId == followeeId)
                throw new InvalidOperationException("Không thể tự unfollow chính mình.");

            return await _followRepository.UnfollowAsync(followerId, followeeId);
        }

        private static void ValidateRequired(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
        }
    }
}