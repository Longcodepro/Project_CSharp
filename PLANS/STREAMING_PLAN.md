# STREAMING_PLAN — Audio, Video, Poster/Thumbnail

> **Mục đích:** Kế hoạch làm streaming cho TuneVault theo hướng an toàn, backend-first.

---

## 1. Thứ tự ưu tiên streaming

Làm theo thứ tự:

```text
1. Audio streaming
2. Video streaming
3. Poster/thumbnail delivery
4. Frontend player integration
```

Không làm video trước audio trừ khi người dùng yêu cầu rõ.

---

## 2. Nguyên tắc chung

Streaming không giống API JSON thường.

Endpoint stream nên:

- Trả file bằng `FileStreamResult` hoặc cơ chế tương đương trong ASP.NET Core.
- Hỗ trợ HTTP Range request.
- Cho phép seek trong player.
- Không load toàn bộ file vào memory.
- Kiểm tra media tồn tại trước khi stream.
- Kiểm tra quyền truy cập nếu media private.
- Không lộ path thật của file trên server.

---

## 3. Audio streaming

### Endpoint gợi ý

```http
GET /api/media/{id}/stream
```

### Checklist

- [ ] Lấy media metadata theo id.
- [ ] Kiểm tra media tồn tại.
- [ ] Kiểm tra media type là audio hoặc cho phép cả audio/video theo thiết kế.
- [ ] Kiểm tra file tồn tại trên disk/storage.
- [ ] Không trả path thật cho frontend.
- [ ] Trả content type đúng, ví dụ `audio/mpeg`, `audio/wav`, `audio/mp4`.
- [ ] Hỗ trợ Range request.
- [ ] Không bọc stream thành `ApiResponse<T>`.
- [ ] Lỗi trước khi stream trả response lỗi chuẩn tiếng Việt.

### Test thủ công

```bash
curl -I http://localhost:<port>/api/media/<id>/stream
curl -H "Range: bytes=0-1023" -I http://localhost:<port>/api/media/<id>/stream
```

Kỳ vọng:

- Request thường trả `200 OK` hoặc framework xử lý phù hợp.
- Request Range trả `206 Partial Content` nếu range hợp lệ.
- Có `Accept-Ranges: bytes` nếu framework hỗ trợ.

---

## 4. Video streaming

### Endpoint gợi ý

```http
GET /api/media/{id}/video/stream
```

hoặc dùng chung:

```http
GET /api/media/{id}/stream
```

Nếu dùng chung, logic phải xác định content type theo file/media type.

### Checklist

- [ ] Hỗ trợ file video phổ biến: mp4 trước.
- [ ] Content type đúng, ví dụ `video/mp4`.
- [ ] Hỗ trợ Range request.
- [ ] Không load toàn bộ video vào memory.
- [ ] Seek được trên frontend.
- [ ] Lỗi file missing trả message tiếng Việt.
- [ ] Không trả exception/path hệ thống.

---

## 5. Poster/thumbnail

### Endpoint gợi ý

```http
GET /api/media/{id}/poster
```

hoặc nếu poster là URL public:

```json
{
  "posterUrl": "/api/media/abc/poster"
}
```

### Checklist

- [ ] Trả image file bằng content type đúng.
- [ ] Có fallback nếu media chưa có poster.
- [ ] Không trả path thật.
- [ ] Không bắt frontend tự đoán đường dẫn file.

---

## 6. Upload liên quan streaming

Khi upload media:

- [ ] Lưu metadata vào database.
- [ ] Lưu file vào storage thông qua service abstraction.
- [ ] Lưu relative path hoặc storage key, không lưu path tuyệt đối nếu có thể tránh.
- [ ] Validate extension và content type.
- [ ] Không tự tạo/sửa SQL schema nếu chưa được người dùng yêu cầu.

---

## 7. Quyền truy cập streaming

Cần hỏi người dùng nếu chưa rõ:

- Media private có cho người khác stream không?
- Playlist private có ảnh hưởng quyền stream media không?
- Artist/admin có quyền stream media private không?
- Có cần giới hạn stream cho user chưa đăng nhập không?

Không tự bịa business rule.

---

## 8. Frontend integration sau cùng

Chỉ sửa frontend khi backend stream đã ổn.

Frontend cần:

- Audio player nhận `src` từ endpoint stream.
- Video player nhận `src` từ endpoint stream.
- Poster dùng endpoint poster.
- Không fetch stream bằng Axios kiểu JSON.
- Không base64 file media lớn.

---

## 9. Hoàn thành streaming task

Streaming task chỉ hoàn thành khi:

- [ ] Stream không load toàn bộ file vào RAM.
- [ ] Range request hoạt động.
- [ ] Player có thể seek.
- [ ] Lỗi trả message tiếng Việt.
- [ ] Không lộ file path.
- [ ] Build pass.
- [ ] Nếu sửa frontend, frontend build pass hoặc nêu rõ chưa test được.
