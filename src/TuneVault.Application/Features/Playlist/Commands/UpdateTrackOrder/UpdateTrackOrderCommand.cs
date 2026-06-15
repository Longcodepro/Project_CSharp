namespace TuneVault.Application.Features.Playlist.Commands.UpdateTrackOrder;

/// <summary>
/// Command dành cho chức năng cập nhật thứ tự (TrackOrder) của một track trong Playlist.
/// </summary>
/// <param name="PlaylistId">Mã playlist chứa track.</param>
/// <param name="MediaItemId">Mã media item cần cập nhật thứ tự.</param>
/// <param name="NewTrackOrder">Thứ tự mới của track (từ 1 đến 100).</param>
public sealed record UpdateTrackOrderCommand(string PlaylistId, string MediaItemId, int NewTrackOrder);