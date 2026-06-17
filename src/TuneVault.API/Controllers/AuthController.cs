// API/Controllers/AuthController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Auth.Commands.Login;
using TuneVault.Application.Features.Auth.Commands.Register;
using TuneVault.Application.Features.Auth.Commands.SendOtp;
using TuneVault.Application.Features.Auth.Commands.ResetPassword;
using TuneVault.Application.Features.Auth.DTOs;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator) => _mediator = mediator;

    /// <summary>Đăng nhập, trả về JWT nếu thành công.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Đăng nhập thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    /// <summary>Gửi mã OTP đến email (purpose: "register" hoặc "reset_password").</summary>
    [HttpPost("send-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(ApiResponse<object?>.Ok(null, "OTP đã được gửi thành công."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    /// <summary>Đăng ký tài khoản mới sau khi xác minh OTP.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Đăng ký tài khoản thành công."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    /// <summary>Đặt lại mật khẩu sau khi xác minh OTP.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(ApiResponse<object?>.Ok(null, "Mật khẩu đã được đặt lại thành công."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }
}
