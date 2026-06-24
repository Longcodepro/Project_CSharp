using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.Friend.Commands.CancelFriendRequest;

/// <summary>
/// Command hủy một lời mời kết bạn đã gửi.
/// </summary>
public sealed record CancelFriendRequestCommand(string CurrentUserId, string RequestId) : IRequest;

/// <summary>
/// Handler hủy lời mời kết bạn nếu người dùng hiện tại là người gửi.
/// </summary>
public sealed class CancelFriendRequestCommandHandler : IRequestHandler<CancelFriendRequestCommand>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler hủy lời mời.
    /// </summary>
    public CancelFriendRequestCommandHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Ẩn lời mời pending nếu người dùng hiện tại là người gửi.
    /// </summary>
    public async Task Handle(CancelFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var relation = await _friendRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (relation is null)
            throw new DomainException("Không tìm thấy lời mời kết bạn.");

        if (relation.Status != FriendStatus.Pending)
            throw new DomainException("Chỉ có thể hủy lời mời kết bạn đang chờ xử lý.");

        if (!string.Equals(relation.RequestedById, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Bạn không có quyền hủy lời mời kết bạn này.");

        await _friendRepository.DeletePendingRequestAsync(request.RequestId, cancellationToken);
    }
}
