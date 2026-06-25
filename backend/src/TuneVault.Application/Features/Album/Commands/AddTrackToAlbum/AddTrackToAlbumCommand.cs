using MediatR;
using TuneVault.Application.Features.Album.DTOs;

namespace TuneVault.Application.Features.Album.Commands.AddTrackToAlbum;

/// <summary>
/// Command thêm một media vào album.
/// </summary>
public sealed record AddTrackToAlbumCommand(string AlbumId, string CurrentUserId, AddTrackToAlbumRequestDto Request) : IRequest;
