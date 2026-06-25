namespace TuneVault.Application.Common;

/// <summary>
/// Định dạng phản hồi chuẩn hóa cho toàn bộ API của TuneVault.
/// Mọi endpoint (thành công hoặc lỗi) đều trả về kiểu này để Frontend xử lý nhất quán.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về trong trường Data.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>Cho biết request có xử lý thành công hay không.</summary>
    public bool Success { get; init; }

    /// <summary>Dữ liệu trả về khi thành công; null khi thất bại.</summary>
    public T? Data { get; init; }

    /// <summary>Thông báo mô tả kết quả hoặc lỗi (tiếng Việt, hiển thị được cho người dùng).</summary>
    public string? Message { get; init; }

    /// <summary>Chi tiết lỗi kỹ thuật (chỉ hiển thị ở môi trường Development).</summary>
    public string? Detail { get; init; }

    /// <summary>Tạo response thành công kèm dữ liệu và message tùy chọn.</summary>
    public static ApiResponse<T> Ok(T? data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    /// <summary>Tạo response lỗi với message bắt buộc, Data luôn null.</summary>
    public static ApiResponse<T> Fail(string message, string? detail = null) =>
        new() { Success = false, Data = default, Message = message, Detail = detail };
}
