# API_RESPONSE_PLAN — Chuẩn hóa response và error handling

> **Mục đích:** Giúp AI Agent sửa response API từng bước, an toàn, không phá toàn bộ project.

---

## 1. Response format chuẩn

Mọi endpoint JSON thông thường phải trả về dạng:

```json
{
  "success": true,
  "message": "Lấy dữ liệu thành công.",
  "data": {}
}
```

Khi lỗi:

```json
{
  "success": false,
  "message": "Không thể lấy dữ liệu.",
  "data": null
}
```

Nếu project đã có `errors`, có thể dùng thêm nhưng không bắt buộc cho mọi endpoint:

```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ.",
  "data": null,
  "errors": [
    "Tên playlist không được để trống."
  ]
}
```

---

## 2. Ngôn ngữ message

- Message trả về frontend: tiếng Việt.
- Tên class, method, DTO, command, query: tiếng Anh.
- Log nội bộ: có thể tiếng Anh hoặc tiếng Việt, ưu tiên rõ nghĩa.

Ví dụ tốt:

```json
{
  "success": false,
  "message": "Không tìm thấy bài hát.",
  "data": null
}
```

Ví dụ không được dùng:

```json
{
  "success": false,
  "message": "Sequence contains no elements",
  "data": null
}
```

---

## 3. Không lộ lỗi hệ thống

Không bao giờ trả trực tiếp các lỗi sau cho client:

- Stack trace.
- SQL Server exception.
- Dapper exception.
- NullReferenceException.
- Connection string.
- File path tuyệt đối trên máy dev.
- JWT secret hoặc config nhạy cảm.

Ví dụ sai:

```json
{
  "success": false,
  "message": "Violation of PRIMARY KEY constraint 'PK_PlaylistTrack'..."
}
```

Ví dụ đúng:

```json
{
  "success": false,
  "message": "Bài hát đã tồn tại trong playlist.",
  "data": null
}
```

---

## 4. Mapping lỗi gợi ý

| Trường hợp | HTTP Status | Message tiếng Việt gợi ý |
|---|---:|---|
| Validation fail | 400 | `Dữ liệu không hợp lệ.` |
| Chưa đăng nhập | 401 | `Bạn cần đăng nhập để thực hiện thao tác này.` |
| Không có quyền | 403 | `Bạn không có quyền thực hiện thao tác này.` |
| Không tìm thấy | 404 | `Không tìm thấy dữ liệu yêu cầu.` |
| Duplicate | 400 hoặc 409 | `Dữ liệu đã tồn tại.` |
| Business rule fail | 400 | Message cụ thể theo nghiệp vụ |
| Lỗi hệ thống | 500 | `Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.` |

---

## 5. Thứ tự chuẩn hóa an toàn

Không refactor toàn bộ cùng lúc. Làm từng module.

Thứ tự đề xuất:

```text
1. BaseApiController / ApiResponse helper
2. Exception middleware nếu có
3. FavoriteController
4. HistoryController
5. PlaylistController
6. MediaController
7. AuthController
8. UsersController
9. ShareController
10. NotificationController
```

Lý do:

- Favorite/History thường nhỏ hơn, ít rủi ro.
- Auth/User ảnh hưởng nhiều frontend hơn nên làm sau khi pattern ổn.
- Media/Streaming có response đặc biệt nên cần cẩn thận.

---

## 6. Quy tắc với streaming endpoint

Streaming endpoint không nhất thiết bọc file stream trong `ApiResponse<T>` vì stream trả binary content.

Tuy nhiên lỗi của streaming endpoint vẫn phải trả response lỗi chuẩn nếu chưa bắt đầu stream.

Ví dụ:

```json
{
  "success": false,
  "message": "Không tìm thấy file media.",
  "data": null
}
```

Không được trả:

```text
FileNotFoundException: /home/user/...
```

---

## 7. Controller checklist

Khi sửa một controller, kiểm tra:

- [ ] Không trả anonymous object tùy tiện nếu endpoint JSON thường.
- [ ] Không trả entity thô.
- [ ] Success response có message rõ ràng.
- [ ] Error response có message tiếng Việt.
- [ ] Không catch exception rồi trả `ex.Message` trực tiếp.
- [ ] Có `ProducesResponseType`.
- [ ] Không gọi repository trực tiếp.
- [ ] Controller gọi MediatR hoặc Application service đúng kiến trúc.

---

## 8. Handler checklist

Khi sửa handler:

- [ ] Throw domain/application exception có chủ ý khi lỗi nghiệp vụ.
- [ ] Không throw exception chung chung nếu có thể trả lỗi rõ nghĩa.
- [ ] Không return entity thô.
- [ ] Map sang DTO.
- [ ] Không hardcode message lỗi hệ thống.
- [ ] Nếu transaction cần thiết, dùng ở Infrastructure/repository hoặc service phù hợp.

---

## 9. Quy tắc migration từng bước

Khi chuẩn hóa response cho module đang làm:

1. Đọc controller hiện tại.
2. Ghi lại các endpoint và response hiện tại.
3. Đề xuất mapping response mới.
4. Sửa ít file nhất có thể.
5. Build.
6. Test bằng Swagger/curl nếu có thể.
7. Không sửa module khác nếu không cần.

---

## 10. Hoàn thành API response task

Task chuẩn hóa response chỉ hoàn thành khi:

- [ ] Endpoint thành công trả đúng format.
- [ ] Endpoint lỗi trả message tiếng Việt.
- [ ] Không lộ lỗi hệ thống.
- [ ] Không phá status code hợp lý.
- [ ] Build pass.
