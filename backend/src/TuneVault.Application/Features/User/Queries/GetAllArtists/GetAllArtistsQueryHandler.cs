using MediatR;
using TuneVault.Application.Features.User.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.User.Queries.GetAllArtists;

/// <summary>
/// Handler xử lý <see cref="GetAllArtistsQuery"/>.
/// Lấy tập hợp <see cref="TuneVault.Domain.Entities.User"/> Entity là nghệ sĩ
/// và map từng Entity sang <see cref="UserDto"/> — không lộ thông tin nhạy cảm.
/// Đây là dữ liệu public để render trang khám phá, không yêu cầu đăng nhập.
/// </summary>
public class GetAllArtistsQueryHandler : IRequestHandler<GetAllArtistsQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo Handler với dependency là <see cref="IUserRepository"/>.
    /// </summary>
    /// <param name="userRepository">Interface kho dữ liệu User, được inject qua DI container.</param>
    public GetAllArtistsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
    public async Task<IEnumerable<UserDto>> Handle(GetAllArtistsQuery request, CancellationToken ct)
    {
        var artists = await _userRepository.GetAllArtistsAsync(ct);

        // Map sang DTO public, không lộ Email hoặc PasswordHash.
        return artists.Select(u => new UserDto
        {
            Id          = u.Id,
            IdDisplay   = u.IdDisplay,
            DisplayName = u.DisplayName,
            AvatarUrl   = u.AvatarUrl,
            Role        = "Artist",
            IsActive    = u.IsActive
        });
    }
}
