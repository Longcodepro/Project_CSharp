using MediatR;
using TuneVault.Application.Features.Album.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Queries.GetPublicAlbums;

/// <summary>
/// Query lấy danh sách album công khai cho trang khám phá.
/// </summary>
/// <param name="Limit">Số lượng album tối đa cần lấy.</param>
public sealed record GetPublicAlbumsQuery(int Limit) : IRequest<IReadOnlyCollection<AlbumPublicDto>>;

/// <summary>
/// Handler lấy album công khai và map sang DTO an toàn cho người xem bên ngoài.
/// </summary>
public sealed class GetPublicAlbumsQueryHandler : IRequestHandler<GetPublicAlbumsQuery, IReadOnlyCollection<AlbumPublicDto>>
{
    private const int DefaultLimit = 10;
    private const int MaxLimit = 50;
    private readonly IAlbumRepository _albumRepository;

    /// <summary>
    /// Khởi tạo handler lấy album công khai.
    /// </summary>
    public GetPublicAlbumsQueryHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
    }

    /// <summary>
    /// Lấy album public, giới hạn số lượng để tránh response quá lớn trên trang home.
    /// </summary>
    public async Task<IReadOnlyCollection<AlbumPublicDto>> Handle(
        GetPublicAlbumsQuery request,
        CancellationToken cancellationToken)
    {
        var limit = request.Limit <= 0 ? DefaultLimit : Math.Min(request.Limit, MaxLimit);
        var albums = await _albumRepository.GetPublicAsync(limit, cancellationToken);

        var result = new List<AlbumPublicDto>();
        foreach (var album in albums)
        {
            var tracks = await _albumRepository.GetAlbumTracksAsync(album.Id, cancellationToken);
            result.Add(MapToPublicDto(album, tracks));
        }

        return result;
    }

    private static AlbumPublicDto MapToPublicDto(
        Domain.Entities.Album album,
        IEnumerable<Domain.Entities.AlbumTrack> tracks) =>
        new(
            album.Id,
            album.ArtistId,
            album.Title,
            album.Description,
            album.CoverImageUrl,
            album.IsPublic,
            album.ContentType?.ToString(),
            album.ReleaseDate,
            album.CreatedAt,
            tracks.Select(track => new AlbumTrackDto(track.MediaItemId, track.TrackOrder, track.AddedAt)).ToList());
}
