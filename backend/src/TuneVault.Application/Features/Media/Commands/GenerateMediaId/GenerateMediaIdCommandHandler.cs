using MediatR;
using TuneVault.Application.Interfaces;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Commands.GenerateMediaId;

/// <summary>
/// Sinh mã media mới theo định dạng tuần tự.
/// </summary>
public sealed class GenerateMediaIdCommandHandler : IRequestHandler<GenerateMediaIdCommand, string>
{
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo handler sinh mã media.
    /// </summary>
    public GenerateMediaIdCommandHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    /// <summary>
    /// Lấy mã media tiếp theo từ repository.
    /// </summary>
    public async Task<string> Handle(GenerateMediaIdCommand request, CancellationToken cancellationToken)
    {
        return await _mediaRepository.GenerateNextMediaIdAsync(cancellationToken);
    }
}
