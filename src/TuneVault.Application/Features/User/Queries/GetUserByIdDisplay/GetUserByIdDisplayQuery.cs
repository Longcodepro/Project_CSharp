// Đường dẫn: src/TuneVault.Application/Features/User/Queries/GetUserByIdDisplay/GetUserByIdDisplayQuery.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetUserByIdDisplay;

/// <summary>
/// Gói tin truy vấn (Query) đại diện cho yêu cầu tìm kiếm người dùng dựa theo IdDisplay (handle công khai).
/// </summary>
/// <param name="IdDisplay">Chuỗi định danh hiển thị công khai của người dùng (ví dụ: john_doe).</param>
public record GetUserByIdDisplayQuery(string IdDisplay) : IRequest<UserDto>;