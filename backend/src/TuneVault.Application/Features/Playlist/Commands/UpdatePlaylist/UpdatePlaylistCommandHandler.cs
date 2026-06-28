using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.UpdatePlaylist;

/// <summary>
/// Handler cập nhật metadata và trạng thái hiển thị của playlist.
/// </summary>
public sealed class UpdatePlaylistCommandHandler : IRequestHandler<UpdatePlaylistCommand, PlaylistDto>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler cập nhật playlist.
    /// </summary>
    /// <param name="playlistRepository">Repository truy cập dữ liệu playlist.</param>
    public UpdatePlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Cập nhật playlist sau khi kiểm tra playlist còn hoạt động và thuộc về user hiện tại.
    /// </summary>
    /// <param name="request">Command chứa playlist id, user id và payload cập nhật.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Playlist sau khi cập nhật.</returns>
    public async Task<PlaylistDto> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(request.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != request.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền sửa playlist này.");

        playlist.Rename(request.Request.Title);
        playlist.UpdateDescription(request.Request.Description);
        playlist.UpdateCoverImage(request.Request.CoverImgUrl);
        playlist.SetPublic(request.Request.IsPublic);
        playlist.SetContentType(ParseContentType(request.Request.ContentType));
        playlist.SetReleaseDate(request.Request.ReleaseDate);

        await _playlistRepository.UpdateAsync(playlist, cancellationToken);

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

    /// <summary>
    /// Chuyển kiểu nội dung playlist từ request sang enum domain.
    /// </summary>
    private static MediaType? ParseContentType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<MediaType>(value, ignoreCase: true, out var mediaType)
            && Enum.IsDefined(typeof(MediaType), mediaType))
            return mediaType;

        throw new DomainException("Kiểu nội dung playlist không hợp lệ.");
    }
}
