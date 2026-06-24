# REFACTOR_SAFETY_PLAN — Sửa code an toàn

> **Mục đích:** Project đang có nhiều người code, style không thống nhất. File này giúp AI Agent refactor cẩn thận, tránh phá chức năng hiện có.

---

## 1. Nguyên tắc sửa an toàn

Người dùng chọn hướng:

```text
An toàn: chỉ sửa module đang làm, hạn chế đụng code cũ.
```

Vì vậy AI Agent phải:

- Sửa ít file nhất có thể.
- Không rewrite toàn bộ module nếu không cần.
- Không đổi public API nếu frontend đang dùng, trừ khi đang chuẩn hóa có chủ ý.
- Không đổi database schema nếu chưa hỏi.
- Không đổi package/framework nếu không cần.
- Không xóa code cũ khi chưa hiểu rõ tác dụng.

---

## 2. Khi nào được refactor

Được refactor cục bộ khi:

- Đang sửa chính module đó.
- Response/error handling cần thống nhất.
- Code hiện tại làm lộ lỗi hệ thống.
- Code vi phạm Clean Architecture rõ ràng.
- Code gây build error.
- Code mới cần style thống nhất với `AGENTS.md`.

---

## 3. Khi nào phải hỏi trước

Phải hỏi người dùng trước khi:

- Đổi database schema.
- Đổi route endpoint public.
- Đổi response contract diện rộng.
- Đổi JWT payload.
- Đổi cách lưu file media.
- Đổi folder structure lớn.
- Xóa một class/service/repository đang được dùng.
- Thêm Docker Compose hoặc CI/CD.

---

## 4. Phong cách comment

Comment nên giống người viết code có kinh nghiệm, không máy móc.

Tránh:

```csharp
// Create object
// Set value
// Check null
```

Nên viết:

```csharp
// User can follow this artist again later, so the relationship is soft-deleted
// instead of being removed from the database.
```

```csharp
// Streaming endpoints should never expose the physical file path to the client.
```

Comment chỉ thêm khi giúp hiểu business rule, quyết định kỹ thuật hoặc edge case. Không comment những dòng code quá hiển nhiên.

---

## 5. Không bịa thông tin thiếu

Nếu thiếu thông tin, AI Agent phải dừng và hỏi.

Không tự bịa:

- Tên cột database.
- Business rule.
- DTO field.
- Route frontend.
- Role/permission.
- Cách tính recommendation.
- File storage path.
- Giới hạn dung lượng upload.

Format hỏi lại:

```md
Mình cần xác nhận thêm trước khi sửa:

1. [Câu hỏi cụ thể]
2. [Câu hỏi cụ thể]

Lý do: nếu tự đoán phần này có thể làm sai schema hoặc sai business rule.
```

---

## 6. Quy trình refactor từng module

1. Đọc controller/handler/repository liên quan.
2. Ghi lại vấn đề hiện tại.
3. Đề xuất sửa nhỏ nhất.
4. Sửa code.
5. Build.
6. Nếu build lỗi, sửa tối đa theo protocol.
7. Báo cáo file đã sửa và lý do.

---

## 7. Checklist trước khi kết thúc refactor

- [ ] Chỉ sửa module/task liên quan.
- [ ] Không đổi schema.
- [ ] Không đổi route nếu chưa cần.
- [ ] Response chuẩn hơn trước.
- [ ] Không lộ lỗi hệ thống.
- [ ] Comment tự nhiên, không máy móc.
- [ ] Build pass hoặc báo cáo lỗi đúng format.
