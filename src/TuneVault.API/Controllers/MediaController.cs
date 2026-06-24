using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.API.DTOs.Media;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Media.Commands.DeleteMedia;
using TuneVault.Application.Features.Media.Commands.GenerateMediaId;
using TuneVault.Application.Features.Media.Commands.UpdateMedia;
using TuneVault.Application.Features.Media.Commands.UploadMedia;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Application.Features.Media.Queries.GetMedia;
using TuneVault.Application.Features.Media.Queries.GetArtistMedia;
using TuneVault.Application.Features.Media.Queries.GetMediaById;
using TuneVault.Application.Features.Media.Queries.GetMediaStream;
using TuneVault.Application.Features.Media.Queries.GetUserMedia;
using TuneVault.Application.Features.User.Queries.GetUserById;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

[ApiController]
[Route("api/media")]
public sealed class MediaController : ControllerBase
{
    private static readonly HashSet<string> AllowedAudioExtensions = [".mp3", ".wav", ".m4a", ".flac", ".ogg"];
    private static readonly HashSet<string> AllowedAudioContentTypes =
    [
        "audio/mpeg",
        "audio/wav",
        "audio/x-wav",
        "audio/mp4",
        "audio/flac",
        "audio/ogg"
    ];

    private static readonly HashSet<string> AllowedVideoExtensions = [".mp4", ".webm"];
    private static readonly HashSet<string> AllowedVideoContentTypes = ["video/mp4", "video/webm"];
    private static readonly HashSet<string> AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly HashSet<string> AllowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];

    private readonly ISender _mediator;
    private readonly IFileStorageService _fileStorage;

    public MediaController(ISender mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub");

    private bool CurrentUserIsAdmin => User.IsInRole("Admin");

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(ApiResponse<object?>.Fail("Id không được để trống."));

        var result = await _mediator.Send(new GetMediaByIdQuery(id), ct);
        return result is not null
            ? Ok(ApiResponse<MediaItemDto>.Ok(result, "Lấy thông tin media thành công."))
            : NotFound(ApiResponse<object>.Fail($"Không tìm thấy media '{id}' hoặc media này đã bị xóa."));
    }

    [HttpGet("stream/{id}")]
    [Authorize]
    public async Task<IActionResult> Stream(string id, CancellationToken ct)
        => await StreamAssetAsync(id, MediaAssetKind.Primary, enableRangeProcessing: true, ct);

    [HttpGet("{id}/audio/stream")]
    [Authorize]
    public async Task<IActionResult> StreamAudio(string id, CancellationToken ct)
        => await StreamAssetAsync(id, MediaAssetKind.Audio, enableRangeProcessing: true, ct);

    [HttpGet("{id}/video/stream")]
    [Authorize]
    public async Task<IActionResult> StreamVideo(string id, CancellationToken ct)
        => await StreamAssetAsync(id, MediaAssetKind.Video, enableRangeProcessing: true, ct);

    [HttpGet("{id}/poster")]
    [AllowAnonymous]
    public async Task<IActionResult> Poster(string id, CancellationToken ct)
        => await StreamAssetAsync(id, MediaAssetKind.Poster, enableRangeProcessing: false, ct);

    private async Task<IActionResult> StreamAssetAsync(
        string id,
        MediaAssetKind assetKind,
        bool enableRangeProcessing,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(ApiResponse<object?>.Fail("Id media không được để trống."));

        var streamInfo = await _mediator.Send(new GetMediaStreamQuery(id, CurrentUserId, assetKind), ct);
        if (streamInfo is null)
            return NotFound(ApiResponse<object?>.Fail($"{GetMissingAssetMessage(assetKind)} Media có thể không tồn tại, đã bị xóa hoặc đang bị khóa do vi phạm."));

        if (Uri.IsWellFormedUriString(streamInfo.FilePath, UriKind.Absolute))
            return Redirect(streamInfo.FilePath);

        var physicalFilePath = ResolvePhysicalPath(streamInfo.FilePath, assetKind);
        if (!System.IO.File.Exists(physicalFilePath))
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy file media trên server. Vui lòng tải lại file hoặc kiểm tra dữ liệu upload của media này."));

        return PhysicalFile(physicalFilePath, streamInfo.ContentType, enableRangeProcessing: enableRangeProcessing);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetMedia([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var publicItems = await _mediator.Send(new GetMediaQuery(page, pageSize), ct);
        return Ok(ApiResponse<IReadOnlyCollection<MediaPublicDto>>.Ok(publicItems, "Lấy danh sách media công khai thành công."));
    }

    [HttpGet("my-media")]
    [Authorize]
    public async Task<IActionResult> GetUserMedia(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));

        var result = await _mediator.Send(new GetUserMediaQuery(userId), ct);
        return Ok(ApiResponse<IReadOnlyCollection<MediaOwnerDetailDto>>.Ok(result, "Lấy danh sách media của bạn thành công."));
    }

    [HttpGet("artist/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserMediaByArtist(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(ApiResponse<object?>.Fail("Id nghệ sĩ không được để trống."));

        var user = await _mediator.Send(new GetUserByIdQuery(userId), ct);
        if (user is null)
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy nghệ sĩ hoặc tài khoản này hiện không còn hoạt động."));

        if (!string.Equals(user.Role, "Artist", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<object?>.Fail("Người dùng được chỉ định không phải là nghệ sĩ."));

        if (!string.IsNullOrWhiteSpace(CurrentUserId) &&
            CurrentUserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
        {
            var ownerItems = await _mediator.Send(new GetUserMediaQuery(userId), ct);
            return Ok(ApiResponse<IReadOnlyCollection<MediaOwnerDetailDto>>.Ok(ownerItems, "Lấy danh sách media chi tiết của nghệ sĩ hiện tại thành công."));
        }

        var publicItems = await _mediator.Send(new GetArtistMediaQuery(userId), ct);
        return Ok(ApiResponse<IReadOnlyCollection<MediaPublicDto>>.Ok(publicItems, "Lấy danh sách media công khai của nghệ sĩ thành công."));
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Artist,Listener")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> Upload([FromForm] UploadMediaFormDto form, CancellationToken ct)
        => await UploadInternalAsync(form, requiredAssetKind: null, ct);

    [HttpPost("upload/audio")]
    [Authorize(Roles = "Artist,Listener")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> UploadAudio([FromForm] UploadMediaFormDto form, CancellationToken ct)
        => await UploadInternalAsync(form, MediaAssetKind.Audio, ct);

    [HttpPost("upload/video")]
    [Authorize(Roles = "Artist,Listener")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> UploadVideo([FromForm] UploadMediaFormDto form, CancellationToken ct)
        => await UploadInternalAsync(form, MediaAssetKind.Video, ct);

    private async Task<IActionResult> UploadInternalAsync(
        UploadMediaFormDto form,
        MediaAssetKind? requiredAssetKind,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));

        try
        {
            var mediaType = ResolveUploadMediaType(form, requiredAssetKind);
            var accessLevel = ResolveAccessLevel(form.AccessLevel);
            ValidateUploadFiles(form, mediaType);

            string? audioUrl = null;
            string? videoUrl = null;
            string? coverUrl = null;
            string? canvasUrl = null;
            string? savedAudioPath = null;
            string? savedVideoPath = null;
            string? savedCoverPath = null;
            string? savedCanvasPath = null;

            try
            {
                if (form.AudioFile is not null)
                {
                    savedAudioPath = await _fileStorage.SaveAsync(form.AudioFile, "media", ct);
                    audioUrl = BuildPublicUploadUrl("media", Path.GetFileName(savedAudioPath));
                }

                if (form.VideoFile is not null)
                {
                    savedVideoPath = await _fileStorage.SaveAsync(form.VideoFile, "video", ct);
                    videoUrl = BuildPublicUploadUrl("video", Path.GetFileName(savedVideoPath));
                }

                if (form.CoverImage is not null)
                {
                    savedCoverPath = await _fileStorage.SaveAsync(form.CoverImage, "media-covers", ct);
                    coverUrl = BuildPublicUploadUrl("media-covers", Path.GetFileName(savedCoverPath));
                }

                if (form.CanvasFile is not null)
                {
                    savedCanvasPath = await _fileStorage.SaveAsync(form.CanvasFile, "canvas", ct);
                    canvasUrl = BuildPublicUploadUrl("canvas", Path.GetFileName(savedCanvasPath));
                }

                var request = new UploadMediaRequestDto(
                    userId,
                    form.Title,
                    form.Description,
                    form.Genre,
                    mediaType.ToString(),
                    audioUrl,
                    videoUrl,
                    coverUrl,
                    canvasUrl,
                    (int)accessLevel,
                    form.IsPublic,
                    form.ReleaseDate,
                    form.FeaturedArtistIds);

                var mediaId = await _mediator.Send(new GenerateMediaIdCommand(), ct);
                var result = await _mediator.Send(new UploadMediaCommand(mediaId, request), ct);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    ApiResponse<MediaItemDto>.Ok(result, "Tải media lên thành công."));
            }
            catch
            {
                await DeleteSavedFileAsync(savedAudioPath, ct);
                await DeleteSavedFileAsync(savedVideoPath, ct);
                await DeleteSavedFileAsync(savedCoverPath, ct);
                await DeleteSavedFileAsync(savedCanvasPath, ct);
                throw;
            }
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Artist,Listener")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(string id, [FromForm] UpdateMediaFormRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(ApiResponse<object?>.Fail("Id không được để trống."));

        var requesterId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(requesterId))
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));

        try
        {
            ValidateUpdateFiles(request);

            string? savedCoverPath = null;
            string? savedCanvasPath = null;
            string? savedMediaPath = null;

            try
            {
                string? mediaUrl = null;
                string? coverUrl = null;
                string? canvasUrl = null;

                if (request.VideoFile is not null)
                {
                    EnsureFileType(request.VideoFile, AllowedVideoExtensions, AllowedVideoContentTypes, "File video chỉ hỗ trợ mp4 hoặc webm.");
                    savedMediaPath = await _fileStorage.SaveAsync(request.VideoFile, "video", ct);
                    mediaUrl = BuildPublicUploadUrl("video", Path.GetFileName(savedMediaPath));
                }

                if (request.AudioFile is not null)
                {
                    EnsureFileType(request.AudioFile, AllowedAudioExtensions, AllowedAudioContentTypes, "File audio chỉ hỗ trợ mp3, wav, m4a, flac hoặc ogg.");
                    savedMediaPath = await _fileStorage.SaveAsync(request.AudioFile, "media", ct);
                    mediaUrl = BuildPublicUploadUrl("media", Path.GetFileName(savedMediaPath));
                }

                if (request.CoverImage is not null)
                {
                    savedCoverPath = await _fileStorage.SaveAsync(request.CoverImage, "media-covers", ct);
                    coverUrl = BuildPublicUploadUrl("media-covers", Path.GetFileName(savedCoverPath));
                }

                if (request.CanvasFile is not null)
                {
                    savedCanvasPath = await _fileStorage.SaveAsync(request.CanvasFile, "canvas", ct);
                    canvasUrl = BuildPublicUploadUrl("canvas", Path.GetFileName(savedCanvasPath));
                }

                var commandRequest = new UpdateMediaRequestDto(
                    request.Title,
                    request.Description,
                    request.Genre,
                    mediaUrl,
                    coverUrl,
                    canvasUrl,
                    request.IsPublic,
                    request.AccessLevel);

                var result = await _mediator.Send(new UpdateMediaCommand(id, requesterId, commandRequest), ct);
                return result is not null
                    ? Ok(ApiResponse<MediaItemDto>.Ok(result, "Cập nhật media thành công."))
                    : BadRequest(ApiResponse<object?>.Fail($"Không thể cập nhật media '{id}'. Media có thể không tồn tại, đã bị xóa hoặc bạn không có quyền chỉnh sửa."));
            }
            catch
            {
                await DeleteSavedFileAsync(savedMediaPath, ct);
                await DeleteSavedFileAsync(savedCoverPath, ct);
                await DeleteSavedFileAsync(savedCanvasPath, ct);
                throw;
            }
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Artist,Listener")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(ApiResponse<object?>.Fail("Id không được để trống."));

        var requesterId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(requesterId))
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));

        try
        {
            var result = await _mediator.Send(new DeleteMediaCommand(id, requesterId), ct);
            return result
                ? Ok(ApiResponse<bool>.Ok(true, "Xóa bài hát thành công."))
                : BadRequest(ApiResponse<bool>.Fail($"Không thể xóa media '{id}'. Media có thể không tồn tại, đã bị xóa hoặc bạn không có quyền xóa."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    private static MediaType ResolveUploadMediaType(UploadMediaFormDto form, MediaAssetKind? requiredAssetKind)
    {
        if (requiredAssetKind == MediaAssetKind.Audio)
            return MediaType.Audio;

        if (requiredAssetKind == MediaAssetKind.Video)
            return MediaType.Video;

        if (!Enum.TryParse<MediaType>(form.Type, ignoreCase: true, out var mediaType))
            throw new DomainException($"Loại media '{form.Type}' không hợp lệ.");

        return mediaType;
    }

    private static AccessLevel ResolveAccessLevel(string rawAccessLevel)
    {
        if (int.TryParse(rawAccessLevel, out var numericAccessLevel) &&
            Enum.IsDefined(typeof(AccessLevel), numericAccessLevel))
        {
            return (AccessLevel)numericAccessLevel;
        }

        if (Enum.TryParse<AccessLevel>(rawAccessLevel, ignoreCase: true, out var accessLevel))
            return accessLevel;

        throw new DomainException("Cấp độ truy cập không hợp lệ. Vui lòng nhập Normal, Premium, 0 hoặc 1.");
    }

    private static void ValidateUploadFiles(UploadMediaFormDto form, MediaType mediaType)
    {
        if (mediaType == MediaType.Video)
        {
            EnsureFile(form.VideoFile, "Vui lòng chọn file video.");
            EnsureFileType(form.VideoFile!, AllowedVideoExtensions, AllowedVideoContentTypes, "File video chỉ hỗ trợ mp4 hoặc webm.");
        }
        else
        {
            EnsureFile(form.AudioFile, "Vui lòng chọn file audio.");
            EnsureFileType(form.AudioFile!, AllowedAudioExtensions, AllowedAudioContentTypes, "File audio chỉ hỗ trợ mp3, wav, m4a, flac hoặc ogg.");
        }

        if (form.CoverImage is not null)
            EnsureFileType(form.CoverImage, AllowedImageExtensions, AllowedImageContentTypes, "Ảnh poster chỉ hỗ trợ jpg, jpeg, png hoặc webp.");

        if (form.CanvasFile is not null)
        {
            if (mediaType == MediaType.Video)
                throw new DomainException("Media dạng video không được dùng canvas riêng.");

            EnsureFileType(form.CanvasFile, AllowedVideoExtensions, AllowedVideoContentTypes, "Canvas chỉ hỗ trợ mp4 hoặc webm.");
        }
    }

    private static void ValidateUpdateFiles(UpdateMediaFormRequestDto form)
    {
        if (form.AudioFile is not null)
            EnsureFileType(form.AudioFile, AllowedAudioExtensions, AllowedAudioContentTypes, "File audio chỉ hỗ trợ mp3, wav, m4a, flac hoặc ogg.");

        if (form.VideoFile is not null)
            EnsureFileType(form.VideoFile, AllowedVideoExtensions, AllowedVideoContentTypes, "File video chỉ hỗ trợ mp4 hoặc webm.");

        if (form.CoverImage is not null)
            EnsureFileType(form.CoverImage, AllowedImageExtensions, AllowedImageContentTypes, "Ảnh poster chỉ hỗ trợ jpg, jpeg, png hoặc webp.");

        if (form.CanvasFile is not null)
            EnsureFileType(form.CanvasFile, AllowedVideoExtensions, AllowedVideoContentTypes, "Canvas chỉ hỗ trợ mp4 hoặc webm.");
    }

    private static void EnsureFile(IFormFile? file, string message)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException(message);
    }

    private static void EnsureFileType(
        IFormFile file,
        ISet<string> allowedExtensions,
        ISet<string> allowedContentTypes,
        string message)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var contentType = file.ContentType.ToLowerInvariant();

        if (!allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(contentType))
            throw new ArgumentException(message);
    }

    private static string GetMissingAssetMessage(MediaAssetKind assetKind)
    {
        return assetKind switch
        {
            MediaAssetKind.Audio => "Không tìm thấy file audio.",
            MediaAssetKind.Video => "Không tìm thấy file video.",
            MediaAssetKind.Poster => "Không tìm thấy poster media.",
            _ => "Không tìm thấy media."
        };
    }

    private static string BuildPublicUploadUrl(string folderName, string fileName)
        => $"/uploads/{folderName}/{fileName}";

    private async Task DeleteSavedFileAsync(string? physicalPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(physicalPath))
            return;

        await _fileStorage.DeleteAsync(physicalPath, ct);
    }

    private static string ResolvePhysicalPath(string storedPath, MediaAssetKind assetKind)
    {
        var candidatePaths = new List<string>();

        if (Path.IsPathRooted(storedPath) &&
            !storedPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            if (System.IO.File.Exists(storedPath))
                return storedPath;

            var normalizedPath = storedPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var uploadMarker = $"{Path.DirectorySeparatorChar}wwwroot{Path.DirectorySeparatorChar}uploads{Path.DirectorySeparatorChar}";
            var uploadIndex = normalizedPath.IndexOf(uploadMarker, StringComparison.OrdinalIgnoreCase);
            if (uploadIndex >= 0)
            {
                var relativeUploadPath = normalizedPath[(uploadIndex + uploadMarker.Length)..];
                candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", relativeUploadPath));
            }
            else
            {
                candidatePaths.Add(storedPath);
            }
        }
        else if (storedPath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            var relativePath = storedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath));
        }
        else
        {
            candidatePaths.Add(storedPath);
        }

        var fileName = Path.GetFileName(storedPath);
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var fallbackFolder = assetKind switch
            {
                MediaAssetKind.Audio => "media",
                MediaAssetKind.Video => "video",
                MediaAssetKind.Poster => "media-covers",
                _ => "media"
            };

            candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fallbackFolder, fileName));

            if (assetKind == MediaAssetKind.Poster)
            {
                candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers", fileName));
                candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "default-cover", fileName));
            }

            var fileStem = Path.GetFileNameWithoutExtension(fileName);
            if (!string.IsNullOrWhiteSpace(fileStem))
            {
                var stemFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fallbackFolder);
                candidatePaths.Add(Path.Combine(stemFolder, fileStem));
                candidatePaths.AddRange(ExpandStemCandidates(stemFolder, fileStem));

                if (assetKind == MediaAssetKind.Poster)
                {
                    candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "media-covers", fileStem));
                    candidatePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers", fileStem));
                    candidatePaths.AddRange(ExpandStemCandidates(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "media-covers"), fileStem));
                    candidatePaths.AddRange(ExpandStemCandidates(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers"), fileStem));
                }
            }
        }

        foreach (var candidatePath in candidatePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(candidatePath) && System.IO.File.Exists(candidatePath))
                return candidatePath;
        }

        return candidatePaths.FirstOrDefault() ?? storedPath;
    }

    private static IEnumerable<string> ExpandStemCandidates(string folderPath, string fileStem)
    {
        if (!Directory.Exists(folderPath))
            return Array.Empty<string>();

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp3", ".wav", ".m4a", ".flac", ".ogg", ".mp4", ".webm" };
        return allowedExtensions.Select(ext => Path.Combine(folderPath, fileStem + ext));
    }
}
