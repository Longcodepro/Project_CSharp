using MediatR;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.DTOs.Media;
using TuneVault.Application.Features.Media.Commands.DeleteMedia;
using TuneVault.Application.Features.Media.Commands.UpdateMedia;
using TuneVault.Application.Features.Media.Commands.UploadMedia;
using TuneVault.Application.Features.Media.Queries.GetMediaById;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller quản lý các tác vụ liên quan đến MediaItem (bài hát/media) trong TuneVault.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Khởi tạo Controller với MediatR.
    /// </summary>
    public MediaController(IMediator mediator) => _mediator = mediator;

    #region Queries

    /// <summary> Lấy thông tin metadata của một bài hát theo Id. </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Id bài hát không được để trống.");

        var result = await _mediator.Send(new GetMediaByIdQuery(id), ct);
        return result is not null ? Ok(result) : NotFound($"Không tìm thấy bài hát với Id '{id}'.");
    }

    #endregion

    #region Commands

    /// <summary> Upload một bài hát mới. MediaId được hệ thống tự sinh. </summary>
    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] UploadMediaRequestDto request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest("Dữ liệu bài hát không hợp lệ.");

        // MediaId do hệ thống tự sinh (chưa có dịch vụ sinh Id tuần tự — dùng định danh duy nhất với tiền tố 'I').
        var mediaId = $"I{Guid.NewGuid():N}";

        var result = await _mediator.Send(new UploadMediaCommand(mediaId, request), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary> Cập nhật metadata của một bài hát. Chỉ ca sĩ chính (Owner) mới có quyền. </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromQuery] string requesterId,
        [FromBody] UpdateMediaRequestDto request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Id bài hát không được để trống.");

        if (string.IsNullOrWhiteSpace(requesterId))
            return BadRequest("RequesterId không được để trống.");

        if (request is null)
            return BadRequest("Dữ liệu cập nhật không hợp lệ.");

        var result = await _mediator.Send(new UpdateMediaCommand(id, requesterId, request), ct);
        return result is not null ? Ok(result) : BadRequest("Không thể cập nhật bài hát.");
    }

    /// <summary> Xóa mềm một bài hát (IsActive = false). Chỉ ca sĩ chính (Owner) mới có quyền. </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string requesterId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("Id bài hát không được để trống.");

        if (string.IsNullOrWhiteSpace(requesterId))
            return BadRequest("RequesterId không được để trống.");

        var result = await _mediator.Send(new DeleteMediaCommand(id, requesterId), ct);
        return result ? Ok() : BadRequest("Không thể xóa bài hát.");
    }

    #endregion
}
