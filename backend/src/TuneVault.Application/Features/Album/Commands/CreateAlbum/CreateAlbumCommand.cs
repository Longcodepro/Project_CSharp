using MediatR;
using TuneVault.Application.Features.Album.DTOs;

namespace TuneVault.Application.Features.Album.Commands.CreateAlbum;

/// <summary>
/// Command tạo album mới cho artist hiện tại.
/// </summary>
public sealed record CreateAlbumCommand(string ArtistId, CreateAlbumRequestDto Request) : IRequest<AlbumDto>;
