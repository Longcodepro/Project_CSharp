using System;
using TuneVault.Domain.Enums;
using TuneVault.Domain.Exceptions;
using TuneVault.Domain.ValueObjects;

namespace TuneVault.Domain.Entities;

/// <summary>
/// Đại diện cho một chiến dịch/nội dung quảng cáo (Ad) độc lập trong hệ thống TuneVault.
/// Thực thể này sử dụng mã định danh dạng chuỗi viết tắt và chịu trách nhiệm phân phối quảng cáo ngắt quãng cho nhóm user phổ thông.
/// </summary>
public class Ad
{
    // --- Constants ---
    private const int MinIdLength = 4;
    private const int MaxIdLength = 5;
    private const int MaxTitleLength = 150;
    private const int MaxAdvertiserLength = 100;

    // --- Properties ---

    /// <summary>
    /// Mã định danh duy nhất (Primary Key) của quảng cáo. Độ dài cố định từ 4 đến 5 ký tự để đồng bộ hệ thống.
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// Tiêu đề hoặc tên hiển thị của chiến dịch quảng cáo. Giới hạn tối đa 150 ký tự.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Tên đơn vị hoặc đối tác đại diện đặt quảng cáo (Nhà quảng cáo). Giới hạn tối đa 100 ký tự.
    /// </summary>
    public string Advertiser { get; private set; } = string.Empty;

    /// <summary>
    /// Phân loại hình thức hiển thị quảng cáo (Sử dụng Enum AdType để điều hướng phát nhạc ngắt quãng).
    /// </summary>
    public AdType Type { get; private set; }

    /// <summary>
    /// Đối tượng giá trị chứa thông tin URL tài nguyên và thời lượng phát của quảng cáo.
    /// </summary>
    public AdMedia Media { get; private set; } = null!;

    /// <summary>
    /// Trạng thái hoạt động của quảng cáo. True biểu thị quảng cáo khả dụng để phân phối, False biểu thị tạm dừng.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Mốc thời gian hệ thống khởi tạo bản ghi quảng cáo này lên hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    // --- Constructors ---

    /// <summary>
    /// Constructor rỗng cấu hình quyền truy cập private phục vụ cơ chế mapping tự động của Dapper/ORM.
    /// Khi map từ cơ sở dữ liệu lên, Dapper sẽ bỏ qua các hàm xác thực nghiệp vụ để tái tạo trạng thái lịch sử.
    /// </summary>
    private Ad() { }

    /// <summary>
    /// Khởi tạo một thực thể quảng cáo mới với mã định danh chuỗi chuẩn hóa và trạng thái kích hoạt mặc định là hoạt động.
    /// </summary>
    /// <param name="id">Mã định danh duy nhất của bản ghi quảng cáo (Độ dài từ 4 đến 5 ký tự).</param>
    /// <param name="title">Tiêu đề nội dung quảng cáo.</param>
    /// <param name="advertiser">Tên thương hiệu/nhà quảng cáo.</param>
    /// <param name="type">Loại hình hiển thị hoặc phát quảng cáo.</param>
    /// <param name="media">Value Object chứa cấu trúc URL và thời lượng quảng cáo.</param>
    /// <exception cref="DomainException">Ném ra khi dữ liệu đầu vào vi phạm các ràng buộc nghiệp vụ hoặc định dạng trống.</exception>
    public Ad(string id, string title, string advertiser, AdType type, AdMedia media)
    {
        ValidateId(id);
        ValidateTitle(title);
        ValidateAdvertiser(advertiser);
        ValidateType(type);
        ValidateMedia(media);

        DateTime now = DateTime.UtcNow;
        ValidateCreatedAt(now);

        Id = id.Trim();
        Title = title.Trim();
        Advertiser = advertiser.Trim();
        Type = type;
        Media = media;
        IsActive = true; // Mặc định chiến dịch mới tạo sẽ ở trạng thái sẵn sàng chạy
        CreatedAt = now;
    }

    // --- Business Methods (Update) ---

    /// <summary>
    /// Cập nhật tiêu đề mới cho chiến dịch quảng cáo.
    /// </summary>
    /// <param name="newTitle">Nội dung tiêu đề mới cần thay đổi.</param>
    /// <exception cref="DomainException">Ném ra nếu tiêu đề mới trống hoặc vượt quá độ dài quy định.</exception>
    public void UpdateTitle(string newTitle)
    {
        ValidateTitle(newTitle);
        Title = newTitle.Trim();
    }

    /// <summary>
    /// Cập nhật riêng đường dẫn tài nguyên URL của quảng cáo mà vẫn giữ nguyên thời lượng (Duration) hiện tại.
    /// </summary>
    /// <param name="newUrl">Đường dẫn liên kết mới của file quảng cáo.</param>
    /// <exception cref="DomainException">Ném ra nếu đường dẫn URL sai định dạng hoặc trống công cộng.</exception>
    public void UpdateUrl(string newUrl)
    {
        // Khởi tạo một đối tượng AdMedia mới để tự kích hoạt logic Validate định dạng URL bên trong nó
        var updatedMedia = new AdMedia(newUrl, Media.DurationInSeconds);
        ValidateMedia(updatedMedia);
        Media = updatedMedia;
    }

