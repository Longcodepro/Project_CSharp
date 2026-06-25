using MediatR;

namespace TuneVault.Application.Features.Playlist.Commands.UpdateTrackOrder;

/// <summary>
/// Command dành cho chức năng cập nhật thứ tự (TrackOrder) của một track trong Playlist.
/// </summary>
/// <param name="PlaylistId">Mã playlist chứa track.</param>
/// <param name="UserId">Mã người dùng đang thực hiện thao tác.</param>
/// <param name="MediaItemId">Mã media item cần cập nhật thứ tự.</param>
/// <param name="NewTrackOrder">Thứ tự mới của track (từ 1 đến 100).</param>
public sealed record UpdateTrackOrderCommand(
    string PlaylistId,
    string UserId,
    string MediaItemId,
    int NewTrackOrder) : IRequest<Unit>;
