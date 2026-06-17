using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.AddTrackToPlaylist;

/// <summary>
/// COMMAND HANDLER - THÊM TRACK VÀO PLAYLIST (Application Layer)
/// ==============================================================
/// Mục đích: Xử lý logic nghiệp vụ thêm một bài hát vào Playlist.
/// 
/// Luồng xử lý:
/// 1. Controller gửi AddTrackToPlaylistCommand
/// 2. Handler sinh ID tự động theo format PT001, PT002...
/// 3. Handler lấy Playlist từ Repository
/// 4. Handler tính trackOrder = số track hiện tại + 1 (thêm vào cuối)
/// 5. Handler tạo PlaylistTrack Entity (Domain tự validate)
/// 6. Handler gọi playlist.AddTrack() — Entity tự kiểm tra logic
/// 7. Handler gọi Repository lưu Track xuống Database
/// 
/// Logic trackOrder:
/// - Đếm số track hiện tại trong playlist
/// - trackOrder = số track hiện tại + 1
/// - Ví dụ: playlist đang có 3 track → track mới sẽ là vị trí 4
/// </summary>
public sealed class AddTrackToPlaylistCommandHandler : IRequestHandler<AddTrackToPlaylistCommand, Unit>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public AddTrackToPlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Thực thi logic thêm Track vào Playlist từ Command.
    /// Track mới luôn được thêm vào cuối playlist.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId và MediaItemId.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="DomainException">Ném ra khi Playlist hoặc media không tồn tại.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra khi user không phải chủ playlist.</exception>
    public async Task<Unit> Handle(AddTrackToPlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != command.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền thêm bài hát vào playlist này.");

        var mediaExists = await _playlistRepository.MediaItemExistsAsync(command.Request.MediaItemId, cancellationToken);
        if (!mediaExists)
            throw new DomainException("Không tìm thấy bài hát.");

        var trackExists = await _playlistRepository.TrackExistsAsync(command.PlaylistId, command.Request.MediaItemId, cancellationToken);
        if (trackExists)
            throw new DomainException("Bài hát đã tồn tại trong playlist.");

        var currentTracks = await _playlistRepository.GetPlaylistTracksAsync(command.PlaylistId, cancellationToken);
        var trackOrder = currentTracks.Any() ? currentTracks.Max(track => track.TrackOrder) + 1 : 1;

        var trackId = await GenerateNextTrackIdAsync(cancellationToken);

        var track = new PlaylistTrack(
            trackId,
            command.PlaylistId,
            command.Request.MediaItemId,
            trackOrder
        );

        playlist.AddTrack(track);

        await _playlistRepository.AddTrackAsync(track, cancellationToken);
        return Unit.Value;
    }

    /// <summary>
    /// Sinh ID tiếp theo theo format PT001, PT002...
    /// Lấy ID lớn nhất trong DB, tách phần chữ và phần số, tăng số lên 1.
    /// Ví dụ: PT006 → tách ra PT + 006 → tăng lên 007 → ghép lại PT007
    /// </summary>
    private async Task<string> GenerateNextTrackIdAsync(CancellationToken cancellationToken)
    {
        const string prefix = "PT";

        var allTracks = await _playlistRepository.GetAllTracksAsync(cancellationToken);

        var maxNumber = allTracks
            .Select(t => t.Id)
            .Where(id => id.StartsWith(prefix) && id.Length > prefix.Length)
            .Select(id =>
            {
                var numberPart = id.Substring(prefix.Length);
                return int.TryParse(numberPart, out var num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        var nextNumber = maxNumber + 1;
        return $"{prefix}{nextNumber:D3}";
    }
}
