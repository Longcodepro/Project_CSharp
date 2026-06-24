# CURRENT_STATUS.md — TuneVault Current Project Status

> File này ghi trạng thái thực tế đã biết để AI Agent không làm lại từ đầu hoặc tự đoán. Cập nhật file này sau mỗi module lớn.

## 1. Hướng phát triển hiện tại

- Ưu tiên: Backend ASP.NET Core trước.
- Frontend: đã có nhưng chưa ổn định, chỉ sửa khi cần nối API hoặc kiểm tra contract.
- Database: SQL Server chạy bằng Docker trên Linux Mint.
- ORM: Dapper, không dùng EF Core.
- SQL schema: developer có thể tự tạo/cập nhật thủ công.

## 2. Đã biết từ AGENTS.md

### Đã hoàn thành tương đối

- User Module.
- Follow / Unfollow.
- Verify Artist.
- User profile endpoints.

### Chưa hoàn thành hoặc cần làm tiếp

- Auth.
- Playlist.
- Media upload/stream.
- Favorite.
- Notification.
- History.
- Album.
- Share.
- SignalR.

## 3. Vấn đề hiện tại do developer ghi nhận

- Code style chưa đồng nhất vì nhiều người code khác nhau.
- API response chưa thống nhất giữa các endpoint.
- Một số lỗi endpoint trả thẳng message hệ thống thay vì message chuẩn hóa.
- Chưa có streaming audio/video/poster/thumbnail hoàn chỉnh xuống frontend.
- Frontend đã có nhưng chưa ổn định.

## 4. Ưu tiên sửa gần nhất

1. Chuẩn hóa API response và error handling theo `API_CONTRACT.md`.
2. Đồng bộ Dapper query với `DATABASE_SCHEMA.md`.
3. Làm Media theo thứ tự:
   - Audio streaming.
   - Video streaming.
   - Poster/thumbnail.
   - Frontend player integration khi backend contract đã ổn.
4. Làm tiếp các module backend còn thiếu theo `PLANS/BACKEND_PLAN.md`.

## 5. Quy tắc cập nhật file này

Khi hoàn thành một task lớn, AI Agent phải đề xuất cập nhật phần tương ứng trong file này. Không tự đánh dấu hoàn thành nếu chưa build/test hoặc chưa được developer xác nhận.
