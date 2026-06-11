// Đường dẫn: src/TuneVault.API/Controllers/UserController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Features.User.Queries.GetUserById;
using TuneVault.Application.Features.User.Queries.GetUserByIdDisplay;
using TuneVault.Application.Features.User.Queries.GetUserProfile;
using TuneVault.Application.Features.User.Queries.GetAllArtists;

namespace TuneVault.API.Controllers;

/// <summary>
/// Cung cấp các Endpoint API xử lý nghiệp vụ liên quan đến Người dùng (User).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Khởi tạo UserController thông qua cơ chế Dependency Injection của MediatR.
    /// </summary>
    /// <param name="mediator">Đối tượng ISender của MediatR để gửi Query/Command.</param>
    public UserController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy thông tin cơ bản của người dùng theo mã định danh Id hệ thống (ví dụ: U001).
    /// </summary>
    /// <param name="id">Mã định danh hệ thống của người dùng.</param>
    /// <returns>Đối tượng <see cref="UserDto"/> chứa thông tin cơ bản của người dùng.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Đã có lỗi hệ thống xảy ra.", detail = ex.Message }); }
    }

    /// <summary>
    /// Lấy thông tin cơ bản của người dùng theo IdDisplay (handle công khai, ví dụ: john_doe).
    /// </summary>
    /// <param name="idDisplay">Handle công khai của người dùng.</param>
    /// <returns>Đối tượng <see cref="UserDto"/> chứa thông tin cơ bản của người dùng.</returns>
    [HttpGet("display/{idDisplay}")]
    public async Task<IActionResult> GetByIdDisplay(string idDisplay)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdDisplayQuery(idDisplay));
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Đã có lỗi hệ thống xảy ra.", detail = ex.Message }); }
    }

    /// <summary>
    /// Lấy profile đầy đủ của người dùng theo Id hệ thống, bao gồm bio, avatar và số người theo dõi.
    /// </summary>
    /// <param name="id">Mã định danh hệ thống của người dùng.</param>
    /// <returns>Đối tượng <see cref="UserProfileDto"/> chứa toàn bộ thông tin profile của người dùng.</returns>
    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetProfile(string id)
    {
        try
        {
            var result = await _mediator.Send(new GetUserProfileQuery(id));
            return Ok(result);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = "Đã có lỗi hệ thống xảy ra.", detail = ex.Message }); }
    }

    /// <summary>
    /// Lấy danh sách tất cả người dùng đã được xác thực là nghệ sĩ (IsArtist = true) và đang hoạt động.
    /// </summary>
    /// <returns>Danh sách các đối tượng <see cref="UserDto"/> đại diện cho các nghệ sĩ trong hệ thống.</returns>
    [HttpGet("artists")]
    public async Task<IActionResult> GetAllArtists()
    {
        try
        {
            var result = await _mediator.Send(new GetAllArtistsQuery());
            return Ok(result);
        }
        catch (Exception ex) { return StatusCode(500, new { message = "Đã có lỗi hệ thống xảy ra.", detail = ex.Message }); }
    }
}