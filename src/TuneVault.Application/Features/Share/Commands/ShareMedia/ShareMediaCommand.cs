using TuneVault.Application.Features.Share.DTOs;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

public sealed record ShareMediaCommand(string SenderId, ShareMediaRequestDto Request);
