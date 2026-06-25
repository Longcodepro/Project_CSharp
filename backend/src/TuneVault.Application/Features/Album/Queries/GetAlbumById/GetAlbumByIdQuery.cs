using MediatR;
using TuneVault.Application.Features.Album.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Queries.GetAlbumById;

/// <summary>
/// Query lấy chi tiết album theo id.
/// </summary>
public sealed record GetAlbumByIdQuery(string AlbumId, string? CurrentUserId) : IRequest<AlbumDto?>;

/// <summary>
/// Handler lấy chi tiết album và kiểm tra quyền xem album private.
/// </summary>
public sealed class GetAlbumByIdQueryHandler : IRequestHandler<GetAlbumByIdQuery, AlbumDto?>
{
    private readonly IAlbumRepository _albumRepository;

    /// <summary>
    /// Khởi tạo handler lấy chi tiết album.
    /// </summary>
    public GetAlbumByIdQueryHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
    }

    /// <summary>
    /// Lấy album theo id, owner được xem chi tiết, người ngoài chỉ xem album public.
    /// </summary>
    public async Task<AlbumDto?> Handle(GetAlbumByIdQuery request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(request.AlbumId, cancellationToken);
        if (album is null)
            return null;

        if (!album.IsPublic && !string.Equals(album.ArtistId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền xem album này.");

        var tracks = await _albumRepository.GetAlbumTracksAsync(album.Id, cancellationToken);

        return new AlbumDto(
            album.Id,
            album.ArtistId,
            album.Title,
            album.Description,
            album.CoverImageUrl,
            album.IsPublic,
            album.ContentType?.ToString(),
            album.ReleaseDate,
            album.CreatedAt,
            tracks.Select(t => new AlbumTrackDto(t.MediaItemId, t.TrackOrder, t.AddedAt)).ToList());
    }
}
