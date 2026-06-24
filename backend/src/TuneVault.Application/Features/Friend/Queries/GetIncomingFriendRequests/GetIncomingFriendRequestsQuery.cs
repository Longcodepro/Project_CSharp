using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Application.Features.Friend.DTOs;

namespace TuneVault.Application.Features.Friend.Queries.GetIncomingFriendRequests;

/// <summary>
/// Query lấy danh sách lời mời kết bạn người dùng hiện tại nhận được.
/// </summary>
public sealed record GetIncomingFriendRequestsQuery(string CurrentUserId) : IRequest<IReadOnlyCollection<FriendRequestDto>>;

/// <summary>
/// Handler lấy danh sách lời mời kết bạn nhận được.
/// </summary>
public sealed class GetIncomingFriendRequestsQueryHandler : IRequestHandler<GetIncomingFriendRequestsQuery, IReadOnlyCollection<FriendRequestDto>>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler truy vấn inbox lời mời kết bạn.
    /// </summary>
    public GetIncomingFriendRequestsQueryHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Lấy danh sách lời mời nhận được và map sang DTO.
    /// </summary>
    public async Task<IReadOnlyCollection<FriendRequestDto>> Handle(GetIncomingFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _friendRepository.GetIncomingRequestsAsync(request.CurrentUserId, cancellationToken);
        return requests
            .Select(item => new FriendRequestDto(item.RequestId, item.UserId, item.IdDisplay, item.DisplayName, item.AvatarUrl, item.RequestedAt, item.Direction))
            .ToList();
    }
}
