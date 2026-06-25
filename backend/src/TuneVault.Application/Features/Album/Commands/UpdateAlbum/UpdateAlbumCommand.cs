using MediatR;
using TuneVault.Application.Features.Album.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Commands.UpdateAlbum;

/// <summary>
/// Command cập nhật album của artist hiện tại.
/// </summary>
public sealed record UpdateAlbumCommand(string AlbumId, string CurrentUserId, UpdateAlbumRequestDto Request) : IRequest<AlbumDto>;

/// <summary>
/// Handler cập nhật thông tin album và giữ rule ngày phát hành không đổi sau khi đã phát hành.
/// </summary>
public sealed class UpdateAlbumCommandHandler : IRequestHandler<UpdateAlbumCommand, AlbumDto>
{
    private readonly IAlbumRepository _albumRepository;

    /// <summary>
    /// Khởi tạo handler cập nhật album.
    /// </summary>
    public UpdateAlbumCommandHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
    }

    /// <summary>
    /// Cập nhật album nếu người thao tác là owner.
    /// </summary>
    public async Task<AlbumDto> Handle(UpdateAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(request.AlbumId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy album.");

        if (!string.Equals(album.ArtistId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền cập nhật album này.");

        album.Rename(request.Request.Title);
        album.UpdateDescription(request.Request.Description);
        album.UpdateCoverImage(request.Request.CoverImageUrl);
        album.SetPublic(request.Request.IsPublic);
        album.SetContentType(ParseContentType(request.Request.ContentType));
        album.SetReleaseDate(request.Request.ReleaseDate);

        await _albumRepository.UpdateAsync(album, cancellationToken);

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

    private static MediaType? ParseContentType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<MediaType>(value, ignoreCase: true, out var mediaType))
            return mediaType;

        throw new DomainException("Kiểu nội dung album không hợp lệ.");
    }
}
