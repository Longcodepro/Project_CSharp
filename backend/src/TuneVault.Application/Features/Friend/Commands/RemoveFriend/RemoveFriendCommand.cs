using MediatR;
using TuneVault.Application.Features.Friend.Abstractions;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.Friend.Commands.RemoveFriend;

/// <summary>
/// Command xóa một quan hệ bạn bè đã được chấp nhận.
/// </summary>
public sealed record RemoveFriendCommand(string CurrentUserId, string FriendUserId) : IRequest;

/// <summary>
/// Handler xóa quan hệ bạn bè giữa hai người dùng.
/// </summary>
public sealed class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand>
{
    private readonly IFriendRepository _friendRepository;

    /// <summary>
    /// Khởi tạo handler xóa bạn.
    /// </summary>
    public RemoveFriendCommandHandler(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository ?? throw new ArgumentNullException(nameof(friendRepository));
    }

    /// <summary>
    /// Xóa quan hệ bạn bè hiện có nếu hai user đang là bạn.
    /// </summary>
    public async Task Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FriendUserId))
            throw new DomainException("Mã người bạn cần xóa không được để trống.");

        if (string.Equals(request.CurrentUserId, request.FriendUserId, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("Bạn không thể xóa chính mình khỏi danh sách bạn bè.");

        var relation = await _friendRepository.GetRelationshipAsync(request.CurrentUserId, request.FriendUserId, cancellationToken);
        if (relation is null || relation.Status != FriendStatus.Accepted)
            throw new DomainException("Hai người dùng này hiện không phải bạn bè.");

        await _friendRepository.DeleteAcceptedFriendshipAsync(request.CurrentUserId, request.FriendUserId, cancellationToken);
    }
}
