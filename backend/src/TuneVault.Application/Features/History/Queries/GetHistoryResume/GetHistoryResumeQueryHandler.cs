using MediatR;
using TuneVault.Application.Features.History.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.History.Queries.GetHistoryResume;

/// <summary>
/// Handler lấy thông tin phát tiếp của một media từ PlayHistory.
/// </summary>
public sealed class GetHistoryResumeQueryHandler : IRequestHandler<GetHistoryResumeQuery, HistoryResumeDto?>
{
    private readonly IPlayHistoryRepository _playHistoryRepository;
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo handler với repository history và media.
    /// </summary>
    /// <param name="playHistoryRepository">Repository đọc dữ liệu lịch sử phát.</param>
    /// <param name="mediaRepository">Repository đọc thông tin media để lấy tên bài.</param>
    public GetHistoryResumeQueryHandler(
        IPlayHistoryRepository playHistoryRepository,
        IMediaRepository mediaRepository)
    {
        _playHistoryRepository = playHistoryRepository;
        _mediaRepository = mediaRepository;
    }

    /// <summary>
    /// Trả thông tin resume nếu media đã nằm trong lịch sử người dùng.
    /// </summary>
    /// <param name="request">Query chứa user id và media id.</param>
    /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
    /// <returns>Thông tin resume hoặc null nếu chưa có lịch sử/media không còn tồn tại.</returns>
    public async Task<HistoryResumeDto?> Handle(GetHistoryResumeQuery request, CancellationToken cancellationToken)
    {
        var history = await _playHistoryRepository.GetByUserIdAndMediaItemIdAsync(
            request.UserId,
            request.MediaId,
            cancellationToken);

        if (history is null)
            return null;

        var media = await _mediaRepository.GetByIdAsync(request.MediaId, cancellationToken);
        if (media is null)
            return null;

        return new HistoryResumeDto(media.Id, media.Title, history.StoppedAt);
    }
}
