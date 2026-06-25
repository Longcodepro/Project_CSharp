using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetProfile;

/// <summary>
/// Query (yêu cầu đọc) đại diện cho nghiệp vụ lấy thông tin profile đầy đủ của người dùng theo Id hệ thống.
/// Dùng khi cần hiển thị toàn bộ trang profile: avatar, bio, followers, ngày tạo, v.v.
/// Kết quả trả về là <see cref="UserProfileDto"/> — không chứa thông tin nhạy cảm.
/// </summary>
/// <param name="Id">Mã định danh hệ thống của người dùng cần lấy profile (ví dụ: U001).</param>
public record GetUserProfileQuery(string Id) : IRequest<UserProfileDto>;
