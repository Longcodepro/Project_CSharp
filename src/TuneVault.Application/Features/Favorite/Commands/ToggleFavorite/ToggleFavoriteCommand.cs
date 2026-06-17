using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;

/// <summary>
/// Command để thêm, cập nhật hoặc xóa trạng thái yêu thích của một bài hát.
/// </summary>
/// <param name="MediaItemId">Mã định danh của bài hát.</param>
/// <param name="Reaction">Loại phản ứng muốn lưu hoặc Remove để xóa phản ứng hiện tại.</param>
public sealed record ToggleFavoriteCommand(string MediaItemId, FavoriteReaction Reaction) : IRequest<ApiResponse<bool>>;
