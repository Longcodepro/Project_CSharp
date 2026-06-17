using System;
using TuneVault.Domain.Exceptions;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một bản ghi Lịch sử phát (PlayHistory) của người dùng trong hệ thống TuneVault.
/// Thực thể độc lập này tự quản lý vòng đời và quy tắc giới hạn tối đa 20 mục bằng cơ chế đẩy lùi vị trí.
/// </summary>
public class PlayHistory
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MinHistoryOrder = 1;
    private const int MaxHistoryOrder = 20;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của bản ghi lịch sử. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Mã định danh của người dùng (User) phát vật phẩm phương tiện. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Mã định danh của vật phẩm phương tiện (MediaItem) được phát. Độ dài cố định từ 4 đến 5 ký tự.
    /// </summary>
    public string MediaItemId { get; private set; } = string.Empty;

    /// <summary>
    /// Vị trí thứ tự sắp xếp trong danh sách lịch sử (Từ 1 đến 20). 
    /// Giá trị 1 đại diện cho bài hát mới phát gần đây nhất. Khi có bài mới, vị trí này sẽ tăng dần (bị đẩy lùi).
    /// </summary>
    public int HistoryOrder { get; private set; }

    /// <summary>
    /// Thời điểm người dùng bấm tạm dừng hoặc tắt trình phát nhạc (Tùy chọn, chỉ khả dụng ở vị trí số 1).
    /// </summary>
    public DateTime? StoppedAt { get; private set; }

    // --- Constructor ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// </summary>
    private PlayHistory() { }

    /// <summary>
    /// Khởi tạo một bản ghi lịch sử phát nhạc mới với các ràng buộc khắt khe về vị trí xếp hàng.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của PlayHistory.</param>
    /// <param name="userId">Mã định danh của người dùng nghe nhạc.</param>
    /// <param name="mediaItemId">Mã định danh của bài hát/video được phát.</param>
    /// <param name="historyOrder">Vị trí xếp thứ tự lịch sử lúc khởi tạo (Thường mặc định là 1 cho bản ghi mới).</param>
    public PlayHistory(string id, string userId, string mediaItemId, int historyOrder)
    {
        ValidateId(id);
        ValidateUserId(userId);
        ValidateMediaItemId(mediaItemId);
        ValidateHistoryOrder(historyOrder);

        Id = id.Trim();
        UserId = userId.Trim();
        MediaItemId = mediaItemId.Trim();
        HistoryOrder = historyOrder;
        StoppedAt = null; // Khởi tạo ban đầu khi đang phát thì chưa có thời gian dừng
    }

    // --- Business Methods ---

    /// <summary>
    /// Ghi nhận hành động dừng phát nhạc (Stop) của người dùng để lưu lại vết trạng thái (phục vụ tính năng nghe tiếp).
    /// Tính năng này kiểm soát nghiêm ngặt và chỉ cho phép thực thi nếu đây là bản ghi mới phát gần đây nhất (Vị trí số 1).
    /// </summary>
    /// <param name="stoppedAt">Mốc thời gian ghi nhận hành động dừng phát.</param>
    /// <exception cref="DomainException">Ném ra khi bản ghi này không phải bản ghi mới nhất hoặc mốc thời gian không hợp lệ.</exception>
    public void Stop(DateTime stoppedAt)
    {
        if (HistoryOrder != 1)
            throw new DomainException("Chức năng dừng phát (Stop) chỉ có hiệu lực với bản ghi lịch sử mới nhất (Vị trí số 1).");

        ValidateStoppedAt(stoppedAt);

        StoppedAt = stoppedAt;
    }

    /// <summary>
    /// Cập nhật đẩy lùi mã thứ tự lịch sử khi có một vật phẩm phương tiện mới được chèn vào đầu danh sách lịch sử của User.
    /// </summary>
    /// <param name="newHistoryOrder">Giá trị thứ tự mới sau khi bị đẩy lùi (Giới hạn nghiêm ngặt tối đa đến vị trí 20).</param>
    public void UpdateHistoryOrder(int newHistoryOrder)
    {
        ValidateHistoryOrder(newHistoryOrder);
        HistoryOrder = newHistoryOrder;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh lịch sử PlayHistory.
    /// </summary>
    /// <param name="id">Chuỗi định danh cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi Id trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của PlayHistory không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của PlayHistory phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh người dùng (UserId).
    /// </summary>
    /// <param name="userId">Chuỗi định danh người dùng cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi UserId trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId trong lịch sử không được để trống.");

        int length = userId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"UserId trong lịch sử phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh vật phẩm phương tiện (MediaItemId).
    /// </summary>
    /// <param name="mediaItemId">Chuỗi định danh MediaItem cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi MediaItemId trống hoặc không nằm trong khoảng độ dài từ 4 đến 5 ký tự.</exception>
    private static void ValidateMediaItemId(string mediaItemId)
    {
        if (string.IsNullOrWhiteSpace(mediaItemId))
            throw new DomainException("MediaItemId trong lịch sử không được để trống.");

        int length = mediaItemId.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"MediaItemId trong lịch sử phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực giới hạn vị trí của bản ghi lịch sử, chặn đứng dữ liệu vượt ngưỡng lưu trữ 20 mục.
    /// </summary>
    /// <param name="historyOrder">Số nguyên đại diện vị trí thứ tự.</param>
    /// <exception cref="DomainException">Ném ra khi thứ tự nằm ngoài khoảng biên từ 1 đến 20.</exception>
    private static void ValidateHistoryOrder(int historyOrder)
    {
        if (historyOrder < MinHistoryOrder || historyOrder > MaxHistoryOrder)
            throw new DomainException($"Vị trí lịch sử (HistoryOrder) phải nằm trong khoảng biên nghiêm ngặt từ {MinHistoryOrder} đến {MaxHistoryOrder}.");
    }

    /// <summary>
    /// Xác thực mốc thời gian hệ thống ghi nhận khi người dùng dừng trình phát.
    /// </summary>
    /// <param name="stoppedAt">Mốc thời gian dừng phát cần kiểm tra.</param>
    /// <exception cref="DomainException">Ném ra khi mốc thời gian mang giá trị mặc định hoặc thuộc về tương lai phi lý.</exception>
    private static void ValidateStoppedAt(DateTime stoppedAt)
    {
        if (stoppedAt == default)
            throw new DomainException("Thời gian dừng phát (StoppedAt) không được mang giá trị mặc định.");

        if (stoppedAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian dừng phát không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}
