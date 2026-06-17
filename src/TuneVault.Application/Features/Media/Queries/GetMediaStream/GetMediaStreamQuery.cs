using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Interfaces;
using System.IO;

namespace TuneVault.Application.Features.Media.Queries.GetMediaStream;

/// <summary>
/// Query lấy thông tin file vật lý để stream một asset media.
/// </summary>
public sealed record GetMediaStreamQuery(string MediaId, MediaAssetKind AssetKind = MediaAssetKind.Primary) : IRequest<MediaStreamDto?>;

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
        // Bước 1: Lấy MediaStreamInfo từ repository dựa trên MediaId.
        var streamInfo = await _mediaRepository.GetStreamAsync(request.MediaId, request.AssetKind, ct);

        // Bước 2: Nếu không tìm thấy media, trả về null.
        if (streamInfo is null)
        {
            return null;
        }

        // Bước 3: Đường dẫn file vật lý đã được cung cấp bởi repository.
        var filePath = streamInfo.FilePath;

        // Bước 4: Kiểm tra sự tồn tại của file (nếu cần, có thể chuyển logic này ra Infrastructure hoặc API layer).
        // For now, we assume the repository ensures the path is valid or the API layer handles file existence.
        // If the file path is a URL, this check is not applicable here.

        // Bước 5: Xác định ContentType dựa trên extension file.
        var contentType = GetContentType(filePath);

        // Bước 6: Xác định xem file có hỗ trợ Range Request không (thường là file media lớn).
        var supportsRange = true; // Giả định hỗ trợ

        // Bước 7: Trả về DTO chứa thông tin stream.
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
            _ => "application/octet-stream" // Mặc định
        };
    }
}
