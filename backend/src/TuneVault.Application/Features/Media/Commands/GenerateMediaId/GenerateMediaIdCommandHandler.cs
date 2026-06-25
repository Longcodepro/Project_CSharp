using MediatR;
using TuneVault.Application.Interfaces; // Assuming ISequentialIdGenerator is here or similar
using TuneVault.Domain.Interfaces; // Assuming IMediaRepository is here

namespace TuneVault.Application.Features.Media.Commands.GenerateMediaId;

public sealed class GenerateMediaIdCommandHandler : IRequestHandler<GenerateMediaIdCommand, string>
{
    private readonly IMediaRepository _mediaRepository;
    // Assuming a service or interface exists for sequential ID generation, similar to User/Follow
    // If not, we'll need to implement it in MediaRepository.
    // For now, let's assume MediaRepository has a method to generate the next ID.

    public GenerateMediaIdCommandHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<string> Handle(GenerateMediaIdCommand request, CancellationToken cancellationToken)
    {
        // This method will call a new method in MediaRepository to get the next sequential ID.
        // Example: "I0001", "I0002", etc.
        return await _mediaRepository.GenerateNextMediaIdAsync(cancellationToken);
    }
}