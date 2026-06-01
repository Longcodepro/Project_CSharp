namespace TuneVault.Application.DTOs.Auth;

public sealed record AuthResponseDto(string AccessToken, string UserId, string UserName, string Email);
