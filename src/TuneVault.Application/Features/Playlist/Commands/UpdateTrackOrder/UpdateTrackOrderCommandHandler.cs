using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.UpdateTrackOrder;

/// <summary>
/// COMMAND HANDLER - CẬP NHẬT THỨ TỰ TRACK TRONG PLAYLIST (Application Layer)
/// =============================================================================
/// Mục đích: Xử lý logic nghiệp vụ cập nhật TrackOrder của một track trong Playlist.
/// 
/// Luồng xử lý:
/// 1. Controller gửi UpdateTrackOrderCommand
/// 2. Handler lấy Playlist từ Repository — kiểm tra tồn tại
/// 3. Handler lấy toàn bộ PlaylistTrack, lọc theo PlaylistId
/// 4. Handler tìm PlaylistTrack cần cập nhật theo MediaItemId
/// 5. Handler gọi Repository cập nhật TrackOrder xuống Database
/// 
/// Lưu ý:
/// - KHÔNG gọi playlist.UpdateTrackOrder() vì Dapper không load navigation property
///   → _tracks bên trong Playlist luôn rỗng → sẽ throw DomainException
/// - Validation đã được thực hiện ở Handler (kiểm tra tồn tại Playlist và Track)
/// </summary>
public sealed class UpdateTrackOrderCommandHandler
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public UpdateTrackOrderCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Thực thi logic cập nhật TrackOrder của một Track trong Playlist.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId, MediaItemId và NewTrackOrder.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="InvalidOperationException">
    /// Ném ra khi Playlist không tồn tại hoặc Track không có trong Playlist.
    /// </exception>
    public async Task HandleAsync(UpdateTrackOrderCommand command, CancellationToken cancellationToken = default)
    {
        // Lấy Playlist từ Database — kiểm tra tồn tại
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new InvalidOperationException($"Playlist '{command.PlaylistId}' không tồn tại.");

        // Lấy toàn bộ PlaylistTrack trong DB, lọc theo PlaylistId
        var allTracks = await _playlistRepository.GetAllTracksAsync(cancellationToken);
        var tracks = allTracks.Where(t => t.PlaylistId == command.PlaylistId).ToList();

        // Tìm PlaylistTrack cần cập nhật theo MediaItemId — throw ngay nếu không tồn tại
        var trackToUpdate = tracks.FirstOrDefault(t => t.MediaItemId == command.MediaItemId)
            ?? throw new InvalidOperationException($"Track '{command.MediaItemId}' không tồn tại trong Playlist '{command.PlaylistId}'.");

        // Cập nhật TrackOrder xuống Database
        // KHÔNG gọi playlist.UpdateTrackOrder() vì Dapper không load _tracks → luôn throw
        await _playlistRepository.UpdateTrackOrderAsync(
            command.PlaylistId,
            trackToUpdate.Id,
            command.NewTrackOrder,
            cancellationToken);
    }
}