using TuneVault.Application.DTOs.Playlist;
using TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;
using TuneVault.Domain.Interfaces;
using PlaylistEntity = TuneVault.Domain.Entities.Playlist;

namespace TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;

/// <summary>
/// COMMAND HANDLER - TẠO PLAYLIST (Application Layer)
/// ===================================================
/// Mục đích: Xử lý logic nghiệp vụ tạo Playlist mới.
/// 
/// Luồng xử lý:
/// 1. Controller gửi CreatePlaylistCommand
/// 2. Handler sinh ID tự động theo format PL001, PL002... (tách chữ + tăng số)
/// 3. Handler tạo Entity Playlist (Domain Layer tự validate)
/// 4. Handler gọi Repository lưu xuống Database
/// 5. Trả về PlaylistDto cho Controller
/// 
/// Logic sinh ID:
/// - Query DB lấy ID lớn nhất hiện tại (ví dụ PL006)
/// - Tách phần chữ (PL) và phần số (006)
/// - Tăng số lên 1 → 007
/// - Ghép lại → PL007
/// </summary>
public sealed class CreatePlaylistCommandHandler
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public CreatePlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Thực thi logic tạo Playlist mới từ Command.
    /// Gọi Entity Constructor để tự validate các ràng buộc nghiệp vụ trước khi lưu.
    /// </summary>
    /// <param name="command">Command chứa OwnerId và thông tin Playlist cần tạo.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>PlaylistDto đại diện cho Playlist vừa được tạo.</returns>
    public async Task<PlaylistDto> HandleAsync(CreatePlaylistCommand command, CancellationToken cancellationToken = default)
    {
        // Sinh ID tự động theo format PL001, PL002...
        var playlistId = await GenerateNextIdAsync(cancellationToken);

        // Gọi Entity Constructor — Domain tự validate toàn bộ ràng buộc nghiệp vụ
        var playlist = new PlaylistEntity(
            playlistId,
            command.OwnerId,
            command.Request.Title,
            command.Request.Description,
            command.Request.CoverImgUrl,
            command.Request.IsPublic
        );

        // Lưu Entity xuống Database thông qua Repository
        await _playlistRepository.AddAsync(playlist, cancellationToken);

        // Map Entity sang DTO trả về cho Controller
        return new PlaylistDto(
            playlist.Id,
            playlist.UserId,
            playlist.Title,
            playlist.CoverImageUrl,
            playlist.IsPublic,
            playlist.CreatedAt
        );
    }

    /// <summary>
    /// Sinh ID tiếp theo theo format PL001, PL002...
    /// Lấy ID lớn nhất trong DB, tách phần chữ và phần số, tăng số lên 1.
    /// Ví dụ: PL006 → tách ra PL + 006 → tăng lên 007 → ghép lại PL007
    /// </summary>
    private async Task<string> GenerateNextIdAsync(CancellationToken cancellationToken)
    {
        const string prefix = "PL";

        // Lấy tất cả playlist từ DB
        var allPlaylists = await _playlistRepository.GetAllAsync(cancellationToken);

        // Lọc các ID có đúng format PL + số, lấy số lớn nhất
        var maxNumber = allPlaylists
            .Select(p => p.Id)
            .Where(id => id.StartsWith(prefix) && id.Length > prefix.Length)
            .Select(id =>
            {
                var numberPart = id.Substring(prefix.Length);
                return int.TryParse(numberPart, out var num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        // Tăng số lên 1, format 3 chữ số (001, 002...)
        var nextNumber = maxNumber + 1;
        return $"{prefix}{nextNumber:D3}";
    }
}