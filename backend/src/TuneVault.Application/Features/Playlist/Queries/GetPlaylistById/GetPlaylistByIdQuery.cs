using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;

namespace TuneVault.Application.Features.Playlist.Queries.GetPlaylistById;

/// <summary>
/// Query lấy chi tiết playlist theo quyền truy cập của người dùng hiện tại.
/// </summary>
/// <param name="PlaylistId">Mã playlist cần lấy.</param>
/// <param name="CurrentUserId">Mã người dùng đang đăng nhập, có thể null nếu người xem là khách.</param>
public sealed record GetPlaylistByIdQuery(string PlaylistId, string? CurrentUserId) : IRequest<PlaylistDto?>;
