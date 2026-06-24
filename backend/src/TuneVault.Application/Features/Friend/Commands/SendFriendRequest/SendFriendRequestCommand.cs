using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.Friend.Commands.SendFriendRequest;

/// <summary>
/// Command gửi lời mời kết bạn tới một người dùng khác.
/// </summary>
public sealed record SendFriendRequestCommand(string CurrentUserId, string ReceiverId) : IRequest<string>;

/// <summary>
/// Handler xử lý gửi lời mời kết bạn và chặn duplicate theo cả hai chiều.
/// </summary>
public sealed class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, string>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler gửi lời mời kết bạn.
    /// </summary>
    public SendFriendRequestCommandHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Gửi lời mời kết bạn mới nếu chưa có quan hệ hoặc lời mời trùng.
    /// </summary>
    public async Task<string> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentUserId))
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để gửi lời mời kết bạn.");

        if (string.IsNullOrWhiteSpace(request.ReceiverId))
            throw new DomainException("Mã người nhận lời mời không được để trống.");

        if (string.Equals(request.CurrentUserId, request.ReceiverId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Bạn không thể tự gửi lời mời kết bạn cho chính mình.");

        if (!await _friendRepository.UserExistsAsync(request.ReceiverId, cancellationToken))
            throw new DomainException("Không tìm thấy người dùng cần kết bạn.");

        var existing = await _friendRepository.GetRelationshipAsync(request.CurrentUserId, request.ReceiverId, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == FriendStatus.Accepted)
                throw new DomainException("Hai người dùng này đã là bạn bè.");

            if (existing.Status == FriendStatus.Pending)
            {
                if (string.Equals(existing.RequestedById, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
                    throw new DomainException("Bạn đã gửi lời mời kết bạn cho người dùng này rồi.");

                throw new DomainException("Người dùng này đã gửi lời mời kết bạn cho bạn. Hãy vào danh sách lời mời để chấp nhận.");
            }
        }

        return await _friendRepository.CreateRequestAsync(request.CurrentUserId, request.ReceiverId, cancellationToken);
    }
}
