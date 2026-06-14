using MediatR;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Commands.DeleteMedia;

/// <summary>
/// Handler xử lý <see cref="DeleteMediaCommand"/>.
/// Luồng: lấy Entity → kiểm tra tồn tại → kiểm tra quyền sở hữu
///         → gọi Deactivate() trên Entity → persist → trả về kết quả.
/// </summary>
public sealed class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand, bool>
{
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo Handler với <see cref="IMediaRepository"/> được inject qua DI.
    /// </summary>
    /// <param name="mediaRepository">Repository thao tác dữ liệu MediaItem.</param>
    public DeleteMediaCommandHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    /// <summary>
    /// Xử lý luồng xóa mềm bài hát theo thứ tự:
    /// lấy Entity → validate tồn tại → validate quyền → gọi Deactivate → persist.
    /// </summary>
    /// <param name="request">Command chứa MediaId và RequesterId.</param>
    /// <param name="ct">Token hủy tác vụ bất đồng bộ.</param>
    /// <returns><c>true</c> nếu xóa thành công.</returns>
    /// <exception cref="DomainException">
    /// Ném ra nếu: bài hát không tồn tại, đã bị xóa, hoặc người dùng không phải Owner.
    /// </exception>
    public async Task<bool> Handle(DeleteMediaCommand request, CancellationToken ct)
    {
        // Step 1: Lấy MediaItem Entity từ database
        var mediaItem = await _mediaRepository.GetByIdAsync(request.MediaId, ct)
            ?? throw new DomainException($"Bài hát với Id '{request.MediaId}' không tồn tại.");

        // Step 2: Kiểm tra quyền sở hữu — chỉ OwnerId (ca sĩ chính) mới được xóa
        if (mediaItem.OwnerId != request.RequesterId)
            throw new ForbiddenAccessException(
                "Bạn không có quyền xóa bài hát này. Chỉ ca sĩ chính (Owner) mới có quyền xóa bài hát.");

        // Step 3: Gọi method nghiệp vụ Deactivate() trên Entity
        // Entity tự kiểm tra IsActive trước khi cho phép xóa
        mediaItem.Deactivate();

        // Step 4: Persist trạng thái Entity (IsActive = false) vào database
        await _mediaRepository.UpdateAsync(mediaItem, ct);

        // Step 5: Trả về true xác nhận thao tác thành công
        return true;
    }
}