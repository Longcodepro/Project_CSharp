using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetFollowing;

/// <summary>
/// Handler xử lý <see cref="GetFollowingQuery"/>.
/// Lấy tập hợp <see cref="TuneVault.Domain.Entities.User"/> Entity mà UserId đang theo dõi
/// và map từng Entity sang <see cref="UserDto"/> — không lộ thông tin nhạy cảm.
/// Phân quyền: chỉ Listener / Artist / Admin đã đăng nhập và UserId phải là chính người dùng hiện tại.
/// </summary>
public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền.</param>
    public GetFollowingQueryHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng lấy danh sách following:
    /// kiểm tra xác thực & quyền sở hữu → truy vấn tập hợp Entity → map từng Entity sang DTO → trả về.
    /// </summary>
    /// <param name="request">Query chứa UserId cần lấy danh sách following.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>
    /// Tập hợp <see cref="UserDto"/> đại diện cho những người UserId đang theo dõi.
    /// Trả về tập hợp rỗng nếu chưa theo dõi ai.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra nếu UserId khác với người dùng hiện tại.</exception>
    public async Task<IEnumerable<UserDto>> Handle(GetFollowingQuery request, CancellationToken ct)
    {
        // Step 0: Kiểm tra đã xác thực chưa
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập để xem danh sách following.");

        // Step 0.1: Chỉ cho phép người dùng xem danh sách following của chính mình
        if (!currentUserId.Equals(request.UserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền xem danh sách following của người dùng khác.");

        // Step 1: Truy vấn danh sách User Entity mà UserId đang theo dõi
        var following = await _userRepository.GetFollowingAsync(request.UserId, ct);

        // Step 2: Map từng Entity sang UserDto — ẩn Id hệ thống, Email, PasswordHash
        return following.Select(u => new UserDto
        {
            IdDisplay   = u.IdDisplay,
            DisplayName = u.DisplayName,
            AvatarUrl   = u.AvatarUrl,
            Role        = u.IsArtist ? "Artist" : "User",
            IsActive    = u.IsActive
        });
    }
}