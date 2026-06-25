using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Application.Features.Friend.DTOs;

namespace TuneVault.Application.Features.Friend.Queries.GetMyFriends;

/// <summary>
/// Query lấy danh sách bạn bè của người dùng hiện tại.
/// </summary>
public sealed record GetMyFriendsQuery(string CurrentUserId) : IRequest<IReadOnlyCollection<FriendDto>>;

/// <summary>
/// Handler lấy danh sách bạn bè.
/// </summary>
public sealed class GetMyFriendsQueryHandler : IRequestHandler<GetMyFriendsQuery, IReadOnlyCollection<FriendDto>>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler truy vấn danh sách bạn bè.
    /// </summary>
    public GetMyFriendsQueryHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Lấy danh sách bạn bè và map sang DTO trả về API.
    /// </summary>
    public async Task<IReadOnlyCollection<FriendDto>> Handle(GetMyFriendsQuery request, CancellationToken cancellationToken)
    {
        var friends = await _friendRepository.GetFriendsAsync(request.CurrentUserId, cancellationToken);
        return friends
            .Select(item => new FriendDto(item.UserId, item.IdDisplay, item.DisplayName, item.AvatarUrl, item.FriendsSince))
            .ToList();
    }
}
