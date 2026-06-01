namespace TuneVault.Application.DTOs.Auth;

public sealed record RegisterRequestDto(string UserName, string Email, string Password, string? DisplayName);
