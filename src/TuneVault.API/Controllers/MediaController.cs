using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Abstractions;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Media.Commands.DeleteMedia;
using TuneVault.Application.Features.Media.Commands.GenerateMediaId;
using TuneVault.Application.Features.Media.Commands.UpdateMedia;
using TuneVault.Application.Features.Media.Commands.UploadMedia;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Application.Features.Media.Queries.GetMedia;
using TuneVault.Application.Features.Media.Queries.GetMediaById;
using TuneVault.Application.Features.Media.Queries.GetMediaStream;
using TuneVault.Application.Features.Media.Queries.GetUserMedia;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

[ApiController]
[Route("api/media")]
public sealed class MediaController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IFileStorageService _fileStorage;

    public MediaController(ISender mediator, IFileStorageService fileStorage)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub");

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
    [AllowAnonymous]
    public async Task<IActionResult> Stream(string id, CancellationToken ct)
        => await StreamAssetAsync(id, MediaAssetKind.Primary, enableRangeProcessing: true, ct);

    [HttpGet("{id}/audio/stream")]
    [AllowAnonymous]
    public async Task<IActionResult> StreamAudio(string id, CancellationToken ct)
        => await StreamAssetAsync(id, MediaAssetKind.Audio, enableRangeProcessing: true, ct);

    [HttpGet("{id}/video/stream")]
    [AllowAnonymous]
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

        var streamInfo = await _mediator.Send(new GetMediaStreamQuery(id, assetKind), ct);
        if (streamInfo is null)
            return NotFound(ApiResponse<object?>.Fail($"{GetMissingAssetMessage(assetKind)} Media có thể không tồn tại, đã bị xóa hoặc đang bị khóa do vi phạm."));

        if (Uri.IsWellFormedUriString(streamInfo.FilePath, UriKind.Absolute))
            return Redirect(streamInfo.FilePath);

        if (!System.IO.File.Exists(streamInfo.FilePath))
            return NotFound(ApiResponse<object?>.Fail("Không tìm thấy file media trên server. Vui lòng tải lại file hoặc kiểm tra dữ liệu upload của media này."));

        return PhysicalFile(streamInfo.FilePath, streamInfo.ContentType, enableRangeProcessing: enableRangeProcessing);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetMedia([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMediaQuery(page, pageSize), ct);
        return Ok(ApiResponse<IEnumerable<MediaItemDto>>.Ok(result, "Lấy danh sách media thành công."));
    }

    [HttpGet("my-media")]
    [Authorize]
    public async Task<IActionResult> GetUserMedia(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));

        var result = await _mediator.Send(new GetUserMediaQuery(userId), ct);
        return Ok(ApiResponse<IEnumerable<MediaItemDto>>.Ok(result, "Lấy danh sách media của bạn thành công."));
    }

    [HttpGet("artist/{userId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserMediaByArtist(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(ApiResponse<object?>.Fail("Id nghệ sĩ không được để trống."));

        var result = await _mediator.Send(new GetUserMediaQuery(userId), ct);
        return Ok(ApiResponse<IEnumerable<MediaItemDto>>.Ok(result, "Lấy danh sách media của nghệ sĩ thành công."));
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Artist,Admin")]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> Upload([FromForm] UploadMediaFormDto form, CancellationToken ct)
        => await UploadInternalAsync(form, requiredAssetKind: null, ct);

    [HttpPost("upload/audio")]
    [Authorize(Roles = "Artist,Admin")]
    [RequestSizeLimit(200_000_000)]
    public async Task<IActionResult> UploadAudio([FromForm] UploadMediaFormDto form, CancellationToken ct)
        => await UploadInternalAsync(form, MediaAssetKind.Audio, ct);

    [HttpPost("upload/video")]
    [Authorize(Roles = "Artist,Admin")]
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

            if (form.AudioFile is not null)
                audioUrl = await _fileStorage.SaveAsync(form.AudioFile, "audio", ct);

            if (form.VideoFile is not null)
                videoUrl = await _fileStorage.SaveAsync(form.VideoFile, "video", ct);

            if (form.CoverImage is not null)
                coverUrl = await _fileStorage.SaveAsync(form.CoverImage, "media-covers", ct);

            var request = new UploadMediaRequestDto(
                userId,
                form.Title,
                form.Description,
                form.Genre,
                mediaType.ToString(),
                audioUrl,
                videoUrl,
                coverUrl,
                null,
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
    [Authorize(Roles = "Artist,Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMediaRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(ApiResponse<object?>.Fail("Id không được để trống."));

        var requesterId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(requesterId))
            return Unauthorized(ApiResponse<object?>.Fail("Bạn cần đăng nhập để thực hiện thao tác này."));

        try
        {
            var result = await _mediator.Send(new UpdateMediaCommand(id, requesterId, request), ct);
            return result is not null
                ? Ok(ApiResponse<MediaItemDto>.Ok(result, "Cập nhật media thành công."))
                : BadRequest(ApiResponse<object?>.Fail($"Không thể cập nhật media '{id}'. Media có thể không tồn tại, đã bị xóa hoặc bạn không có quyền chỉnh sửa."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Artist,Admin")]
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
            EnsureExtension(form.VideoFile!, [".mp4", ".webm"], "File video chỉ hỗ trợ mp4 hoặc webm.");
        }
        else
        {
            EnsureFile(form.AudioFile, "Vui lòng chọn file audio.");
            EnsureExtension(form.AudioFile!, [".mp3", ".wav", ".m4a", ".flac", ".ogg"], "File audio chỉ hỗ trợ mp3, wav, m4a, flac hoặc ogg.");
        }

        if (form.CoverImage is not null)
            EnsureExtension(form.CoverImage, [".jpg", ".jpeg", ".png", ".webp"], "Ảnh poster chỉ hỗ trợ jpg, jpeg, png hoặc webp.");
    }

    private static void EnsureFile(IFormFile? file, string message)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException(message);
    }

    private static void EnsureExtension(IFormFile file, string[] allowedExtensions, string message)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
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
}
