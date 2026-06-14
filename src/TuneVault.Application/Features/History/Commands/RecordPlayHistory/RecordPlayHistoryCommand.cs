using TuneVault.Application.Features.History.DTOs;

namespace TuneVault.Application.Features.History.Commands.RecordPlayHistory;

public sealed record RecordPlayHistoryCommand(string UserId, RecordPlayHistoryRequestDto Request);
