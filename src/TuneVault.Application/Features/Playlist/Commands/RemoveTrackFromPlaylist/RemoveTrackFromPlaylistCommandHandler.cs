using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.RemoveTrackFromPlaylist;

/// <summary>
/// COMMAND HANDLER - XÓA TRACK KHỎI PLAYLIST (Application Layer)
/// ==============================================================
/// Mục đích: Xử lý logic nghiệp vụ xóa một bài hát khỏi Playlist.
/// 
/// Luồng xử lý:
/// 1. Controller gửi RemoveTrackFromPlaylistCommand
/// 2. Handler lấy Playlist từ Repository — kiểm tra tồn tại
/// 3. Handler lấy toàn bộ PlaylistTrack từ DB, lọc theo PlaylistId
/// 4. Handler tìm PlaylistTrack cần xóa theo MediaItemId — lấy TrackOrder và PlaylistTrack.Id
/// 5. Handler gọi playlist.RemoveTrack() — Entity tự validate (nhận PlaylistTrack.Id)
/// 6. Handler gọi Repository xóa Track khỏi Database
/// 7. Handler cập nhật lại TrackOrder của các track phía sau (giảm 1)
/// 
/// Lưu ý quan trọng:
/// - GetTracksAsync() trả về MediaItem (không có TrackOrder) → KHÔNG dùng
/// - GetAllTracksAsync() trả về PlaylistTrack (có đủ Id, MediaItemId, TrackOrder) → dùng cái này
/// - RemoveTrack() nhận PlaylistTrack.Id (PT001...), KHÔNG phải MediaItemId
/// 
/// Logic điều chỉnh vị trí sau khi xóa:
/// - Xóa track ở vị trí 3
/// - Track ở vị trí 4 → xuống vị trí 3
/// - Track ở vị trí 5 → xuống vị trí 4
/// - ... và cứ tiếp tục
/// </summary>
public sealed class RemoveTrackFromPlaylistCommandHandler
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo Handler với Repository được inject qua DI container.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public RemoveTrackFromPlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Thực thi logic xóa Track khỏi Playlist từ Command.
    /// Sau khi xóa, tự động điều chỉnh lại TrackOrder của các track phía sau.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId và MediaItemId cần xóa.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="InvalidOperationException">
    /// Ném ra khi Playlist không tồn tại hoặc Track không có trong Playlist.
    /// </exception>
    public async Task HandleAsync(RemoveTrackFromPlaylistCommand command, CancellationToken cancellationToken = default)
    {
        // Lấy Playlist từ Database — kiểm tra tồn tại
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new InvalidOperationException($"Playlist '{command.PlaylistId}' không tồn tại.");

        // Lấy toàn bộ PlaylistTrack trong DB, lọc theo PlaylistId
        // Dùng GetAllTracksAsync() vì GetTracksAsync() trả về MediaItem — không có TrackOrder
        var allTracks = await _playlistRepository.GetAllTracksAsync(cancellationToken);
        var tracks = allTracks.Where(t => t.PlaylistId == command.PlaylistId).ToList();

        // Tìm PlaylistTrack cần xóa theo MediaItemId — throw ngay nếu không tồn tại
        var trackToRemove = tracks.FirstOrDefault(t => t.MediaItemId == command.MediaItemId)
            ?? throw new InvalidOperationException($"Track '{command.MediaItemId}' không tồn tại trong Playlist '{command.PlaylistId}'.");

        var removedOrder = trackToRemove.TrackOrder;

        // Gọi Aggregate Root để xóa Track — truyền PlaylistTrack.Id (PT001...), không phải MediaItemId
        playlist.RemoveTrack(trackToRemove.Id);

        // Xóa track khỏi Database
        await _playlistRepository.RemoveTrackAsync(command.PlaylistId, command.MediaItemId, cancellationToken);

        // Cập nhật lại TrackOrder của các track phía sau (giảm xuống 1)
        var tracksToUpdate = tracks
            .Where(t => t.Id != trackToRemove.Id && t.TrackOrder > removedOrder)
            .ToList();

        foreach (var track in tracksToUpdate)
        {
            await _playlistRepository.UpdateTrackOrderAsync(
                command.PlaylistId,
                track.Id,
                track.TrackOrder - 1,
                cancellationToken);
        }
    }
}