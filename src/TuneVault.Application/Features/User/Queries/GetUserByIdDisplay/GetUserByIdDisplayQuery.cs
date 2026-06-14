using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetUserByIdDisplay;

/// <summary>
/// Query (yêu cầu đọc) đại diện cho nghiệp vụ tìm kiếm người dùng theo handle hiển thị công khai (IdDisplay).
/// Dùng cho các tính năng: tìm kiếm user qua username, xem profile bằng URL dạng /@john_doe.
/// Kết quả trả về là <see cref="UserDto"/> — chỉ gồm các trường công khai cơ bản.
/// </summary>
/// <param name="IdDisplay">Handle công khai của người dùng (ví dụ: john_doe). Không phân biệt hoa/thường.</param>
public record GetUserByIdDisplayQuery(string IdDisplay) : IRequest<UserDto?>;
