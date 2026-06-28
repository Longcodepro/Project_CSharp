using MediatR;
using TuneVault.Application.Features.Album.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;
using AlbumEntity = TuneVault.Domain.Entities.Album;

namespace TuneVault.Application.Features.Album.Commands.CreateAlbum;

/// <summary>
/// Handler tạo album mới cho nghệ sĩ hiện tại.
/// </summary>
public sealed class CreateAlbumCommandHandler : IRequestHandler<CreateAlbumCommand, AlbumDto>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Khởi tạo handler tạo album.
    /// </summary>
    public CreateAlbumCommandHandler(IAlbumRepository albumRepository, IUserRepository userRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Tạo album mới nếu user hiện tại là artist đang hoạt động.
    /// </summary>
    public async Task<AlbumDto> Handle(CreateAlbumCommand request, CancellationToken cancellationToken)
    {
        var artist = await _userRepository.GetByIdAsync(request.ArtistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy người dùng hiện tại.");

        if (!artist.IsActive)
            throw new DomainException("Tài khoản hiện tại không còn hoạt động.");

        if (!artist.IsArtist)
            throw new DomainException("Chỉ nghệ sĩ mới được tạo album.");

        var albumId = await GenerateNextAlbumIdAsync(cancellationToken);
        var contentType = ParseContentType(request.Request.ContentType);

        var album = new AlbumEntity(
            albumId,
            request.ArtistId,
            request.Request.Title,
            request.Request.Description,
            request.Request.CoverImageUrl,
            request.Request.IsPublic,
            contentType,
            request.Request.ReleaseDate);

        await _albumRepository.AddAsync(album, cancellationToken);

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
            Array.Empty<AlbumTrackDto>());
    }

    private static MediaType? ParseContentType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<MediaType>(value, ignoreCase: true, out var mediaType)
            && Enum.IsDefined(typeof(MediaType), mediaType))
            return mediaType;

        throw new DomainException("Kiểu nội dung album không hợp lệ.");
    }

    private async Task<string> GenerateNextAlbumIdAsync(CancellationToken cancellationToken)
    {
        const string prefix = "AL";

        var allAlbums = await _albumRepository.GetAllAsync(cancellationToken);

        var maxNumber = allAlbums
            .Select(a => a.Id)
            .Where(id => id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && id.Length > prefix.Length)
            .Select(id =>
            {
                var numberPart = id[prefix.Length..];
                return int.TryParse(numberPart, out var number) ? number : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{maxNumber + 1:D3}";
    }
}
