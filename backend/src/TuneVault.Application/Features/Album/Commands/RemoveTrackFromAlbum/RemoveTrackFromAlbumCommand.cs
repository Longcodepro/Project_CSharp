using MediatR;

namespace TuneVault.Application.Features.Album.Commands.RemoveTrackFromAlbum;

/// <summary>
/// Command xóa một media khỏi album.
/// </summary>
public sealed record RemoveTrackFromAlbumCommand(string AlbumId, string CurrentUserId, string MediaItemId) : IRequest;
