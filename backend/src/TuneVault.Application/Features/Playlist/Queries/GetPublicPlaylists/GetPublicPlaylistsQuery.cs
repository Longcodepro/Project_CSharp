using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Queries.GetPublicPlaylists;

/// <summary>
/// Query lấy danh sách playlist công khai cho trang khám phá.
/// </summary>
/// <param name="Limit">Số lượng playlist tối đa cần lấy.</param>
public sealed record GetPublicPlaylistsQuery(int Limit) : IRequest<IReadOnlyCollection<PlaylistPublicDto>>;

/// <summary>
/// Handler lấy playlist công khai và map sang DTO an toàn cho người xem bên ngoài.
/// </summary>
public sealed class GetPublicPlaylistsQueryHandler : IRequestHandler<GetPublicPlaylistsQuery, IReadOnlyCollection<PlaylistPublicDto>>
{
    private const int DefaultLimit = 10;
    private const int MaxLimit = 50;
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler lấy playlist công khai.
    /// </summary>
    public GetPublicPlaylistsQueryHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Lấy playlist public, giới hạn số lượng để tránh response quá lớn trên trang home.
    /// </summary>
    public async Task<IReadOnlyCollection<PlaylistPublicDto>> Handle(
        GetPublicPlaylistsQuery request,
        CancellationToken cancellationToken)
    {
        var limit = request.Limit <= 0 ? DefaultLimit : Math.Min(request.Limit, MaxLimit);
        var playlists = await _playlistRepository.GetPublicAsync(limit, cancellationToken);

        var result = new List<PlaylistPublicDto>();
        foreach (var playlist in playlists)
        {
            var tracks = await _playlistRepository.GetPlaylistTracksAsync(playlist.Id, cancellationToken);
            result.Add(MapToPublicDto(playlist, tracks));
        }

        return result;
    }

    private static PlaylistPublicDto MapToPublicDto(
        Domain.Entities.Playlist playlist,
        IEnumerable<Domain.Entities.PlaylistTrack> tracks) =>
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
