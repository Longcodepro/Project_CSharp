using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.Favorite.Commands.ToggleFavorite;

/// <summary>
/// Command để thêm hoặc xóa trạng thái yêu thích của media, album hoặc playlist.
/// </summary>
/// <param name="TargetId">Mã định danh của media, album hoặc playlist.</param>
/// <param name="TargetType">Loại đối tượng được tương tác.</param>
public sealed record ToggleFavoriteCommand(
    string TargetId,
    FavoriteTargetType TargetType,
    bool IsActive) : IRequest<ApiResponse<bool>>;
