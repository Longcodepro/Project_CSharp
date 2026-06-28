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
            SenderIdDisplay: i.SenderIdDisplay?.ToString(),
            SenderDisplayName: i.SenderDisplayName?.ToString(),
            SenderAvatarUrl: i.SenderAvatarUrl?.ToString(),
            ReceiverId: i.ReceiverId,
            ReceiverIdDisplay: i.ReceiverIdDisplay?.ToString(),
            ReceiverDisplayName: i.ReceiverDisplayName?.ToString(),
            ReceiverAvatarUrl: i.ReceiverAvatarUrl?.ToString(),
            ShareType: i.ShareTypeName?.ToString() ?? i.ShareType.ToString(),
            SharedItemId: i.SharedItemId,
            ItemTitle: i.ItemTitle?.ToString(),
            ItemCoverImageUrl: i.ItemCoverImageUrl?.ToString() ?? i.ItemCoverImgUrl?.ToString(),
            Message: i.Message?.ToString(),
            SharedAt: i.SharedAt,
            IsRead: i.IsRead is bool isRead && isRead
        )).ToList();
    }
}
