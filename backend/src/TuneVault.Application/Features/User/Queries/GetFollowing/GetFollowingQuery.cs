using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetFollowing;

/// <summary>
/// Query (yêu cầu đọc) đại diện cho nghiệp vụ lấy danh sách những người mà một tài khoản đang theo dõi.
/// Dùng để hiển thị danh sách "Following" trên trang profile.
/// Kết quả trả về là tập hợp <see cref="UserDto"/> — không chứa thông tin nhạy cảm.
/// </summary>
/// <param name="UserId">Mã định danh hệ thống của User cần lấy danh sách following (ví dụ: U001).</param>
public record GetFollowingQuery(string UserId) : IRequest<IEnumerable<UserDto>>;
