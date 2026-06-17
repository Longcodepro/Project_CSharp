using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Queries.GetPlaylistById;

/// <summary>
/// Handler lấy chi tiết playlist và kiểm tra quyền xem playlist private.
/// </summary>
public sealed class GetPlaylistByIdQueryHandler : IRequestHandler<GetPlaylistByIdQuery, PlaylistDto?>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler lấy chi tiết playlist.
    /// </summary>
    /// <param name="playlistRepository">Repository truy cập dữ liệu playlist.</param>
    public GetPlaylistByIdQueryHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Lấy playlist theo id, trả null nếu không tồn tại và ném lỗi 403 nếu playlist private không thuộc user.
    /// </summary>
    /// <param name="request">Query chứa playlist id và user hiện tại.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thông tin playlist hoặc null nếu không tìm thấy.</returns>
    public async Task<PlaylistDto?> Handle(GetPlaylistByIdQuery request, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(request.PlaylistId, cancellationToken);
        if (playlist == null)
        {
            return null;
        }

        if (!playlist.IsPublic && playlist.UserId != request.CurrentUserId)
            throw new ForbiddenAccessException("Bạn không có quyền xem playlist này.");

        var tracks = await _playlistRepository.GetPlaylistTracksAsync(playlist.Id, cancellationToken);

        return new PlaylistDto(
            playlist.Id,
            playlist.UserId,
            playlist.Title,
            playlist.Description,
            playlist.CoverImageUrl,
            playlist.IsPublic,
            playlist.ContentType?.ToString(),
            playlist.ReleaseDate,
            playlist.CreatedAt,
            tracks.Select(track => new PlaylistTrackDto(track.MediaItemId, track.TrackOrder, track.AddedAt)).ToList());
    }
}
