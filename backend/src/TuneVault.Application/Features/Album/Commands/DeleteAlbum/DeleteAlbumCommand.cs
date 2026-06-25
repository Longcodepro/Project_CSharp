using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Album.Commands.DeleteAlbum;

/// <summary>
/// Command xóa mềm album của artist hiện tại.
/// </summary>
public sealed record DeleteAlbumCommand(string AlbumId, string CurrentUserId) : IRequest;

/// <summary>
/// Handler xóa mềm album.
/// </summary>
public sealed class DeleteAlbumCommandHandler : IRequestHandler<DeleteAlbumCommand>
{
    private readonly IAlbumRepository _albumRepository;

    /// <summary>
    /// Khởi tạo handler xóa album.
    /// </summary>
    public DeleteAlbumCommandHandler(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
    }

    /// <summary>
    /// Xóa mềm album nếu người thao tác là owner.
    /// </summary>
    public async Task Handle(DeleteAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(request.AlbumId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy album.");

        if (!string.Equals(album.ArtistId, request.CurrentUserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền xóa album này.");

        album.Deactivate();
        await _albumRepository.DeleteAsync(request.AlbumId, cancellationToken);
    }
}
