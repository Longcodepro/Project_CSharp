using TuneVault.Application.DTOs.Playlist;

namespace TuneVault.Application.Features.Playlist.Commands.CreatePlaylist;

public sealed record CreatePlaylistCommand(string OwnerId, CreatePlaylistRequestDto Request);
