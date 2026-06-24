using MediatR;
using TuneVault.Application.Features.Media.DTOs;
using TuneVault.Domain.Interfaces;

namespace TuneVault.Application.Features.Media.Queries.GetArtistMedia;

/// <summary>
/// Query lấy danh sách media công khai của một nghệ sĩ để hiển thị cho người xem bên ngoài.
/// </summary>
/// <param name="ArtistId">Mã người dùng nghệ sĩ cần lấy danh sách media.</param>
public sealed record GetArtistMediaQuery(string ArtistId) : IRequest<IReadOnlyCollection<MediaPublicDto>>;

/// <summary>
/// Handler lấy danh sách media công khai, đang hoạt động của một nghệ sĩ.
/// </summary>
public sealed class GetArtistMediaQueryHandler : IRequestHandler<GetArtistMediaQuery, IReadOnlyCollection<MediaPublicDto>>
{
    private readonly IMediaRepository _mediaRepository;

    /// <summary>
    /// Khởi tạo handler với repository media.
    /// </summary>
    public GetArtistMediaQueryHandler(IMediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    /// <summary>
    /// Truy vấn danh sách media public của nghệ sĩ và map sang DTO công khai.
    /// </summary>
    public async Task<IReadOnlyCollection<MediaPublicDto>> Handle(GetArtistMediaQuery request, CancellationToken ct)
    {
        var items = await _mediaRepository.GetPublicByOwnerAsync(request.ArtistId, ct);
        var result = new List<MediaPublicDto>(items.Count);

        foreach (var item in items)
        {
            var artists = await _mediaRepository.GetArtistsByMediaIdAsync(item.Id, ct);
            result.Add(MediaDtoMapper.ToPublicDto(item, artists));
        }

        return result;
    }
}
