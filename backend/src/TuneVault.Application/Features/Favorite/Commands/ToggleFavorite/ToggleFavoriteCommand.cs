using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;

/// <summary>
/// Command để thêm, cập nhật hoặc xóa trạng thái cảm xúc của media, album hoặc playlist.
/// </summary>
/// <param name="TargetId">Mã định danh của media, album hoặc playlist.</param>
/// <param name="TargetType">Loại đối tượng được tương tác.</param>
/// <param name="Reaction">Loại phản ứng muốn lưu hoặc Remove để xóa phản ứng hiện tại.</param>
public sealed record ToggleFavoriteCommand(
    string TargetId,
    FavoriteTargetType TargetType,
    FavoriteReaction Reaction) : IRequest<ApiResponse<FavoriteReaction?>>;
