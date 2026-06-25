using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.Friend.Commands.RejectFriendRequest;

/// <summary>
/// Command từ chối một lời mời kết bạn.
/// </summary>
public sealed record RejectFriendRequestCommand(string CurrentUserId, string RequestId) : IRequest;

/// <summary>
/// Handler từ chối lời mời kết bạn bằng cách ẩn bản ghi pending.
/// </summary>
public sealed class RejectFriendRequestCommandHandler : IRequestHandler<RejectFriendRequestCommand>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler từ chối lời mời.
    /// </summary>
    public RejectFriendRequestCommandHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Ẩn lời mời pending nếu người dùng hiện tại là người nhận.
    /// </summary>
    public async Task Handle(RejectFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var relation = await _friendRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (relation is null)
            throw new DomainException("Không tìm thấy lời mời kết bạn.");

        if (relation.Status != FriendStatus.Pending)
            throw new DomainException("Lời mời kết bạn này không còn ở trạng thái chờ xử lý.");

        if (!string.Equals(relation.RequestedToId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Bạn không có quyền từ chối lời mời kết bạn này.");

        await _friendRepository.DeletePendingRequestAsync(request.RequestId, cancellationToken);
    }
}
