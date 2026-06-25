// API/Controllers/AuthController.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TuneVault.Application.Common;
using TuneVault.Application.Features.Auth.Commands.Login;
using TuneVault.Application.Features.Auth.Commands.ChangePassword;
using TuneVault.Application.Features.Auth.Commands.Register;
using TuneVault.Application.Features.Auth.Commands.SendOtp;
using TuneVault.Application.Features.Auth.Commands.ResetPassword;
using TuneVault.Application.Features.Auth.DTOs;
using TuneVault.Application.Interfaces;
using TuneVault.Domain.Exceptions;

namespace TuneVault.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private const string AccessTokenCookieName = "tunevault_access_token";
    private const string RefreshTokenCookieName = "tunevault_refresh_token";
    private readonly ISender _mediator;
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthController(ISender mediator, IConfiguration configuration, IJwtTokenGenerator jwtTokenGenerator)
    {
        _mediator = mediator;
        _configuration = configuration;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    /// <summary>
    /// Đăng xuất tài khoản hiện tại.
    /// Xóa toàn bộ cookie xác thực của TuneVault để đảm bảo không còn session dùng chung trong cùng trình duyệt.
    /// </summary>
    /// <returns>ApiResponse xác nhận đăng xuất thành công.</returns>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        ClearAuthCookies(Response);
        return Ok(ApiResponse<object?>.Ok(null, "Đăng xuất thành công. Vui lòng xóa token ở phía client."));
    }

    /// <summary>Đăng nhập, trả về JWT nếu thành công.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            WriteAuthCookies(Response, result);
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

    /// <summary>Gửi mã OTP đến email (purpose: "register", "reset_password" hoặc "change_password").</summary>
    [AllowAnonymous]
    [HttpPost("send-otp")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            WriteAuthCookies(Response, result);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Đăng ký tài khoản thành công."));
        }
        catch (DomainException ex)
        {
            return BadRequest(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    /// <summary>Đặt lại mật khẩu sau khi xác minh OTP.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
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

    /// <summary>Đổi mật khẩu cho tài khoản đang đăng nhập sau khi xác minh OTP.</summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(ApiResponse<object?>.Ok(null, "Đổi mật khẩu thành công."));
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

    /// <summary>Gia hạn session bằng refresh token đang lưu trong cookie httpOnly.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status401Unauthorized)]
    public IActionResult Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(ApiResponse<object?>.Fail("Thiếu refresh token."));

        try
        {
            var principal = ValidateRefreshToken(refreshToken);
            var tokenUse = principal.FindFirstValue("token_use");

            if (!string.Equals(tokenUse, "refresh", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

            var idDisplay = principal.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                ?? string.Empty;

            var roles = principal.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (roles.Count == 0)
            {
                roles.Add("Listener");
            }

            var roleCsv = string.Join(",", roles);
            var result = new AuthResponseDto(
                AccessToken: _jwtTokenGenerator.GenerateToken(userId, idDisplay, roleCsv),
                RefreshToken: _jwtTokenGenerator.GenerateRefreshToken(userId, idDisplay, roleCsv),
                UserId: userId,
                IdDisplay: idDisplay,
                Roles: roles);

            WriteAuthCookies(Response, result);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Gia hạn phiên đăng nhập thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            ClearAuthCookies(Response);
            return Unauthorized(ApiResponse<object?>.Fail(ex.Message));
        }
    }

    private static void WriteAuthCookies(HttpResponse response, AuthResponseDto result)
    {
        response.Cookies.Append(AccessTokenCookieName, result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = response.HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        response.Cookies.Append(RefreshTokenCookieName, result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = response.HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }

    private static void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookieName, new CookieOptions { Path = "/" });
        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/" });
    }

    private ClaimsPrincipal ValidateRefreshToken(string refreshToken)
    {
        var secretKey = ReadRequiredConfiguration("JwtSettings:SecretKey", "Không tìm thấy JWT SecretKey trong cấu hình.");
        var issuer = ReadRequiredConfiguration("JwtSettings:Issuer", "Không tìm thấy JWT Issuer trong cấu hình.");
        var audience = ReadRequiredConfiguration("JwtSettings:Audience", "Không tìm thấy JWT Audience trong cấu hình.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(5),
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = ClaimTypes.Role
        };

        return tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
    }

    private string ReadRequiredConfiguration(string key, string errorMessage)
    {
        var value = _configuration[key];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(errorMessage);
    }
}
