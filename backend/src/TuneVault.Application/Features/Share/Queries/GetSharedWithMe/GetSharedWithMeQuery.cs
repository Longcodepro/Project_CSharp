using MediatR;
using TuneVault.Application.Features.Share.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Share.Queries.GetSharedWithMe;

public sealed record GetSharedWithMeQuery(string ReceiverId) : IRequest<List<SharedItemDto>>;

public sealed class GetSharedWithMeQueryHandler : IRequestHandler<GetSharedWithMeQuery, List<SharedItemDto>>
{
    private readonly IMediaShareRepository _repo;

    public GetSharedWithMeQueryHandler(IMediaShareRepository repo) => _repo = repo;

    public async Task<List<SharedItemDto>> Handle(GetSharedWithMeQuery request, CancellationToken ct)
    {
        var items = await _repo.GetSharedWithMeAsync(request.ReceiverId, ct);
        return items.Select(i => new SharedItemDto(
            Id: i.Id.ToString(),
            SenderId: i.SenderId,
            ReceiverId: i.ReceiverId,
            ShareType: i.ShareTypeName?.ToString() ?? i.ShareType.ToString(),
            SharedItemId: i.SharedItemId,
            SharedAt: i.SharedAt,
            IsRead: i.IsRead is bool isRead && isRead
        )).ToList();
    }
}
