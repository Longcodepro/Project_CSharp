namespace TuneVault.Domain.Interfaces;

/// <summary>
/// Định nghĩa các thao tác truy cập dữ liệu cho chức năng chia sẻ media giữa người dùng trong TuneVault.
/// Interface này dùng cho các query share ở Application layer.
/// </summary>
public interface IMediaShareRepository
{
    Task<IEnumerable<dynamic>> GetSharedByMeAsync(string senderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<dynamic>> GetSharedWithMeAsync(string receiverId, CancellationToken cancellationToken = default);
}
