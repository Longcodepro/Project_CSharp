using MediatR;
using TuneVault.Application.Features.Media.DTOs;

namespace TuneVault.Application.Features.History.Queries.GetRecentHistory;

public record GetRecentHistoryQuery(string UserId) : IRequest<List<MediaItemDto>>;
