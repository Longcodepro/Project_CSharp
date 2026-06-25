using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetFollowers;

/// <summary>
/// Query (yêu cầu đọc) đại diện cho nghiệp vụ lấy danh sách những người đang theo dõi một tài khoản.
/// Dùng để hiển thị danh sách "Followers" trên trang profile.
/// Kết quả trả về là tập hợp <see cref="UserDto"/> — không chứa thông tin nhạy cảm.
/// </summary>
/// <param name="UserId">Mã định danh hệ thống của User cần lấy danh sách followers (ví dụ: U001).</param>
public record GetFollowersQuery(string UserId) : IRequest<IEnumerable<UserDto>>;
