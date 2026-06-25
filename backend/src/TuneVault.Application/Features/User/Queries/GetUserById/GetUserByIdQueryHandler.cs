using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetUserById;

/// <summary>
/// Handler xử lý <see cref="GetUserByIdQuery"/>.
/// Lấy <see cref="TuneVault.Domain.Entities.User"/> Entity và map sang <see cref="UserPublicDetailDto"/>
/// để trả về hồ sơ công khai mà không lộ dữ liệu nhạy cảm.
/// Trả về <c>null</c> nếu không tìm thấy (soft-not-found — cho phép caller xử lý tùy ý).
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserPublicDetailDto?>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng lấy thông tin cơ bản người dùng theo Id:
    /// kiểm tra xác thực → truy vấn Entity → kiểm tra null → map sang DTO → trả về.
    /// </summary>
    /// <param name="request">Query chứa Id của người dùng cần tìm.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><see cref="UserPublicDetailDto"/> nếu tìm thấy; <c>null</c> nếu không tồn tại hoặc đã bị khóa.</returns>
    public async Task<UserPublicDetailDto?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        // Trang hồ sơ công khai có thể được xem bởi người chưa đăng nhập,
        // nhưng không trả về tài khoản đã bị vô hiệu hóa.
        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        if (user is null || !user.IsActive)
            return null;

        var followerCount = (await _userRepository.GetFollowersAsync(user.Id, ct)).Count();
        var followingCount = (await _userRepository.GetFollowingAsync(user.Id, ct)).Count();

        return new UserPublicDetailDto
        {
            IdDisplay      = user.IdDisplay,
            DisplayName    = user.DisplayName,
            AvatarUrl      = user.AvatarUrl,
            Bio            = user.Bio,
            Email          = user.Email,
            Role           = user.IsArtist ? "Artist" : "User",
            TotalFollowers = followerCount,
            FollowingCount = followingCount,
            CreatedAt      = user.CreatedAt,
            IsActive       = user.IsActive
        };
    }
}
