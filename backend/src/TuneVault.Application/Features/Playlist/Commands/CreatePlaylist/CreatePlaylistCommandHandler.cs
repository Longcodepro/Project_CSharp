using MediatR;
using TuneVault.Application.Features.Playlist.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;
using PlaylistEntity = TuneVault.Domain.Entities.Playlist;

namespace TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;

/// <summary>
/// Tạo playlist mới cho người dùng.
/// </summary>
public sealed class CreatePlaylistCommandHandler : IRequestHandler<CreatePlaylistCommand, PlaylistDto>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler tạo playlist.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public CreatePlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Tạo playlist, lưu xuống database và trả về DTO.
    /// </summary>
    /// <param name="command">Command chứa OwnerId và thông tin Playlist cần tạo.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>PlaylistDto đại diện cho Playlist vừa được tạo.</returns>
    public async Task<PlaylistDto> Handle(CreatePlaylistCommand command, CancellationToken cancellationToken = default)
    {
        var playlistId = await GenerateNextIdAsync(cancellationToken);

        var contentType = ParseContentType(command.Request.ContentType);

        var playlist = new PlaylistEntity(
            playlistId,
            command.OwnerId,
            command.Request.Title,
            command.Request.Description,
            command.Request.CoverImgUrl,
            command.Request.IsPublic,
            contentType,
            command.Request.ReleaseDate
        );

        await _playlistRepository.AddAsync(playlist, cancellationToken);

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
            Array.Empty<PlaylistTrackDto>()
        );
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

    /// <summary>
    /// Sinh ID tiếp theo theo format PL001, PL002...
    /// Lấy ID lớn nhất trong DB, tách phần chữ và phần số, tăng số lên 1.
    /// Ví dụ: PL006 → tách ra PL + 006 → tăng lên 007 → ghép lại PL007
    /// </summary>
    private async Task<string> GenerateNextIdAsync(CancellationToken cancellationToken)
    {
        const string prefix = "PL";

        // Lấy tất cả playlist từ DB
        var allPlaylists = await _playlistRepository.GetAllAsync(cancellationToken);

        // Lọc các ID có đúng format PL + số, lấy số lớn nhất
        var maxNumber = allPlaylists
            .Select(p => p.Id)
            .Where(id => id.StartsWith(prefix) && id.Length > prefix.Length)
            .Select(id =>
            {
                var numberPart = id.Substring(prefix.Length);
                return int.TryParse(numberPart, out var num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        // Tăng số lên 1, format 3 chữ số (001, 002...)
        var nextNumber = maxNumber + 1;
        return $"{prefix}{nextNumber:D3}";
    }
}
