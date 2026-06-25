using MediatR;
using TuneVault.Application.Common;
using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.CollectionLike.Commands.ToggleCollectionLike;

/// <summary>
/// Command bật/tắt lượt thích cho album hoặc playlist.
/// </summary>
/// <param name="TargetId">Mã album hoặc playlist.</param>
/// <param name="TargetType">Loại đối tượng cần tương tác.</param>
public sealed record ToggleCollectionLikeCommand(
    string TargetId,
    CollectionLikeTargetType TargetType) : IRequest<ApiResponse<bool>>;
