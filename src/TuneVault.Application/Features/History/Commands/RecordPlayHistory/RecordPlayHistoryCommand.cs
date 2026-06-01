using TuneVault.Application.DTOs.History;

namespace TuneVault.Application.Features.History.Commands.RecordPlayHistory;

public sealed record RecordPlayHistoryCommand(string UserId, RecordPlayHistoryRequestDto Request);
