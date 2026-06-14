using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetAllArtists;

/// <summary>
/// Handler xử lý <see cref="GetAllArtistsQuery"/>.
/// Lấy tập hợp <see cref="TuneVault.Domain.Entities.User"/> Entity là nghệ sĩ
/// và map từng Entity sang <see cref="UserDto"/> — không lộ thông tin nhạy cảm.
/// Phân quyền: yêu cầu người dùng đã đăng nhập (Listener / Artist / Admin).
/// </summary>
public class GetAllArtistsQueryHandler : IRequestHandler<GetAllArtistsQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/> và <see cref="ICurrentUserContext"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    /// <param name="currentUserContext">Service lấy thông tin người dùng hiện tại từ JWT để kiểm tra quyền.</param>
    public GetAllArtistsQueryHandler(IUserRepository userRepository, ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// Xử lý luồng lấy danh sách nghệ sĩ:
    /// kiểm tra xác thực → truy vấn tập hợp Entity (IsArtist = true, IsActive = true) → map từng Entity sang DTO → trả về.
    /// </summary>
    /// <param name="request">Query không có tham số — lấy toàn bộ danh sách nghệ sĩ.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns>
    /// Tập hợp <see cref="UserDto"/> đại diện cho tất cả nghệ sĩ đang hoạt động.
    /// Trả về tập hợp rỗng nếu chưa có nghệ sĩ nào trong hệ thống.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Ném ra nếu chưa đăng nhập.</exception>
    public async Task<IEnumerable<UserDto>> Handle(GetAllArtistsQuery request, CancellationToken ct)
    {
        // Step 0: Kiểm tra đã xác thực chưa
        if (string.IsNullOrWhiteSpace(_currentUserContext.GetCurrentUserId()))
            throw new UnauthorizedAccessException("Chưa xác thực. Vui lòng đăng nhập để xem danh sách nghệ sĩ.");

        // Step 1: Truy vấn tập hợp User Entity có IsArtist = true và IsActive = true
        var artists = await _userRepository.GetAllArtistsAsync(ct);

        // Step 2: Map từng Entity sang UserDto — ẩn Id hệ thống, Email, PasswordHash
        //         Role luôn là "Artist" vì đây là kết quả đã lọc từ repository
        return artists.Select(u => new UserDto
        {
            IdDisplay   = u.IdDisplay,
            DisplayName = u.DisplayName,
            AvatarUrl   = u.AvatarUrl,
            Role        = "Artist",
            IsActive    = u.IsActive
        });
    }
}