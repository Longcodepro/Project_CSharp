// API/Controllers/MediaController.cs
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.Media.Commands.DeleteMedia;
using TuneVault.Application.Features.Media.Commands.GenerateMediaId; // Added using directive
using TuneVault.Application.Features.Media.Commands.UpdateMedia;
using TuneVault.Application.Features.Media.Commands.UploadMedia;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Application.Features.Media.Queries.GetMediaById;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController : ControllerBase
{
    private readonly ISender _mediator;

    public MediaController(ISender mediator) => _mediator = mediator;

    // Lấy UserId từ JWT claim (sub)
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub");

    #region Queries

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { success = false, data = (object?)null, message = "Id không được để trống." });

        var result = await _mediator.Send(new GetMediaByIdQuery(id), ct);
        return result is not null
            ? Ok(new { success = true, data = result, message = (string?)null })
            : NotFound(new { success = false, data = (object?)null, message = $"Không tìm thấy bài hát '{id}'." });
    }

    #endregion

    #region Commands

    [HttpPost]
    [Authorize(Roles = "Artist,Admin")]
    public async Task<IActionResult> Upload([FromBody] UploadMediaRequestDto request, CancellationToken ct)
    {
        if (request is null)
            return BadRequest(new { success = false, data = (object?)null, message = "Dữ liệu không hợp lệ." });

        try
        {
            var mediaId = await _mediator.Send(new GenerateMediaIdCommand(), ct); // Use command to generate sequential ID
            var result  = await _mediator.Send(new UploadMediaCommand(mediaId, request), ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, data = result, message = (string?)null });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Artist,Admin")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateMediaRequestDto request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { success = false, data = (object?)null, message = "Id không được để trống." });

        // Lấy requesterId từ JWT, không nhận từ client (bảo mật)
        var requesterId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(requesterId))
            return Unauthorized(new { success = false, data = (object?)null, message = "Chưa xác thực." });

        try
        {
            var result = await _mediator.Send(new UpdateMediaCommand(id, requesterId, request), ct);
            return result is not null
                ? Ok(new { success = true, data = result, message = (string?)null })
                : BadRequest(new { success = false, data = (object?)null, message = "Không thể cập nhật bài hát." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Artist,Admin")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { success = false, data = (object?)null, message = "Id không được để trống." });

        // Lấy requesterId từ JWT, không nhận từ client
        var requesterId = CurrentUserId;
        if (string.IsNullOrWhiteSpace(requesterId))
            return Unauthorized(new { success = false, data = (object?)null, message = "Chưa xác thực." });

        try
        {
            var result = await _mediator.Send(new DeleteMediaCommand(id, requesterId), ct);
            return result
                ? Ok(new { success = true, data = (object?)null, message = "Xóa bài hát thành công." })
                : BadRequest(new { success = false, data = (object?)null, message = "Không thể xóa bài hát." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, data = (object?)null, message = ex.Message });
        }
    }

    #endregion
}