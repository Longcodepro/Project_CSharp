using MediatR;
using TuneVault.Domain.Exceptions;
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
public sealed class RemoveTrackFromPlaylistCommandHandler : IRequestHandler<RemoveTrackFromPlaylistCommand, Unit>
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
    /// <exception cref="DomainException">
    /// Ném ra khi Playlist không tồn tại hoặc Track không có trong Playlist.
    /// </exception>
    /// <exception cref="ForbiddenAccessException">Ném ra khi user không phải chủ playlist.</exception>
    public async Task<Unit> Handle(RemoveTrackFromPlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != command.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền xóa bài hát khỏi playlist này.");

        var tracks = (await _playlistRepository.GetPlaylistTracksAsync(command.PlaylistId, cancellationToken)).ToList();
        var trackToRemove = tracks.FirstOrDefault(t => t.MediaItemId == command.MediaItemId)
            ?? throw new DomainException("Bài hát không tồn tại trong playlist.");

        var removedOrder = trackToRemove.TrackOrder;

        await _playlistRepository.RemoveTrackAsync(command.PlaylistId, command.MediaItemId, cancellationToken);

        await _playlistRepository.ShiftTrackOrdersAsync(
            command.PlaylistId,
            removedOrder + 1,
            -1,
            cancellationToken);

        return Unit.Value;
    }
}
