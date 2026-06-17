using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Application.Features.History.Commands.RecordPlayHistory;

/// <summary>
/// Xử lý command để ghi nhận lịch sử nghe bài hát.
/// </summary>
public sealed class RecordPlayHistoryCommandHandler : IRequestHandler<RecordPlayHistoryCommand, ApiResponse<bool>>
{
    private readonly IPlayHistoryRepository _playHistoryRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public RecordPlayHistoryCommandHandler(IPlayHistoryRepository playHistoryRepository, ICurrentUserContext currentUserContext)
    {
        _playHistoryRepository = playHistoryRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<ApiResponse<bool>> Handle(RecordPlayHistoryCommand request, CancellationToken ct)
    {
        var userId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        var mediaExists = await _playHistoryRepository.MediaItemExistsAsync(request.MediaItemId, ct);
        if (!mediaExists)
        {
            throw new DomainException("Không tìm thấy bài hát.");
        }

        var existingHistory = await _playHistoryRepository.GetByUserIdAndMediaItemIdAsync(userId, request.MediaItemId, ct);
        var stoppedAt = request.StoppedAt ?? DateTime.UtcNow;

        if (existingHistory == null)
        {
            var newHistory = new PlayHistory("PH00", userId, request.MediaItemId, 1);
            newHistory.Stop(stoppedAt);
            await _playHistoryRepository.AddAsync(newHistory, ct);
            return ApiResponse<bool>.Ok(true, "Đã ghi nhận lịch sử nghe mới.");
        }
        else
        {
            existingHistory.UpdateHistoryOrder(1);
            existingHistory.Stop(stoppedAt);
            await _playHistoryRepository.UpdateAsync(existingHistory, ct);
            return ApiResponse<bool>.Ok(true, "Đã cập nhật lịch sử nghe.");
        }
    }
}
