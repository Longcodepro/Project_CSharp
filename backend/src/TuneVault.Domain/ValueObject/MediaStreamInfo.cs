namespace TuneVault.Domain.ValueObject;

/// <summary>
/// Value Object chứa thông tin cần thiết để stream một MediaItem.
/// </summary>
/// <param name="MediaId">ID của MediaItem.</param>
/// <param name="FilePath">Đường dẫn tuyệt đối tới file media trên hệ thống.</param>
/// <param name="ContentType">Loại nội dung (MIME type) của file media.</param>
/// <param name="SupportsRange">Cho biết file có hỗ trợ HTTP Range Request hay không (cho phép seek).</param>
public sealed record MediaStreamInfo(string MediaId, string FilePath, string ContentType, bool SupportsRange);