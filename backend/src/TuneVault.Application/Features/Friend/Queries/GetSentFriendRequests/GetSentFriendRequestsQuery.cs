using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Application.Features.Friend.DTOs;

namespace TuneVault.Application.Features.Friend.Queries.GetSentFriendRequests;

/// <summary>
/// Query lấy danh sách lời mời kết bạn người dùng hiện tại đã gửi.
/// </summary>
public sealed record GetSentFriendRequestsQuery(string CurrentUserId) : IRequest<IReadOnlyCollection<FriendRequestDto>>;

/// <summary>
/// Handler lấy danh sách lời mời kết bạn đã gửi.
/// </summary>
public sealed class GetSentFriendRequestsQueryHandler : IRequestHandler<GetSentFriendRequestsQuery, IReadOnlyCollection<FriendRequestDto>>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler truy vấn danh sách lời mời đã gửi.
    /// </summary>
    public GetSentFriendRequestsQueryHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Lấy danh sách lời mời đã gửi và map sang DTO.
    /// </summary>
    public async Task<IReadOnlyCollection<FriendRequestDto>> Handle(GetSentFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _friendRepository.GetSentRequestsAsync(request.CurrentUserId, cancellationToken);
        return requests
            .Select(item => new FriendRequestDto(item.RequestId, item.UserId, item.IdDisplay, item.DisplayName, item.AvatarUrl, item.RequestedAt, item.Direction))
            .ToList();
    }
}
