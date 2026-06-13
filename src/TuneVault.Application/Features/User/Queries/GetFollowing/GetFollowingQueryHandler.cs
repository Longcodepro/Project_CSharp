using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetFollowing;

/// <summary>
/// Handler xử lý <see cref="GetFollowingQuery"/>.
/// Lấy tập hợp <see cref="TuneVault.Domain.Entities.User"/> Entity mà UserId đang theo dõi
/// và map từng Entity sang <see cref="UserDto"/> — không lộ thông tin nhạy cảm.
/// </summary>
public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public GetFollowingQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Xử lý luồng lấy danh sách following:
    /// truy vấn tập hợp Entity → map từng Entity sang DTO → trả về.
    /// </summary>
    /// <param name="request">Query chứa UserId cần lấy danh sách following.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>
    /// Tập hợp <see cref="UserDto"/> đại diện cho những người UserId đang theo dõi.
    /// Trả về tập hợp rỗng nếu chưa theo dõi ai.
    /// </returns>
    public async Task<IEnumerable<UserDto>> Handle(GetFollowingQuery request, CancellationToken ct)
    {
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
