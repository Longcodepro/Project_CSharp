namespace TuneVault.Application.DTOs.User;

public sealed record UserProfileDto(
    string Id,
    string UserName,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string Role,
    string Rank);
