# MASTER_PLAN — TuneVault

> **Mục đích:** File này cho AI Agent biết project TuneVault đang ở đâu, ưu tiên làm gì trước, và khi nào cần đọc các plan con.  
> **Quy tắc:** `AGENTS.md` là luật bắt buộc. `PLANS/*.md` là kế hoạch làm việc. Nếu có xung đột, ưu tiên `AGENTS.md`.

---

## 1. Định hướng tổng quát

Project TuneVault ưu tiên **Backend ASP.NET Core trước**. Frontend chỉ sửa khi backend contract đã rõ hoặc khi cần kiểm thử luồng end-to-end.

Ưu tiên khi AI Agent làm việc:

1. Đúng kiến trúc Clean Architecture.
2. Không phá code đang chạy.
3. Response API thống nhất.
4. Error message tiếng Việt, không lộ lỗi hệ thống.
5. Hoàn thiện backend trước frontend.
6. Làm từng module nhỏ, build sau mỗi thay đổi.
7. Thiếu dữ liệu thì hỏi lại, không tự bịa.

---

## 2. Trạng thái hiện tại

### Đã có hoặc đã bắt đầu

- Auth module.
- User module.
- Media module.
- Playlist module.
- Favorite module.
- History module.
- Share module.
- Notification module.
- SignalR infrastructure.
- React/Vite frontend cơ bản.

### Vấn đề hiện tại cần ưu tiên xử lý

- Response giữa các endpoint chưa thống nhất.
- Lỗi API đôi khi trả thẳng message hệ thống.
- Coding style giữa các thành viên chưa đồng nhất.
- Frontend đã có nhưng chưa ổn định.
- Streaming audio/video/poster chưa hoàn thiện đúng nghĩa.
- SQL schema do người dùng quản lý thủ công, AI không tự sửa schema nếu chưa được yêu cầu.

---

## 3. Thứ tự ưu tiên chính

AI Agent phải làm theo thứ tự ưu tiên sau, trừ khi người dùng yêu cầu khác.

```text
1. API response + error handling consistency
2. Auth/User stability
3. Media metadata + upload flow
4. Audio streaming
5. Video streaming
6. Poster/thumbnail delivery
7. Playlist
8. Favorite + PlayHistory
9. Share
10. Notification + SignalR
11. Frontend integration only when backend stable
12. Bonus AI features / deployment
```

---

## 4. Khi nào đọc từng plan con

| File | Khi nào cần đọc |
|---|---|
| `BACKEND_PLAN.md` | Trước khi sửa backend, controller, command/query, repository, service |
| `API_RESPONSE_PLAN.md` | Trước khi sửa response, error handling, middleware, controller |
| `STREAMING_PLAN.md` | Trước khi làm upload, audio stream, video stream, poster/thumbnail |
| `FRONTEND_PLAN.md` | Khi cần sửa frontend, route, axios service, player, SignalR client |
| `LINUX_MINT_ENVIRONMENT_PLAN.md` | Khi cần chạy build/test/dev server/database bằng lệnh terminal |
| `REFACTOR_SAFETY_PLAN.md` | Khi cần sửa code cũ, chuẩn hóa module hoặc đụng nhiều file |

---

## 5. Quy tắc cập nhật plan

Sau khi hoàn thành một task, AI Agent phải cập nhật checklist liên quan nếu được phép sửa plan.

Ví dụ:

```md
- [x] Tạo `MediaController` endpoint upload metadata.
- [ ] Thêm audio streaming endpoint hỗ trợ Range request.
```

Không đánh dấu `[x]` nếu:

- Chưa build thành công.
- Chưa kiểm tra response format.
- Chưa kiểm tra error handling.
- Task chỉ mới code một phần.

---

## 6. Cách chọn task tiếp theo

Nếu người dùng chỉ nói chung chung như:

```text
Làm tiếp giúp tôi.
```

AI Agent phải chọn task theo thứ tự:

1. Đọc `AGENTS.md`.
2. Đọc `MASTER_PLAN.md`.
3. Kiểm tra module đang làm gần nhất nếu có ngữ cảnh.
4. Ưu tiên task backend có rủi ro thấp.
5. Trình bày kế hoạch ngắn trước khi sửa code.
6. Chỉ implement sau khi người dùng đồng ý nếu task có thể ảnh hưởng nhiều module.

---

## 7. Các việc không tự làm nếu chưa hỏi

AI Agent không tự ý:

- Rewrite toàn bộ project.
- Đổi kiến trúc Clean Architecture.
- Đổi Dapper sang EF Core.
- Tạo/sửa SQL schema chính nếu người dùng chưa yêu cầu.
- Đổi response contract đã thống nhất.
- Thêm Docker Compose mới.
- Sửa frontend diện rộng.
- Thêm package lớn chỉ để giải quyết vấn đề nhỏ.

---

## 8. Gợi ý task khởi đầu tốt nhất

Nếu chưa biết bắt đầu từ đâu, task an toàn nhất là:

```text
Chuẩn hóa response và error handling cho một controller/module nhỏ trước.
```

Ví dụ module phù hợp để bắt đầu:

1. Favorite.
2. History.
3. Playlist.
4. Notification.

Không nên bắt đầu bằng streaming nếu response/error handling chưa ổn, vì streaming có nhiều edge case hơn.
