using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Queries.GetPlaylists;

/// <summary>
/// Query lấy danh sách playlist của người dùng hiện tại.
/// </summary>
/// <param name="OwnerId">Mã người dùng sở hữu playlist.</param>
public sealed record GetPlaylistsQuery(string OwnerId) : IRequest<IReadOnlyCollection<PlaylistDto>>;

/// <summary>
/// Handler lấy danh sách playlist của người dùng hiện tại.
/// </summary>
public sealed class GetPlaylistsQueryHandler : IRequestHandler<GetPlaylistsQuery, IReadOnlyCollection<PlaylistDto>>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler lấy danh sách playlist.
    /// </summary>
    /// <param name="playlistRepository">Repository truy cập dữ liệu playlist.</param>
    public GetPlaylistsQueryHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Lấy danh sách playlist của user và map kèm track cơ bản.
    /// </summary>
    /// <param name="request">Query chứa mã user sở hữu playlist.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Danh sách playlist của user.</returns>
    public async Task<IReadOnlyCollection<PlaylistDto>> Handle(GetPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var playlists = await _playlistRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

        var result = new List<PlaylistDto>();
        foreach (var playlist in playlists)
        {
            var tracks = await _playlistRepository.GetPlaylistTracksAsync(playlist.Id, cancellationToken);
            result.Add(MapToDto(playlist, tracks));
        }

        return result;
    }

    private static PlaylistDto MapToDto(Domain.Entities.Playlist playlist, IEnumerable<Domain.Entities.PlaylistTrack> tracks) =>
        new(
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
