using MediatR;
using TuneVault.Application.Common;

namespace TuneVault.Application.Features.History.Commands.RecordPlayHistory;

/// <summary>
/// Command để ghi nhận lịch sử nghe bài hát.
/// </summary>
/// <param name="UserId">Mã định danh người dùng.</param>
/// <param name="MediaItemId">Mã định danh bài hát.</param>
/// <param name="StoppedAt">Thời điểm dừng nghe (tùy chọn).</param>
public sealed record RecordPlayHistoryCommand(string UserId, string MediaItemId, DateTime? StoppedAt) : IRequest<ApiResponse<bool>>;
