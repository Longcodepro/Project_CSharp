using TuneVault.Domain.Entities;
using TuneVault.Domain.Interfaces;
using TuneVault.Infrastructure.DAO;

namespace TuneVault.Infrastructure.Repositories;

/// <summary>
/// Repository triển khai các thao tác lưu trữ và truy vấn dữ liệu chia sẻ media giữa người dùng.
/// Lớp này chuyển đổi dữ liệu giữa ShareDAO và entity MediaShare trong tầng Domain.
/// </summary>
public sealed class MediaShareRepository : IMediaShareRepository
{
    private readonly ShareDAO _shareDao;

    /// <summary>
    /// Khởi tạo một instance mới của MediaShareRepository với DAO xử lý dữ liệu chia sẻ.
    /// </summary>
    public MediaShareRepository(ShareDAO shareDao)
    {
        _shareDao = shareDao;
    }

    /// <summary>
    /// Lưu một bản ghi chia sẻ media mới từ người gửi đến người nhận.
    /// </summary>
    public async Task ShareAsync(MediaShare mediaShare, CancellationToken cancellationToken = default)
    {
        await _shareDao.CreateMediaShareAsync(
            mediaShare.SenderId,
            mediaShare.ReceiverId,
            ToDatabaseShareType(mediaShare.ShareType),
            mediaShare.SharedItemId);
    }

    /// <summary>
    /// Lấy danh sách các nội dung mà người dùng đã chia sẻ cho người khác.
    /// </summary>
    public async Task<IReadOnlyCollection<MediaShare>> GetSharedByMeAsync(Guid senderId, CancellationToken cancellationToken = default)
    {
        var rows = await _shareDao.GetSentSharesAsync(RepositoryMappingHelper.ToDatabaseId(senderId));
        return rows.Select(MapMediaShare).ToList();
    }

    /// <summary>
    /// Lấy danh sách các nội dung mà người dùng đã nhận được từ người khác.
    /// </summary>
    public async Task<IReadOnlyCollection<MediaShare>> GetSharedWithMeAsync(Guid receiverId, CancellationToken cancellationToken = default)
    {
        var rows = await _shareDao.GetInboxSharesAsync(RepositoryMappingHelper.ToDatabaseId(receiverId));
        return rows.Select(MapMediaShare).ToList();
    }

    /// <summary>
    /// Đánh dấu một bản ghi chia sẻ là đã đọc đối với người nhận.
    /// </summary>
    public async Task MarkAsReadAsync(Guid shareId, Guid receiverId, CancellationToken cancellationToken = default)
    {
        await _shareDao.MarkShareAsReadAsync(
            RepositoryMappingHelper.ToDatabaseId(shareId),
            RepositoryMappingHelper.ToDatabaseId(receiverId));
    }

    /// <summary>
    /// Đánh dấu toàn bộ bản ghi chia sẻ của một người nhận là đã đọc.
    /// </summary>
    public async Task MarkAllAsReadAsync(Guid receiverId, CancellationToken cancellationToken = default)
    {
        await _shareDao.MarkAllSharesAsReadAsync(RepositoryMappingHelper.ToDatabaseId(receiverId));
    }

    /// <summary>
    /// Đếm số lượng bản ghi chia sẻ chưa đọc của một người nhận.
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid receiverId, CancellationToken cancellationToken = default)
    {
        return await _shareDao.CountUnreadSharesAsync(RepositoryMappingHelper.ToDatabaseId(receiverId));
    }

    /// <summary>
    /// Ánh xạ một dòng dữ liệu chia sẻ từ DAO thành entity MediaShare của tầng Domain.
    /// </summary>
    private static MediaShare MapMediaShare(object row)
    {
        return RepositoryMappingHelper.CreateEntity<MediaShare>(
            (nameof(MediaShare.Id), RepositoryMappingHelper.ReadString(row, "Id")),
            (nameof(MediaShare.SenderId), RepositoryMappingHelper.ReadString(row, "SenderId")),
            (nameof(MediaShare.ReceiverId), RepositoryMappingHelper.ReadString(row, "ReceiverId")),
            (nameof(MediaShare.SharedItemId), RepositoryMappingHelper.ReadString(row, "SharedItemId")),
            (nameof(MediaShare.ShareType), RepositoryMappingHelper.ReadEnum(row, "ShareType", ShareType.MediaItem)),
            (nameof(MediaShare.Message), null),
            (nameof(MediaShare.SharedAt), RepositoryMappingHelper.ReadDateTime(row, "SharedAt")));
    }

    /// <summary>
    /// Chuyển đổi ShareType của tầng Domain sang chuỗi giá trị tương ứng được lưu trong cơ sở dữ liệu.
    /// </summary>
    private static string ToDatabaseShareType(ShareType shareType)
    {
        return shareType switch
        {
            ShareType.MediaItem => "Track",
            ShareType.Album => "Album",
            ShareType.Playlist => "Playlist",
            _ => shareType.ToString()
        };
    }
}
