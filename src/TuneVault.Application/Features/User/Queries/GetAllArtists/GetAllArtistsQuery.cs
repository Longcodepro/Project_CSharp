using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetAllArtists;

/// <summary>
/// Query (yêu cầu đọc) đại diện cho nghiệp vụ lấy danh sách tất cả tài khoản nghệ sĩ đang hoạt động.
/// Dùng cho trang khám phá nghệ sĩ, tìm kiếm, gợi ý theo dõi.
/// Kết quả trả về là tập hợp <see cref="UserDto"/> — không chứa thông tin nhạy cảm.
/// </summary>
public record GetAllArtistsQuery() : IRequest<IEnumerable<UserDto>>;
