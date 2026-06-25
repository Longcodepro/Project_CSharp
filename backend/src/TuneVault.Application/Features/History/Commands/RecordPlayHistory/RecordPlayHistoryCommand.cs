using MediatR;
using TuneVault.Application.Common;

namespace TuneVault.Application.Features.History.Commands.RecordPlayHistory;

/// <summary>
/// Command để ghi nhận lịch sử nghe bài hát.
/// </summary>
/// <param name="UserId">Mã định danh người dùng.</param>
/// <param name="MediaItemId">Mã định danh bài hát.</param>
/// <param name="StoppedAt">Vị trí dừng phát theo giây trong media để phục vụ resume.</param>
public sealed record RecordPlayHistoryCommand(
    string UserId,
    string MediaItemId,
    int? StoppedAt) : IRequest<ApiResponse<bool>>;
