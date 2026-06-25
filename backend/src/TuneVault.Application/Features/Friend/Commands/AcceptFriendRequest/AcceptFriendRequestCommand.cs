using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.Friend.Commands.AcceptFriendRequest;

/// <summary>
/// Command chấp nhận một lời mời kết bạn.
/// </summary>
public sealed record AcceptFriendRequestCommand(string CurrentUserId, string RequestId) : IRequest;

/// <summary>
/// Handler chấp nhận lời mời kết bạn nếu người dùng hiện tại là người nhận.
/// </summary>
public sealed class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler chấp nhận lời mời.
    /// </summary>
    public AcceptFriendRequestCommandHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Chấp nhận lời mời đang pending.
    /// </summary>
    public async Task Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var relation = await _friendRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (relation is null)
            throw new DomainException("Không tìm thấy lời mời kết bạn.");

        if (relation.Status != FriendStatus.Pending)
            throw new DomainException("Lời mời kết bạn này không còn ở trạng thái chờ xử lý.");

        if (!string.Equals(relation.RequestedToId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Bạn không có quyền chấp nhận lời mời kết bạn này.");

        await _friendRepository.AcceptRequestAsync(request.RequestId, cancellationToken);
    }
}
