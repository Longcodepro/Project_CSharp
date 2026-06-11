// Đường dẫn: src/TuneVault.Application/Features/User/Queries/GetAllArtists/GetAllArtistsQuery.cs
using MediatR;
using TuneVault.Application.Features.User.DTOs;

namespace TuneVault.Application.Features.User.Queries.GetAllArtists;

/// <summary>
/// Gói tin truy vấn (Query) đại diện cho yêu cầu lấy danh sách tất cả người dùng
/// có trạng thái nghệ sĩ được xác thực (IsArtist = true).
/// </summary>
public record GetAllArtistsQuery() : IRequest<IEnumerable<UserDto>>;