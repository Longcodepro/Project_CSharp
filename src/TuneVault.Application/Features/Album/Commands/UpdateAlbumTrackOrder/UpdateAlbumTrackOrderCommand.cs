using MediatR;

namespace TuneVault.Application.Features.Album.Commands.UpdateAlbumTrackOrder;

/// <summary>
/// Command cập nhật vị trí của một media trong album.
/// </summary>
public sealed record UpdateAlbumTrackOrderCommand(string AlbumId, string CurrentUserId, string MediaItemId, int NewOrder) : IRequest;
