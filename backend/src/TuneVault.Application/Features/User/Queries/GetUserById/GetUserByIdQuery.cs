using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetUserById;

/// <summary>
/// Query (yêu cầu đọc) đại diện cho nghiệp vụ lấy thông tin cơ bản của người dùng theo Id hệ thống.
/// Dùng trong các trường hợp cần thông tin nhanh (header, mention, tag) — không cần đầy đủ như profile page.
/// Kết quả trả về là <see cref="UserPublicDetailDto"/> để frontend hiển thị trang profile công khai.
/// </summary>
/// <param name="Id">Mã định danh hệ thống của người dùng (ví dụ: U001).</param>
public record GetUserByIdQuery(string Id) : IRequest<UserPublicDetailDto?>;
