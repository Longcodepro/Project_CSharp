using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.History.Commands;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller cung cấp các API lịch sử nghe nhạc của người dùng trong TuneVault.
/// Controller không gọi DAO trực tiếp nữa.
/// </summary>
public sealed class HistoryController : BaseApiController
{
    private readonly RecordPlayHistoryCommand _recordPlayHistoryCommand;

    public HistoryController(RecordPlayHistoryCommand recordPlayHistoryCommand)
    {
        _recordPlayHistoryCommand = recordPlayHistoryCommand;
    }

    /// <summary>
    /// Lấy danh sách bài hát nghe gần đây của một người dùng.
    /// Mặc định trả về 10 bài gần nhất.
    /// </summary>
    [HttpGet("recent/{userId}")]
    public async Task<IActionResult> GetRecent(
        string userId,
        [FromQuery] int limit = 10)
    {
        var items = await _recordPlayHistoryCommand.GetRecentAsync(
            userId,
            limit);

        return Ok(items);
    }

    /// <summary>
    /// Ghi nhận một lần nghe bài hát của người dùng vào PlayHistory.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordPlayHistoryRequest request)
    {
        var result = await _recordPlayHistoryCommand.RecordAsync(
            request.UserId,
            request.MediaItemId,
            request.StoppedAt);

        return Ok(new
        {
            success = result,
            message = "Đã ghi lịch sử nghe",
            request.UserId,
            request.MediaItemId,
            request.StoppedAt
        });
    }
}

/// <summary>
/// Request body dùng để ghi nhận lịch sử nghe bài hát.
/// </summary>
public sealed record RecordPlayHistoryRequest(
    string UserId,
    string MediaItemId,
    double? StoppedAt);