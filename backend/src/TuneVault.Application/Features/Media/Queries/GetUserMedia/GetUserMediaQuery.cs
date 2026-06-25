using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Queries.GetUserMedia;

/// <summary>
/// Query lấy danh sách media chi tiết của chính owner đang đăng nhập.
/// </summary>
public sealed record GetUserMediaQuery(string UserId) : IRequest<IReadOnlyCollection<MediaOwnerDetailDto>>;

/// <summary>
/// Handler chỉ cho phép owner xem danh sách media chi tiết của chính mình.
/// </summary>
public sealed class GetUserMediaQueryHandler : IRequestHandler<GetUserMediaQuery, IReadOnlyCollection<MediaOwnerDetailDto>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ICurrentUserContext _currentUserContext;

    public GetUserMediaQueryHandler(IMediaRepository mediaRepository, ICurrentUserContext currentUserContext)
    {
        _mediaRepository = mediaRepository;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyCollection<MediaOwnerDetailDto>> Handle(GetUserMediaQuery request, CancellationToken ct)
    {
        var currentUserId = _currentUserContext.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new UnauthorizedAccessException("Bạn cần đăng nhập để xem media của chính mình.");

        if (!currentUserId.Equals(request.UserId, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenAccessException("Bạn không có quyền xem danh sách media chi tiết của người dùng khác.");

        var items = await _mediaRepository.GetByOwnerAsync(request.UserId, ct);
        var result = new List<MediaOwnerDetailDto>(items.Count);

        foreach (var item in items)
        {
            var artists = await _mediaRepository.GetArtistsByMediaIdAsync(item.Id, ct);
            result.Add(MediaDtoMapper.ToOwnerDetailDto(item, artists));
        }

        return result;
    }
}
