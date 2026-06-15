using TuneVault.Application.DTOs.Playlist;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Queries.GetPlaylists;

/// <summary>
/// QUERY HANDLER - LẤY DANH SÁCH PLAYLIST (Application Layer)
/// ===========================================================
/// Mục đích: Xử lý logic truy vấn danh sách Playlist của một user.
/// 
/// Luồng xử lý:
/// 1. Controller gửi GetPlaylistsQuery
/// 2. Handler gọi Repository lấy danh sách Playlist theo OwnerId
/// 3. Map Entity sang DTO
/// 4. Trả về danh sách PlaylistDto cho Controller
/// 
/// Lý do tách ra khỏi Controller:
/// - Controller chỉ lo nhận/trả HTTP request
/// - Logic truy vấn tập trung tại đây, dễ test, dễ bảo trì
/// </summary>
public sealed class GetPlaylistsQueryHandler
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public GetPlaylistsQueryHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Thực thi logic lấy danh sách Playlist của user từ Query.
    /// </summary>
    /// <param name="query">Query chứa OwnerId của user cần lấy playlist.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách PlaylistDto của user.</returns>
    public async Task<IReadOnlyCollection<PlaylistDto>> HandleAsync(GetPlaylistsQuery query, CancellationToken cancellationToken = default)
    {
        // Lấy danh sách Playlist từ Database theo OwnerId
        var playlists = await _playlistRepository.GetByOwnerIdAsync(query.OwnerId, cancellationToken);

        // Map Entity sang DTO trả về cho Controller
        return playlists.Select(p => new PlaylistDto(
            p.Id,
            p.UserId,
            p.Title,
            p.CoverImageUrl,
            p.IsPublic,
            p.CreatedAt
        )).ToList();
    }
}
