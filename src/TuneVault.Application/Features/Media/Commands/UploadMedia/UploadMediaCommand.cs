using TuneVault.Application.DTOs.Media;

namespace TuneVault.Application.Features.Media.Commands.UploadMedia;

public sealed record UploadMediaCommand(string OwnerId, UploadMediaRequestDto Request);
