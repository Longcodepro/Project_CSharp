using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.Follow.Commands;

namespace TuneVault.API.Controllers;

/// <summary>
/// Controller cung cấp các API Follow/Unfollow giữa người dùng và nghệ sĩ trong TuneVault.
/// Controller không gọi DAO nữa.
/// </summary>
public sealed class FollowController : BaseApiController
{
    private readonly IFollowSqlRepository _followRepository;
    private readonly FollowUserCommand _followUserCommand;
    private readonly UnFollowUserCommand _unFollowUserCommand;

    public FollowController(
        IFollowSqlRepository followRepository,
        FollowUserCommand followUserCommand,
        UnFollowUserCommand unFollowUserCommand)
    {
        _followRepository = followRepository;
        _followUserCommand = followUserCommand;
        _unFollowUserCommand = unFollowUserCommand;
    }

    /// <summary>
    /// Follow một nghệ sĩ/người dùng khác.
    /// Khi follow thành công, hệ thống tạo thông báo NewFollower cho người được follow.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Follow([FromBody] FollowRequest request)
    {
        try
        {
            var result = await _followUserCommand.ExecuteAsync(
                request.FollowerId,
                request.FolloweeId);

            if (!result)
                return Ok(new
                {
                    success = false,
                    message = "Đã follow trước đó"
                });

            return Ok(new
            {
                success = true,
                message = "Follow thành công",
                request.FollowerId,
                request.FolloweeId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Bỏ follow một nghệ sĩ/người dùng khác.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Unfollow([FromBody] FollowRequest request)
    {
        try
        {
            var result = await _unFollowUserCommand.ExecuteAsync(
                request.FollowerId,
                request.FolloweeId);

            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy quan hệ follow để xóa"
                });

            return Ok(new
            {
                success = true,
                message = "Unfollow thành công",
                request.FollowerId,
                request.FolloweeId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Kiểm tra một người dùng có đang follow một nghệ sĩ/người dùng khác hay không.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> IsFollowing(
        [FromQuery] string followerId,
        [FromQuery] string followeeId)
    {
        var isFollowing = await _followRepository.IsFollowingAsync(
            followerId,
            followeeId);

        return Ok(new
        {
            followerId,
            followeeId,
            isFollowing
        });
    }

    /// <summary>
    /// Lấy danh sách nghệ sĩ mà người dùng đang follow.
    /// </summary>
    [HttpGet("following/{userId}")]
    public async Task<IActionResult> GetFollowing(string userId)
    {
        var items = await _followRepository.GetFollowingAsync(userId);
        return Ok(items);
    }

    /// <summary>
    /// Lấy danh sách người đang follow một nghệ sĩ/người dùng.
    /// </summary>
    [HttpGet("followers/{artistId}")]
    public async Task<IActionResult> GetFollowers(string artistId)
    {
        var items = await _followRepository.GetFollowersAsync(artistId);
        return Ok(items);
    }

    /// <summary>
    /// Đếm số follower của một nghệ sĩ/người dùng.
    /// </summary>
    [HttpGet("followers-count/{artistId}")]
    public async Task<IActionResult> CountFollowers(string artistId)
    {
        var count = await _followRepository.CountFollowersAsync(artistId);

        return Ok(new
        {
            artistId,
            followerCount = count
        });
    }
}

/// <summary>
/// Request body dùng cho thao tác Follow và Unfollow.
/// </summary>
public sealed record FollowRequest(string FollowerId, string FolloweeId);