using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.FollowUser;

/// <summary>
/// Handler xử lý <see cref="FollowUserCommand"/>.
/// Luồng xử lý: kiểm tra xác thực & quyền (FollowerId phải là người dùng hiện tại)
/// → kiểm tra tự follow → kiểm tra tồn tại → kiểm tra đã follow rồi chưa
/// → gọi <c>IncrementFollowers()</c> trên Entity → persist Entity → tạo bản ghi UserFollows.
/// Phân quyền: chỉ người dùng đã đăng nhập và đang thao tác cho chính mình.
/// </summary>
public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền.</param>
    public FollowUserCommandHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng theo dõi người dùng theo thứ tự:
    /// kiểm tra xác thực & quyền sở hữu → guard clauses → gọi method Entity → persist Entity → tạo bản ghi quan hệ.
    /// </summary>
    /// <param name="request">Command chứa FollowerId và FolloweeId.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu toàn bộ thao tác thành công.</returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu FollowerId khác với người dùng hiện tại.</exception>
    /// <exception cref="DomainException">
    /// Ném ra nếu: tự follow, User không tồn tại, hoặc đã follow rồi.
    /// </exception>
    public async Task<bool> Handle(FollowUserCommand request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi thực hiện theo dõi.");

        if (!currentUserId.Equals(request.FollowerId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền thực hiện theo dõi thay cho người dùng khác.");

        if (request.FollowerId == request.FolloweeId)
            throw new DomainException("Người dùng không thể tự theo dõi chính mình.");

        var follower = await _userRepository.GetByIdAsync(request.FollowerId, ct);
        var followee = await _userRepository.GetByIdAsync(request.FolloweeId, ct);

        if (follower is null)
            throw new DomainException($"Người dùng với Id '{request.FollowerId}' không tồn tại.");
        if (followee is null || !followee.IsActive)
            throw new DomainException("Không tìm thấy người dùng cần theo dõi hoặc tài khoản này hiện không còn hoạt động.");

        var alreadyFollowing = await _userRepository.IsFollowingAsync(request.FollowerId, request.FolloweeId, ct);
        if (alreadyFollowing)
            throw new DomainException("Bạn đã theo dõi người dùng này rồi.");

        // TotalFollowers sẽ được cập nhật bởi FollowRepository.
        return await _userRepository.FollowUserAsync(request.FollowerId, request.FolloweeId, ct);
    }
}
