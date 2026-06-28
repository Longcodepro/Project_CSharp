using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Playlist.Commands.AddTrackToPlaylist;

/// <summary>
/// Thêm media vào playlist của người dùng.
/// </summary>
public sealed class AddTrackToPlaylistCommandHandler : IRequestHandler<AddTrackToPlaylistCommand, Unit>
{
    private readonly IPlaylistRepository _playlistRepository;

    /// <summary>
    /// Khởi tạo handler thêm track vào playlist.
    /// </summary>
    /// <param name="playlistRepository">Repository xử lý truy cập database cho Playlist.</param>
    public AddTrackToPlaylistCommandHandler(IPlaylistRepository playlistRepository)
    {
        _playlistRepository = playlistRepository;
    }

    /// <summary>
    /// Thêm track mới lên đầu playlist.
    /// </summary>
    /// <param name="command">Command chứa PlaylistId và MediaItemId.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <exception cref="DomainException">Ném ra khi Playlist hoặc media không tồn tại.</exception>
    /// <exception cref="ForbiddenAccessException">Ném ra khi user không phải chủ playlist.</exception>
    public async Task<Unit> Handle(AddTrackToPlaylistCommand command, CancellationToken cancellationToken)
    {
        var playlist = await _playlistRepository.GetByIdAsync(command.PlaylistId, cancellationToken)
            ?? throw new DomainException("Không tìm thấy playlist.");

        if (playlist.UserId != command.UserId)
            throw new ForbiddenAccessException("Bạn không có quyền thêm bài hát vào playlist này.");

        var mediaExists = await _playlistRepository.MediaItemExistsAsync(command.Request.MediaItemId, cancellationToken);
        if (!mediaExists)
            throw new DomainException("Không tìm thấy bài hát.");

        var trackExists = await _playlistRepository.TrackExistsAsync(command.PlaylistId, command.Request.MediaItemId, cancellationToken);
        if (trackExists)
            throw new DomainException("Bài hát đã tồn tại trong playlist.");

        var currentTracks = (await _playlistRepository.GetPlaylistTracksAsync(command.PlaylistId, cancellationToken)).ToList();
        if (currentTracks.Count > 0)
        {
            await _playlistRepository.ShiftTrackOrdersAsync(command.PlaylistId, 1, 1, cancellationToken);
        }

        const int trackOrder = 1;

        var trackId = await GenerateNextTrackIdAsync(cancellationToken);

        var track = new PlaylistTrack(
            trackId,
            command.PlaylistId,
            command.Request.MediaItemId,
            trackOrder
        );

        playlist.AddTrack(track);

        await _playlistRepository.AddTrackAsync(track, cancellationToken);
        return Unit.Value;
    }

    /// <summary>
    /// Sinh ID tiếp theo theo format PT001, PT002...
    /// Lấy ID lớn nhất trong DB, tách phần chữ và phần số, tăng số lên 1.
    /// Ví dụ: PT006 → tách ra PT + 006 → tăng lên 007 → ghép lại PT007
    /// </summary>
    private async Task<string> GenerateNextTrackIdAsync(CancellationToken cancellationToken)
    {
        const string prefix = "PT";

        var allTracks = await _playlistRepository.GetAllTracksAsync(cancellationToken);

        var maxNumber = allTracks
            .Select(t => t.Id)
            .Where(id => id.StartsWith(prefix) && id.Length > prefix.Length)
            .Select(id =>
            {
                var numberPart = id.Substring(prefix.Length);
                return int.TryParse(numberPart, out var num) ? num : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        var nextNumber = maxNumber + 1;
        return $"{prefix}{nextNumber:D3}";
    }
}
