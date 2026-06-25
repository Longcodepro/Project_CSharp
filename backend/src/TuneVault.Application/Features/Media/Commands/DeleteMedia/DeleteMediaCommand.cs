using MediatR;

namespace TuneVault.Application.Features.Media.Commands.DeleteMedia;

/// <summary>
/// Command thực hiện Soft Delete một bài hát (chuyển IsActive = false).
/// Chỉ Owner (ca sĩ chính) mới có quyền xóa bài hát của mình.
/// </summary>
/// <param name="MediaId">Mã định danh bài hát cần xóa.</param>
/// <param name="RequesterId">Mã định danh người dùng thực hiện yêu cầu xóa — dùng để kiểm tra quyền.</param>
public sealed record DeleteMediaCommand(
    string MediaId,
    string RequesterId
) : IRequest<bool>;