    /// <summary>
    /// Cập nhật toàn bộ cấu trúc tài nguyên quảng cáo bao gồm cả đường dẫn URL lẫn thời lượng phát sóng mới.
    /// </summary>
    /// <param name="newMedia">Value Object chứa thông tin truyền thông mới.</param>
    /// <exception cref="DomainException">Ném ra nếu đối tượng truyền vào mang giá trị null.</exception>
    public void UpdateMedia(AdMedia newMedia)
    {
        ValidateMedia(newMedia);
        Media = newMedia;
    }

    /// <summary>
    /// Cập nhật trạng thái hoạt động (Bật/Tắt) của quảng cáo phục vụ luồng phân phối ngắt quãng cho tài khoản Free.
    /// </summary>
    /// <param name="newStatus">Trạng thái mong muốn gán cho quảng cáo (True: Hoạt động, False: Tạm dừng).</param>
    /// <exception cref="DomainException">Ném ra nếu trạng thái mới trùng lặp với trạng thái hiện tại của thực thể.</exception>
    public void UpdateActiveStatus(bool newStatus)
    {
        ValidateIsActive(IsActive, newStatus);
        IsActive = newStatus;
    }

    // --- Validation Methods (Single Responsibility) ---

    /// <summary>
    /// Xác thực tính hợp lệ về định dạng và độ dài của mã định danh bản ghi quảng cáo.
    /// </summary>
    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Id của quảng cáo không được để trống.");

        int length = id.Trim().Length;
        if (length < MinIdLength || length > MaxIdLength)
            throw new DomainException($"Id của quảng cáo phải cố định từ {MinIdLength} đến {MaxIdLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ và giới hạn độ dài cho chuỗi văn bản tiêu đề quảng cáo.
    /// </summary>
    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Tiêu đề quảng cáo không được để trống.");

        if (title.Trim().Length > MaxTitleLength)
            throw new DomainException($"Tiêu đề quảng cáo không được phép vượt quá {MaxTitleLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ và giới hạn độ dài cho chuỗi văn bản tên nhà quảng cáo đối tác.
    /// </summary>
    private static void ValidateAdvertiser(string advertiser)
    {
        if (string.IsNullOrWhiteSpace(advertiser))
            throw new DomainException("Tên đơn vị nhà quảng cáo đối tác không được để trống.");

        if (advertiser.Trim().Length > MaxAdvertiserLength)
            throw new DomainException($"Tên đơn vị nhà quảng cáo không được phép vượt quá {MaxAdvertiserLength} ký tự.");
    }

    /// <summary>
    /// Xác thực tính hợp lệ của loại quảng cáo dựa trên tập hợp cấu trúc Enum được định nghĩa sẵn.
    /// </summary>
    private static void ValidateType(AdType type)
    {
        if (!Enum.IsDefined(typeof(AdType), type))
            throw new DomainException("Loại hình thức quảng cáo (AdType) không tồn tại trên hệ thống.");
    }

    /// <summary>
    /// Xác thực tính toàn vẹn dữ liệu của cấu trúc Value Object tài nguyên quảng cáo đi kèm.
    /// </summary>
    private static void ValidateMedia(AdMedia media)
    {
        if (media == null)
            throw new DomainException("Thông tin tài nguyên truyền thông quảng cáo (AdMedia) không được phép rỗng.");
    }

    /// <summary>
    /// Xác thực logic chuyển đổi trạng thái hoạt động của quảng cáo để tránh các thao tác cập nhật thừa thãi.
    /// </summary>
    /// <param name="currentStatus">Trạng thái IsActive hiện tại của thực thể quảng cáo.</param>
    /// <param name="newStatus">Trạng thái IsActive mới chuẩn bị được thay đổi.</param>
    private static void ValidateIsActive(bool currentStatus, bool newStatus)
    {
        if (currentStatus == newStatus)
            throw new DomainException($"Chiến dịch quảng cáo vốn dĩ đã ở trạng thái {(newStatus ? "Hoạt động" : "Tạm dừng")} từ trước.");
    }

    /// <summary>
    /// Xác thực tính toàn vẹn của mốc thời gian hệ thống ghi nhận lúc khởi tạo quảng cáo.
    /// </summary>
    private static void ValidateCreatedAt(DateTime createdAt)
    {
        if (createdAt == default)
            throw new DomainException("Thời gian tạo quảng cáo (CreatedAt) không được mang giá trị mặc định.");

        if (createdAt > DateTime.UtcNow.AddMinutes(1))
            throw new DomainException("Thời gian tạo quảng cáo không hợp lệ (không được vượt quá thời gian hiện tại).");
    }
}