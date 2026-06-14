using MediatR;
using TuneVault.Application.Features.Media.DTOs;

namespace TuneVault.Application.Features.Media.Queries.GetMediaById;

/// <summary>
/// Query lấy thông tin metadata của một bài hát theo Id nội bộ.
/// Không bao gồm file stream — chỉ trả về thông tin hiển thị.
/// </summary>
/// <param name="MediaId">Mã định danh nội bộ của bài hát (VD: I001).</param>
public sealed record GetMediaByIdQuery(string MediaId) : IRequest<MediaItemDto?>;
