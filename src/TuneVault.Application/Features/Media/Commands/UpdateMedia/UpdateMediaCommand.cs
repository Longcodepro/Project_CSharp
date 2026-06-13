using MediatR;
using TuneVault.Application.DTOs.Media;

namespace TuneVault.Application.Features.Media.Commands.UpdateMedia;

/// <summary>
/// Command cập nhật thông tin metadata của một bài hát.
/// Chỉ OwnerId (ca sĩ chính) mới có quyền cập nhật.
/// </summary>
/// <param name="MediaId">Mã định danh bài hát cần cập nhật.</param>
/// <param name="RequesterId">Mã định danh người dùng thực hiện — dùng để kiểm tra quyền.</param>
/// <param name="Request">DTO chứa thông tin metadata mới.</param>
public sealed record UpdateMediaCommand(
    string MediaId,
    string RequesterId,
    UpdateMediaRequestDto Request
) : IRequest<MediaItemDto>;
