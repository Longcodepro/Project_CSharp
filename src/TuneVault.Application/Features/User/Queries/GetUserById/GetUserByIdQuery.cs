// Đường dẫn: src/TuneVault.Application/Features/Users/Queries/GetUserById/GetUserByIdQuery.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetUserById;

/// <summary>
/// Gói tin truy vấn (Query) đại diện cho yêu cầu tìm kiếm thông tin người dùng dựa theo ID.
/// </summary>
/// <param name="Id">Mã định danh hệ thống của người dùng cần được truy vấn dữ liệu.</param>
public record GetUserByIdQuery(string Id) : IRequest<UserDto>;