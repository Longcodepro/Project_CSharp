using MediatR;
using TuneVault.Application.Features.Share.DTOs; // Assuming SharedItemDto is here

namespace TuneVault.Application.Features.Share.Queries.GetSharedByMe;

public record GetSharedByMeQuery(string SenderId) : IRequest<List<SharedItemDto>>;