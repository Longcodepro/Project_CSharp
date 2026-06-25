# FRONTEND_PLAN — TuneVault Frontend

> **Mục đích:** Frontend đã có nhưng chưa ổn định. AI Agent chỉ sửa frontend khi cần thiết cho backend contract hoặc khi người dùng yêu cầu rõ.

---

## 1. Nguyên tắc frontend

Frontend không phải ưu tiên chính hiện tại.

Chỉ sửa frontend khi:

- Backend endpoint đã ổn định.
- Cần test luồng end-to-end.
- Người dùng yêu cầu sửa UI/route cụ thể.
- Cần cập nhật service layer để khớp response backend.

Không tự rewrite toàn bộ frontend.

---

## 2. Response contract frontend phải hiểu

Backend JSON thường trả:

```json
{
  "success": true,
  "message": "Lấy dữ liệu thành công.",
  "data": {}
}
```

Frontend service layer phải đọc `response.data.data` nếu dùng Axios.

Ví dụ:

```js
const res = await api.get('/media');
return res.data.data;
```

Không giả định backend trả DTO trực tiếp.

---

## 3. Axios service layer

Checklist:

- [ ] Có instance Axios dùng chung.
- [ ] Có base URL rõ ràng qua env/config.
- [ ] Có interceptor gắn JWT nếu có.
- [ ] Xử lý lỗi theo `message` từ backend.
- [ ] Không hiển thị lỗi hệ thống/raw object cho user.

---

## 4. Routes tối thiểu theo đề

Các route nên có hoặc sẽ cần có:

- [ ] `/login`
- [ ] `/home`
- [ ] `/search`
- [ ] `/library`
- [ ] `/playlist/:id`
- [ ] `/share-inbox`
- [ ] `/notifications`
- [ ] `/profile`

Nếu frontend hiện tại chưa đủ route, không tự tạo toàn bộ cùng lúc. Làm từng route theo backend module đã hoàn thiện.

---

## 5. Player integration

Chỉ làm sau khi backend streaming ổn.

### Audio player

- [ ] Dùng stream URL làm `src`.
- [ ] Không tải toàn bộ file bằng Axios.
- [ ] Hỗ trợ play/pause/seek.
- [ ] Gọi history endpoint khi play nếu backend đã có.

### Video player

- [ ] Dùng stream URL làm `src`.
- [ ] Dùng poster URL nếu có.
- [ ] Không base64 video.

---

## 6. SignalR client

Chỉ làm khi backend notification hub đã ổn.

Checklist:

- [ ] Cài `@microsoft/signalr` nếu chưa có.
- [ ] Kết nối hub bằng JWT nếu endpoint yêu cầu auth.
- [ ] Lắng nghe notification event.
- [ ] Cập nhật UI notification.
- [ ] Có reconnect strategy cơ bản.

---

## 7. UI style

Vì project tham khảo Spotify:

- Dark theme.
- Sidebar.
- Content area.
- Bottom player bar.
- Layout responsive cơ bản.

Không ưu tiên animation/phức tạp nếu backend chưa xong.

---

## 8. Linux Mint frontend commands

```bash
cd client
npm install
npm run dev
npm run build
```

Nếu project frontend nằm ở thư mục khác, AI phải kiểm tra `package.json` trước khi chạy lệnh.

---

## 9. Hoàn thành frontend task

Frontend task chỉ hoàn thành khi:

- [ ] Không phá route hiện có.
- [ ] Service layer khớp response backend.
- [ ] Không hardcode URL nếu đã có env/config.
- [ ] Không fetch stream như JSON.
- [ ] `npm run build` pass nếu môi trường cho phép.
