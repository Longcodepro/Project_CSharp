using TuneVault.Application.DTOs.Share;

namespace TuneVault.Application.Features.Share.Commands.ShareMedia;

public sealed record ShareMediaCommand(string SenderId, ShareMediaRequestDto Request);
