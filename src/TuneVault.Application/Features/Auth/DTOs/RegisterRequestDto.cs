namespace TuneVault.Application.Features.Auth.DTOs;

public sealed record RegisterRequestDto(string UserName, string Email, string Password, string? DisplayName);