# API_CONTRACT.md — TuneVault API Response Contract

> File này dùng để chuẩn hóa response giữa backend và frontend. Mọi endpoint mới hoặc endpoint được sửa phải đi theo contract này.

## 1. Response thành công

```json
{
  "success": true,
  "message": "Lấy dữ liệu thành công.",
  "data": {}
}
```

## 2. Response lỗi

```json
{
  "success": false,
  "message": "Không thể lấy dữ liệu.",
  "data": null
}
```

## 3. Validation lỗi nhiều field

Nếu cần trả nhiều lỗi validation, dùng `errors` nhưng vẫn giữ `message` tiếng Việt rõ ràng.

```json
{
  "success": false,
  "message": "Dữ liệu gửi lên không hợp lệ.",
  "data": null,
  "errors": [
    "Tên hiển thị không được để trống.",
    "Email không đúng định dạng."
  ]
}
```

## 4. Luật message

- Message trả về cho client dùng tiếng Việt.
- Không trả stack trace.
- Không trả raw SQL Server error.
- Không trả raw Dapper exception.
- Không trả nội dung exception hệ thống nếu có thể chứa thông tin nội bộ.

## 5. Message gợi ý theo tình huống

| Tình huống | Message |
|---|---|
| Lấy dữ liệu thành công | `Lấy dữ liệu thành công.` |
| Tạo mới thành công | `Tạo mới thành công.` |
| Cập nhật thành công | `Cập nhật thành công.` |
| Xóa thành công | `Xóa thành công.` |
| Không tìm thấy | `Không tìm thấy dữ liệu.` |
| Không có quyền | `Bạn không có quyền thực hiện thao tác này.` |
| Chưa đăng nhập | `Vui lòng đăng nhập để tiếp tục.` |
| Validation lỗi | `Dữ liệu gửi lên không hợp lệ.` |
| Lỗi hệ thống | `Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.` |
| Upload lỗi | `Không thể tải tệp lên.` |
| Stream lỗi | `Không thể phát nội dung này.` |

## 6. Controller rule

Controller không trả entity thô. Controller chỉ trả DTO hoặc response wrapper.

Ví dụ:

```csharp
return Ok(ApiResponse<UserDto>.Ok("Lấy thông tin người dùng thành công.", user));
```

Nếu `ApiResponse<T>` hiện tại chưa có overload message, AI phải đề xuất sửa wrapper trước khi sửa hàng loạt controller.

## 7. Không refactor hàng loạt nếu chưa được yêu cầu

Vì project đang ưu tiên sửa an toàn, khi chuẩn hóa response:

1. Sửa module đang làm trước.
2. Không sửa toàn bộ controller cùng lúc nếu developer chưa yêu cầu.
3. Nếu thấy module khác sai format, ghi vào `CURRENT_STATUS.md` hoặc báo lại để xử lý sau.
