using MediatR;

namespace TuneVault.Application.Features.Media.Commands.GenerateMediaId;

public sealed record GenerateMediaIdCommand : IRequest<string>;