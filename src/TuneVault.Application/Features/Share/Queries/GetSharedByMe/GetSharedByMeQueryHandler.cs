using MediatR;
using TuneVault.Application.Features.Share.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Share.Queries.GetSharedByMe;

public sealed class GetSharedByMeQueryHandler : IRequestHandler<GetSharedByMeQuery, List<SharedItemDto>>
{
    private readonly IMediaShareRepository _repo;

    public GetSharedByMeQueryHandler(IMediaShareRepository repo) => _repo = repo;

    public async Task<List<SharedItemDto>> Handle(GetSharedByMeQuery request, CancellationToken ct)
    {
        // Gọi repository lấy danh sách shared bởi senderId
        var items = await _repo.GetSharedByMeAsync(request.SenderId, ct);
        return items.Select(i => new SharedItemDto(
            Id: i.Id.ToString(),
            SenderId: i.SenderId,
            ReceiverId: i.ReceiverId,
            ShareType: i.ShareTypeName?.ToString() ?? i.ShareType.ToString(),
            SharedItemId: i.SharedItemId,
            SharedAt: i.SharedAt,
            IsRead: i.IsRead
        )).ToList();
    }
}
