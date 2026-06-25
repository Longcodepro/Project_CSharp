using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Commands.UnfollowUser;

/// <summary>
/// Handler xử lý <see cref="UnfollowUserCommand"/>.
/// Luồng xử lý: kiểm tra quan hệ follow tồn tại → gọi <c>DecrementFollowers()</c>
/// trên Entity followee → persist Entity → xóa bản ghi UserFollows.
/// </summary>
public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền.</param>
    public UnfollowUserCommandHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng hủy theo dõi người dùng theo thứ tự:
    /// guard clauses → kiểm tra quan hệ → gọi method Entity → persist Entity → xóa bản ghi quan hệ.
    /// </summary>
    /// <param name="request">Command chứa FollowerId và FolloweeId.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu toàn bộ thao tác thành công.</returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu FollowerId khác với người dùng hiện tại.</exception>
    /// <exception cref="DomainException">
    /// Ném ra nếu: User không tồn tại, hoặc quan hệ follow không tồn tại.
    /// </exception>
    public async Task<bool> Handle(UnfollowUserCommand request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập trước khi bỏ theo dõi.");

        if (!currentUserId.Equals(request.FollowerId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền thực hiện bỏ theo dõi thay cho người dùng khác.");

        if (request.FollowerId == request.FolloweeId)
            throw new DomainException("Người dùng không thể tự bỏ theo dõi chính mình.");

        // Step 1: Lấy Entity của người được theo dõi (cần để giảm TotalFollowers)
        var followee = await _userRepository.GetByIdAsync(request.FolloweeId, ct);

        // Step 2: Kiểm tra sự tồn tại của followee
        if (followee is null || !followee.IsActive)
            throw new DomainException("Không tìm thấy người dùng cần bỏ theo dõi hoặc tài khoản này hiện không còn hoạt động.");

        // Step 3: Kiểm tra quan hệ follow có tồn tại không — tránh thao tác vô nghĩa
        var isFollowing = await _userRepository.IsFollowingAsync(request.FollowerId, request.FolloweeId, ct);
        if (!isFollowing)
            throw new DomainException("Bạn chưa theo dõi người dùng này.");

        // Step 4: Xóa bản ghi quan hệ follow khỏi bảng UserFollows và trả về kết quả.
        // TotalFollowers sẽ được cập nhật bởi FollowRepository.
        return await _userRepository.UnfollowUserAsync(request.FollowerId, request.FolloweeId, ct);
    }
}
