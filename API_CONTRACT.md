# API_CONTRACT.md — TuneVault Endpoint Summary

> Tài liệu này tóm tắt các endpoint hiện có trong `src/TuneVault.API/Controllers`.
> Dùng cho frontend tra cứu nhanh: endpoint nào làm gì, cần đăng nhập/role nào, và nên dùng trong chức năng nào.

## 1. Quy ước chung

Base URL local thường dùng: `http://localhost:<port>`.

Hầu hết response JSON dùng wrapper:

```json
{
  "success": true,
  "data": {},
  "message": "Thông báo tiếng Việt"
}
```

Lỗi thường có dạng:

```json
{
  "success": false,
  "data": null,
  "message": "Mô tả lỗi"
}
```

Các endpoint có `[Authorize]` thường lấy session từ cookie httpOnly `tunevault_access_token` sau khi user đăng nhập.
Frontend hiện không cần lưu JWT trong `localStorage`; nếu gọi từ client thì `fetch` cần bật `credentials: 'include'`.

Các endpoint có `[Authorize]` vẫn chấp nhận header:

```http
Authorization: Bearer <accessToken>
```

Các endpoint upload/cập nhật file dùng `multipart/form-data`, không gửi JSON.

## 2. Auth

| Method | Endpoint | Auth | Body | Dùng khi nào |
|---|---|---|---|---|
| `POST` | `/api/auth/login` | Public | JSON `{ idDisplay, password }` | Đăng nhập, backend set cookie access/refresh token và trả `accessToken`, `refreshToken`, `userId`, `idDisplay`, `roles`. |
| `POST` | `/api/auth/logout` | Public | Không cần body | Đăng xuất và xóa cookie auth của session hiện tại. |
| `POST` | `/api/auth/refresh` | Public | Không cần body | Gia hạn session bằng refresh token đang nằm trong cookie httpOnly. |
| `POST` | `/api/auth/send-otp` | Public | JSON `{ email, purpose }` với `purpose` là `register`, `reset_password` hoặc `change_password` | Gửi OTP cho đăng ký, quên mật khẩu hoặc đổi mật khẩu. |
| `POST` | `/api/auth/register` | Public | JSON `{ email, otpCode, idDisplay, displayName, password }` | Tạo tài khoản mới sau khi đã có OTP. Backend set cookie auth luôn sau khi đăng ký. |
| `POST` | `/api/auth/reset-password` | Public | JSON `{ email, otpCode, newPassword }` | Đặt lại mật khẩu bằng OTP cho user đã quên mật khẩu. |
| `POST` | `/api/auth/change-password` | User | JSON `{ email, oldPassword, otpCode, newPassword }` | Đổi mật khẩu của tài khoản đang đăng nhập sau khi xác minh OTP. |

## 3. Users & Follow

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/users/me/profile` | User | Không | Lấy hồ sơ đầy đủ của tài khoản đang đăng nhập. Response gồm thêm `email` để chỉ hiển thị, không cho sửa. |
| `PUT` | `/api/users/me/profile` | User | Form `{ idDisplay, displayName, bio, avatarFile?, removeAvatar }` | Cập nhật profile và avatar của chính mình. Response gồm thêm `email` để chỉ hiển thị, không cho sửa. |
| `PATCH` | `/api/users/{id}/verify-artist` | Admin | Không | Admin xác thực một user thành Artist. |
| `GET` | `/api/users/artists` | User | Không | Lấy danh sách artist đang hoạt động. |
| `GET` | `/api/users/by-handle/{idDisplay}` | User | Không | Tìm user theo handle công khai. |
| `GET` | `/api/users/{id}` | Public | Không | Xem hồ sơ public của user/artist. Response gồm `bio`, `createdAt` và `totalFollowers` để render profile xem-only. |
| `POST` | `/api/users/follow` | User | JSON `{ followeeId }` | Follow artist/user. |
| `DELETE` | `/api/users/unfollow` | User | JSON `{ followeeId }` | Bỏ follow artist/user. |
| `GET` | `/api/users/is-following/{followeeId}` | User | Không | Kiểm tra tài khoản hiện tại có follow user đó không. |
| `GET` | `/api/users/{followerId}/is-following/{followeeId}` | User | Không | Route cũ; chỉ hợp lệ khi `followerId` là user đang đăng nhập. |
| `GET` | `/api/users/{id}/following` | User | Không | Lấy danh sách user mà `{id}` đang follow. |
| `GET` | `/api/users/{id}/followers` | User | Không | Lấy followers của chính tài khoản hiện tại. Controller chặn xem followers của người khác. |
| `GET` | `/api/users/{id}/followers-count` | Public | Không | Hiển thị số follower trên trang artist/profile public. |

## 4. Media

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/media?page=1&pageSize=10` | Public | Query `page`, `pageSize` | Lấy danh sách media public cho home/discover. |
| `GET` | `/api/media/{id}` | Public | Không | Lấy chi tiết một media. |
| `GET` | `/api/media/my-media` | Artist/Listener | Không | Lấy media của chính người đăng nhập, gồm thông tin owner/detail. |
| `GET` | `/api/media/artist/{userId}` | Public | Không | Lấy media public của artist; nếu chính owner đang đăng nhập thì trả bản chi tiết. |
| `GET` | `/api/media/stream/{id}` | User | Không | Stream asset chính của media, hỗ trợ range processing. |
| `GET` | `/api/media/{id}/audio/stream` | User | Không | Stream file audio. |
| `GET` | `/api/media/{id}/video/stream` | User | Không | Stream file video. |
| `GET` | `/api/media/{id}/poster` | Public | Không | Lấy poster/cover của media. |
| `POST` | `/api/media/upload` | Artist/Listener | Form upload | Upload media dạng audio hoặc video, tự xác định theo `type`. |
| `POST` | `/api/media/upload/audio` | Artist/Listener | Form upload | Upload bắt buộc dạng audio. |
| `POST` | `/api/media/upload/video` | Artist/Listener | Form upload | Upload bắt buộc dạng video. |
| `PUT` | `/api/media/{id}` | Artist/Listener | Form update | Cập nhật metadata, cover, canvas của media mình sở hữu. |
| `DELETE` | `/api/media/{id}` | Artist/Listener | Không | Xóa mềm media mình sở hữu. |

