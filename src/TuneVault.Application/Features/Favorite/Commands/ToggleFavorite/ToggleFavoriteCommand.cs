using TuneVault.Application.Features.Favorite.DTOs;

namespace TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;

public sealed record ToggleFavoriteCommand(string UserId, ToggleFavoriteRequestDto Request);
