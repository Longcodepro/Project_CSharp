using MediatR;
using TuneVault.Application.Features.Album.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Queries.GetMyAlbums;

/// <summary>
/// Query lấy danh sách album của artist hiện tại.
/// </summary>
public sealed record GetMyAlbumsQuery(string ArtistId) : IRequest<IReadOnlyCollection<AlbumDto>>;

/// <summary>
/// Handler lấy danh sách album của artist.
/// </summary>
public sealed class GetMyAlbumsQueryHandler : IRequestHandler<GetMyAlbumsQuery, IReadOnlyCollection<AlbumDto>>
{
    private readonly IAlbumRepository _albumRepository;

    /// <summary>
    /// Khởi tạo handler lấy danh sách album.
    /// </summary>
    public GetMyAlbumsQueryHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
    }

    /// <summary>
    /// Lấy danh sách album và track của từng album cho owner.
    /// </summary>
    public async Task<IReadOnlyCollection<AlbumDto>> Handle(GetMyAlbumsQuery request, CancellationToken cancellationToken)
    {
        var albums = await _albumRepository.GetByArtistIdAsync(request.ArtistId, cancellationToken);
        var result = new List<AlbumDto>();

        foreach (var album in albums)
        {
            var tracks = await _albumRepository.GetAlbumTracksAsync(album.Id, cancellationToken);
            result.Add(new AlbumDto(
                album.Id,
                album.ArtistId,
                album.Title,
                album.Description,
                album.CoverImageUrl,
                album.IsPublic,
                album.ContentType?.ToString(),
                album.ReleaseDate,
                album.CreatedAt,
                tracks.Select(t => new AlbumTrackDto(t.MediaItemId, t.TrackOrder, t.AddedAt)).ToList()));
        }

        return result;
    }
}
