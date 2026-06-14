namespace TuneVault.Application.Features.Share.Queries.GetSharedWithMe;

public interface IMediaShareQueryRepository
{
    Task<IEnumerable<dynamic>> GetInboxSharesAsync(string receiverId);

    Task<int> CountUnreadSharesAsync(string receiverId);
}

public sealed class GetSharedWithMeQuery
{
    private readonly IMediaShareQueryRepository _mediaShareRepository;

    public GetSharedWithMeQuery(IMediaShareQueryRepository mediaShareRepository)
    {
        _mediaShareRepository = mediaShareRepository;
    }

    public async Task<IEnumerable<dynamic>> GetInboxAsync(string receiverId)
    {
        ValidateRequired(receiverId, nameof(receiverId));

        return await _mediaShareRepository.GetInboxSharesAsync(receiverId.Trim());
    }

    public async Task<int> CountUnreadAsync(string receiverId)
    {
        ValidateRequired(receiverId, nameof(receiverId));

        return await _mediaShareRepository.CountUnreadSharesAsync(receiverId.Trim());
    }

    private static void ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} không được để trống.", parameterName);
    }
}