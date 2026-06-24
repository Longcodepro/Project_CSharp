# BACKEND_PLAN — TuneVault Backend

> **Mục đích:** Kế hoạch backend-first cho TuneVault. Backend là ưu tiên chính vì chiếm phần lớn điểm và là nền tảng cho frontend.

---

## 1. Nguyên tắc backend

Backend phải tuân thủ:

```text
Domain → Infrastructure → Application → API
```

Không được:

- Controller gọi repository trực tiếp.
- Controller gọi Dapper trực tiếp.
- Application reference Infrastructure.
- Trả entity thô ra API.
- Trả lỗi hệ thống trực tiếp cho frontend.

---

## 2. Checklist nền tảng backend

- [ ] Kiểm tra `dotnet restore` chạy được trên Linux Mint.
- [ ] Kiểm tra `dotnet build TuneVault.sln` chạy được.
- [ ] Xác định warning hiện có, không tạo thêm warning mới.
- [ ] Kiểm tra DI registration trong `Program.cs`.
- [ ] Kiểm tra Swagger hoạt động.
- [ ] Kiểm tra JWT middleware hoạt động.
- [ ] Kiểm tra connection string không hardcode secret.
- [ ] Kiểm tra `appsettings.Development.json` không bị commit.

---

## 3. Auth module

### Mục tiêu

Đăng ký, đăng nhập, xác thực người dùng, phát JWT.

### Checklist

- [ ] Register endpoint trả `ApiResponse<T>` chuẩn.
- [ ] Login endpoint trả `ApiResponse<T>` chuẩn.
- [ ] Lỗi đăng nhập sai trả message tiếng Việt, không lộ lỗi hệ thống.
- [ ] Password xử lý đúng theo rule hiện tại của project.
- [ ] JWT secret đọc từ configuration/environment.
- [ ] Không trả `PasswordHash` ra API.
- [ ] Validator đầy đủ cho request.
- [ ] Swagger có `ProducesResponseType`.

### Không tự ý làm

- Không đổi cơ chế password nếu chưa hỏi.
- Không đổi cấu trúc token nếu frontend đang phụ thuộc.

---

## 4. User module

### Mục tiêu

Ổn định profile, follow/unfollow, artist verification.

### Checklist

- [ ] Response tất cả endpoint User thống nhất.
- [ ] Follow self trả lỗi tiếng Việt.
- [ ] Duplicate follow trả lỗi tiếng Việt.
- [ ] Unfollow dùng soft delete theo rule.
- [ ] Không để `TotalFollowers` âm.
- [ ] Không trả field nhạy cảm.
- [ ] XML summary đầy đủ cho code mới/chỉnh sửa.

---

## 5. Media module

### Mục tiêu

Quản lý metadata media, upload file, chuẩn bị streaming.

### Checklist

- [ ] `MediaItem` entity không bị trả trực tiếp ra API.
- [ ] Upload metadata qua Command/Handler.
- [ ] Upload file qua service abstraction, không xử lý file trực tiếp trong controller nếu có thể tránh.
- [ ] Kiểm tra owner trước khi sửa/xóa media.
- [ ] Validate loại file audio/video.
- [ ] Validate kích thước file nếu đã có rule.
- [ ] Response upload chuẩn `{ success, message, data }`.
- [ ] Không tự sửa SQL schema nếu người dùng chưa yêu cầu.

### Liên quan

Đọc thêm `STREAMING_PLAN.md` trước khi làm stream.

---

## 6. Playlist module

### Mục tiêu

CRUD playlist, thêm/xóa track, public/private.

### Checklist

- [ ] Create playlist.
- [ ] Get playlist by id.
- [ ] Get playlists by user.
- [ ] Update playlist.
- [ ] Delete playlist hoặc soft delete nếu project đang dùng.
- [ ] Add track to playlist.
- [ ] Remove track from playlist.
- [ ] Kiểm tra ownership khi sửa/xóa.
- [ ] Không cho thêm media không tồn tại.
- [ ] Không duplicate track nếu business rule yêu cầu.

### Cần hỏi nếu thiếu

- Playlist delete là hard delete hay soft delete?
- Playlist public/private ảnh hưởng quyền xem thế nào?
- Track order có tự tăng không?

---

## 7. Favorite module

### Mục tiêu

Like/favorite media.

### Checklist

- [ ] Toggle favorite.
- [ ] Get favorites by current user.
- [ ] Check favorite status.
- [ ] Không favorite media không tồn tại.
- [ ] Response lỗi tiếng Việt.
- [ ] Không expose exception hệ thống.

---

## 8. PlayHistory module

### Mục tiêu

Ghi lịch sử nghe/xem và lấy danh sách gần nhất.

### Checklist

- [ ] Record play history.
- [ ] Get recent play history.
- [ ] Giới hạn 10 bài gần nhất nếu theo yêu cầu đề.
- [ ] Không ghi lịch sử nếu media không tồn tại.
- [ ] Có thể dùng cho recommendation sau này.

### Cần hỏi nếu thiếu

- Một media nghe lại nhiều lần thì tạo nhiều record hay update `PlayedAt`?
- Có cần phân biệt audio/video trong history không?

---

## 9. Share module

### Mục tiêu

Chia sẻ media/playlist cho user khác.

### Checklist

- [ ] Share media.
- [ ] Share playlist nếu schema hỗ trợ.
- [ ] Get shared with me.
- [ ] Get shared by me.
- [ ] Không cho sender = receiver.
- [ ] Tạo notification nếu module notification sẵn sàng.
- [ ] Transaction khi tạo share + notification.
- [ ] Response chuẩn, lỗi tiếng Việt.

---

## 10. Notification + SignalR

### Mục tiêu

Lưu notification vào DB và push realtime nếu có kết nối.

### Checklist

- [ ] Get notifications.
- [ ] Mark as read.
- [ ] Mark all as read nếu cần.
- [ ] Tạo notification từ share/follow nếu có yêu cầu.
- [ ] SignalR hub không chứa business logic nặng.
- [ ] Push service nằm ở Infrastructure/Application abstraction hợp lý.

---

## 11. Search module

### Mục tiêu

Tìm media, artist, playlist.

### Checklist

- [ ] Search media theo keyword.
- [ ] Search user/artist theo keyword.
- [ ] Search playlist public.
- [ ] Có phân trang nếu response lớn.
- [ ] Query Dapper parameterized.
- [ ] Không dùng string interpolation trong SQL.

---

## 12. Backend Definition of Done

Một backend task chỉ hoàn thành khi:

- [ ] `dotnet build TuneVault.sln` pass.
- [ ] Không thêm warning mới.
- [ ] Response chuẩn `{ success, message, data }`.
- [ ] Lỗi trả tiếng Việt.
- [ ] Không lộ exception hệ thống.
- [ ] Không trả entity thô.
- [ ] Có XML summary cho code mới/chỉnh sửa.
- [ ] Dependency mới đã đăng ký DI.
- [ ] Swagger hiển thị endpoint mới.
- [ ] Không tự sửa schema nếu chưa được yêu cầu.
