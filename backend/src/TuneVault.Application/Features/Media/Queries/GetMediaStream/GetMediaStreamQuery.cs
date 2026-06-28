using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using System.IO;

namespace TuneVault.Application.Features.Media.Queries.GetMediaStream;

/// <summary>
/// Query lấy thông tin file vật lý để stream một asset media.
/// </summary>
/// <param name="MediaId">Mã media cần stream.</param>
/// <param name="RequesterId">User hiện tại; null chỉ được xem asset public.</param>
/// <param name="AssetKind">Loại asset cần stream.</param>
public sealed record GetMediaStreamQuery(
    string MediaId,
    string? RequesterId = null,
    MediaAssetKind AssetKind = MediaAssetKind.Primary) : IRequest<MediaStreamDto?>;

/// <summary>
/// Xử lý truy vấn lấy thông tin stream cho một MediaItem.
/// </summary>
public sealed class GetMediaStreamQueryHandler : IRequestHandler<GetMediaStreamQuery, MediaStreamDto?>
{
    private readonly IMediaRepository _mediaRepository;

    public GetMediaStreamQueryHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<MediaStreamDto?> Handle(GetMediaStreamQuery request, CancellationToken ct)
    {
        var streamInfo = await _mediaRepository.GetStreamAsync(request.MediaId, request.RequesterId, request.AssetKind, ct);

        if (streamInfo is null)
        {
            return null;
        }

        var filePath = streamInfo.FilePath;

        var contentType = GetContentType(filePath);
        var supportsRange = true;

        return new MediaStreamDto(streamInfo.MediaId, filePath, contentType, supportsRange);
    }

    /// <summary>
    /// Xác định ContentType dựa trên extension file.
    /// </summary>
    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".flac" => "audio/flac",
            ".ogg" => "audio/ogg",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
