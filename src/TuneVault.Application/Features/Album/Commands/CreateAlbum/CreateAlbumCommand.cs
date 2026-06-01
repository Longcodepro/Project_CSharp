namespace TuneVault.Application.Features.Album.Commands.CreateAlbum;

public sealed record CreateAlbumCommand(string OwnerId, string Title, string? CoverImgUrl, DateTime ReleaseDate, bool IsPublic);