Form upload media hỗ trợ: `audioFile`, `videoFile`, `coverImage`, `canvasFile`, `title`, `description`, `genre`, `type` (`Audio`/`Video`), `accessLevel` (`Normal`/`Premium` hoặc `0`/`1`), `isPublic`, `releaseDate`, `featuredArtistIds`.

## 5. Search & Discovery

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/search?keyword=...&page=1&pageSize=10` | Public | Query `keyword`, `page`, `pageSize` | Tìm kiếm media/playlist/artist theo từ khóa. |
| `GET` | `/api/search/trending?top=10` | Public | Query `top` | Lấy media thịnh hành cho explore/home. |

## 6. Playlists

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/playlists?limit=10` | Public | Query `limit` | Lấy playlist public cho trang khám phá. |
| `GET` | `/api/playlists/me` | User | Không | Lấy playlist của tài khoản hiện tại. |
| `GET` | `/api/playlists/{id}` | Public | Không | Xem chi tiết playlist; owner nhận DTO đầy đủ, người khác nhận DTO public. |
| `POST` | `/api/playlists` | User | Form `{ title, description?, isPublic, coverImage?, contentType?, releaseDate? }` | Tạo playlist mới. |
| `PUT` | `/api/playlists/{id}` | User | Form update | Cập nhật playlist của chính mình. |
| `DELETE` | `/api/playlists/{id}` | User | Không | Xóa mềm playlist của chính mình. |
| `POST` | `/api/playlists/{id}/tracks` | User | JSON `{ mediaItemId }` | Thêm media vào playlist. |
| `DELETE` | `/api/playlists/{id}/tracks/{mediaId}` | User | Không | Xóa media khỏi playlist. |
| `PATCH` | `/api/playlists/{playlistId}/tracks/{mediaItemId}/order?newOrder=1` | User | Query `newOrder` | Đổi thứ tự track trong playlist. |

## 7. Albums

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/albums?limit=10` | Public | Query `limit` | Lấy album public cho trang khám phá. |
| `GET` | `/api/albums/me` | Artist | Không | Lấy album của artist đang đăng nhập. |
| `GET` | `/api/albums/{id}` | Public | Không | Xem chi tiết album; owner nhận DTO đầy đủ, người khác nhận DTO public. |
| `POST` | `/api/albums` | Artist | Form `{ title, description?, coverImage?, isPublic, contentType?, releaseDate? }` | Tạo album mới. |
| `PUT` | `/api/albums/{id}` | Artist | Form update | Cập nhật album của chính artist. |
| `DELETE` | `/api/albums/{id}` | Artist | Không | Xóa mềm album của chính artist. |
| `POST` | `/api/albums/{id}/tracks` | Artist | JSON `{ mediaItemId }` | Thêm media vào album. |
| `DELETE` | `/api/albums/{id}/tracks/{mediaId}` | Artist | Không | Xóa media khỏi album. |
| `PATCH` | `/api/albums/{albumId}/tracks/{mediaId}/order?newOrder=1` | Artist | Query `newOrder` | Đổi thứ tự media trong album. |

## 8. Favorites / Reactions

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/favorites/reactions` | Public | Không | Lấy danh sách reaction enum để render UI chọn cảm xúc. |
| `PUT` | `/api/favorites/{mediaId}` | User | JSON `{ reaction }` hoặc body rỗng | Like/react media. Body rỗng mặc định là `Like`. |
| `DELETE` | `/api/favorites/{mediaId}` | User | Không | Bỏ reaction/unlike media. |
| `GET` | `/api/favorites/me` | User | Không | Lấy danh sách media user đã react. |
| `GET` | `/api/favorites/status/{mediaId}` | User | Không | Kiểm tra reaction hiện tại của user với media. |

