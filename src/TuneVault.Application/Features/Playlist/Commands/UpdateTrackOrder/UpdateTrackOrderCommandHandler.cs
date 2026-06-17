using MediatR;
using TuneVault.Domain.Exceptions;
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
public sealed class UpdateTrackOrderCommandHandler : IRequestHandler<UpdateTrackOrderCommand, Unit>
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
    /// <exception cref="DomainException">
    /// Ném ra khi Playlist không tồn tại hoặc Track không có trong Playlist.
    /// </exception>
    /// <exception cref="ForbiddenAccessException">Ném ra khi user không phải chủ playlist.</exception>
    public async Task<Unit> Handle(UpdateTrackOrderCommand command, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != command.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền cập nhật playlist này.");

        var tracks = (await _playlistRepository.GetPlaylistTracksAsync(command.PlaylistId, cancellationToken)).ToList();

        var trackToUpdate = tracks.FirstOrDefault(t => t.MediaItemId == command.MediaItemId)
            ?? throw new DomainException("Bài hát không tồn tại trong playlist.");

        if (command.NewTrackOrder < 1 || command.NewTrackOrder > tracks.Count)
            throw new DomainException($"Thứ tự bài hát phải nằm trong khoảng từ 1 đến {tracks.Count}.");

        if (trackToUpdate.TrackOrder == command.NewTrackOrder)
            return Unit.Value;

        var reorderedTracks = tracks
            .Where(track => track.Id != trackToUpdate.Id)
            .OrderBy(track => track.TrackOrder)
            .ToList();

        reorderedTracks.Insert(command.NewTrackOrder - 1, trackToUpdate);

        for (var index = 0; index < reorderedTracks.Count; index++)
        {
            await _playlistRepository.UpdateTrackOrderAsync(
                command.PlaylistId,
                reorderedTracks[index].Id,
                index + 1,
                cancellationToken);
        }

        return Unit.Value;
    }
}
