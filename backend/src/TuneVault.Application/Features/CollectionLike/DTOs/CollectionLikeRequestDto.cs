using TuneVault.Domain.Enums;

namespace TuneVault.Application.Features.CollectionLike.DTOs;

/// <summary>
/// Request body dùng để thích hoặc bỏ thích một album/playlist.
/// </summary>
/// <param name="TargetId">Mã album hoặc playlist.</param>
/// <param name="TargetType">Loại đối tượng cần tương tác.</param>
public sealed record CollectionLikeRequestDto(string TargetId, CollectionLikeTargetType TargetType);