## 9. Collection Likes

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/collection-likes/recent?limit=3` | User | Query `limit` | Lấy album/playlist đã thích gần đây cho sidebar/library. |
| `PUT` | `/api/collection-likes` | User | JSON `{ targetId, targetType }` | Toggle like/unlike album hoặc playlist. |

`targetType` là enum `CollectionLikeTargetType` ở backend; frontend nên dùng đúng giá trị enum backend đang expose/serialize.

## 10. Friends

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `POST` | `/api/friends/requests/{receiverId}` | User | Không | Gửi lời mời kết bạn. |
| `POST` | `/api/friends/requests/{requestId}/accept` | User | Không | Chấp nhận lời mời kết bạn. |
| `POST` | `/api/friends/requests/{requestId}/reject` | User | Không | Từ chối lời mời kết bạn. |
| `DELETE` | `/api/friends/requests/{requestId}` | User | Không | Hủy lời mời đã gửi. |
| `DELETE` | `/api/friends/{friendUserId}` | User | Không | Xóa bạn khỏi danh sách. |
| `GET` | `/api/friends/me` | User | Không | Lấy danh sách bạn bè hiện tại. |
| `GET` | `/api/friends/requests/inbox` | User | Không | Lấy lời mời kết bạn nhận được. |
| `GET` | `/api/friends/requests/sent` | User | Không | Lấy lời mời kết bạn đã gửi. |

## 11. Shares

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `POST` | `/api/shares` | User | JSON `{ receiverId, sharedItemId, shareType, message? }` | Chia sẻ media/video/playlist cho user khác. |
| `GET` | `/api/shares/inbox` | User | Không | Lấy danh sách item người khác chia sẻ cho mình. |
| `GET` | `/api/shares/sent` | User | Không | Lấy danh sách item mình đã chia sẻ. |

`shareType` theo comment hiện tại hỗ trợ các tên như `Track`, `Media`, `Video`, `Song`, `Playlist`.

## 12. Notifications

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/notifications?limit=50` | User | Query `limit` | Lấy thông báo của user hiện tại. |
| `GET` | `/api/notifications/unread?limit=50` | User | Query `limit` | Lấy thông báo chưa đọc. |
| `GET` | `/api/notifications/unread-count` | User | Không | Lấy số lượng thông báo chưa đọc để hiển thị badge. |
| `PATCH` | `/api/notifications/{notificationId}/read` | User | Không | Đánh dấu một thông báo đã đọc. |
| `PATCH` | `/api/notifications/read-all` | User | Không | Đánh dấu toàn bộ thông báo đã đọc. |
| `DELETE` | `/api/notifications/{notificationId}` | User | Không | Xóa mềm một thông báo. |

Realtime hub hiện map tại `/hubs/notifications` để client nhận notification qua SignalR.

## 13. History

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/api/history/recent` | User | Không | Lấy lịch sử phát gần đây. |
| `POST` | `/api/history/{mediaId}` | User | Không | Ghi nhận media vừa được phát. |
| `PATCH` | `/api/history/{mediaId}/stop` | User | JSON `{ stoppedAt }` | Lưu vị trí dừng phát theo giây. |
| `GET` | `/api/history/{mediaId}/resume` | User | Không | Lấy vị trí phát tiếp của media. |

## 14. System

| Method | Endpoint | Auth | Body/Query | Dùng khi nào |
|---|---|---|---|---|
| `GET` | `/` | Public | Không | Kiểm tra service trả lời tên API. |
| `GET` | `/health` | Public | Không | Health check đơn giản. |
| `GET` | `/swagger` | Development | Không | Mở Swagger UI khi chạy môi trường Development. |

## 15. Ghi chú tích hợp frontend

- Dùng `/api/auth/login` trước, nhưng frontend hiện nên dựa vào cookie httpOnly và `credentials: 'include'` thay vì lưu JWT trong `localStorage`.
- Khi gọi endpoint cần auth, nếu nhận `401` thì frontend có thể gọi `/api/auth/refresh` rồi retry request một lần.
- Các route stream trả file/redirect/physical file, không trả `ApiResponse<T>` như JSON API thông thường.
- Các form upload chỉ nhận định dạng phổ biến:
  - Audio: `.mp3`, `.wav`, `.m4a`, `.flac`, `.ogg`.
  - Video/canvas: `.mp4`, `.webm`.
  - Image/avatar/cover: `.jpg`, `.jpeg`, `.png`, `.webp`.
- Một số endpoint public vẫn trả dữ liệu khác nếu request có token của owner, ví dụ `GET /api/media/artist/{userId}`, `GET /api/playlists/{id}`, `GET /api/albums/{id}`.
- Nếu nhận `401`, frontend nên điều hướng đăng nhập hoặc refresh state token. Nếu nhận `403`, user đã đăng nhập nhưng không đủ quyền.
