using MediatR;
using TuneVault.Application.Features.Media.DTOs;

namespace TuneVault.Application.Features.Media.Commands.UploadMedia;

/// <summary>
/// Command upload một bài hát mới lên hệ thống TuneVault.
/// </summary>
/// <param name="MediaId">Mã định danh bài hát do system tự sinh (VD: I001).</param>
/// <param name="Request">DTO chứa toàn bộ thông tin bài hát và danh sách ca sĩ phụ.</param>
public sealed record UploadMediaCommand(
    string MediaId,
    UploadMediaRequestDto Request
) : IRequest<MediaItemDto>;
