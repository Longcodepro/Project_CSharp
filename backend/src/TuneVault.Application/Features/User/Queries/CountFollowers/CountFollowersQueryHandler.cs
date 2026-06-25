using MediatR;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.CountFollowers;

/// <summary>
/// Handler for the <see cref="CountFollowersQuery"/>.
/// </summary>
public sealed class CountFollowersQueryHandler : IRequestHandler<CountFollowersQuery, int>
{
    private readonly IFollowRepository _followRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CountFollowersQueryHandler"/> class.
    /// </summary>
    /// <param name="followRepository">The follow repository.</param>
    public CountFollowersQueryHandler(IFollowRepository followRepository)
    {
        _followRepository = followRepository ?? throw new ArgumentNullException(nameof(followRepository));
    }

    /// <summary>
    /// Handles the query to count followers for a specific user.
    /// </summary>
    /// <param name="request">The query containing the user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of followers, or -1 if the user does not exist.</returns>
    public async Task<int> Handle(CountFollowersQuery request, CancellationToken cancellationToken)
    {
        // Bước 1: Gọi repository để đếm số lượng follower.
        // Repository sẽ đếm số bản ghi trong bảng Follow có FolloweeId = request.UserId.
        var followerCount = await _followRepository.CountFollowersAsync(request.UserId);
        return followerCount;
    }
}
