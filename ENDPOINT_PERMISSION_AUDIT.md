# TuneVault Endpoint Permission Audit

> Tài liệu này ghi lại phân quyền và rule nghiệp vụ đang phản ánh trong codebase hiện tại.
> Đây là tài liệu kiểm tra kỹ thuật, không thay thế business rule cuối cùng.

## Quy ước

- `User từ token`: user hiện tại lấy từ JWT claim hoặc `ICurrentUserContext`.
- `Owner`: chủ sở hữu tài nguyên theo DB.
- `IsActive = 1`: còn hoạt động, chưa xóa mềm.
- `IsPublic = 1`: được hiển thị công khai.
- `MediaItems.IsValid = 0`: media hoạt động bình thường.
- `MediaItems.IsValid = 1`: media đang bị admin khóa vì vi phạm.

## 1. Auth

Base route: `/api/auth`

| Endpoint | Đăng nhập | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|
| `POST /api/auth/login` | Không | Public | Login user/admin, tạo JWT có user id và role | Kiểm tra role claim phải khớp `Program.cs` |
| `POST /api/auth/send-otp` | Không | Public | Gửi OTP qua email service | Cần DB/email local đúng config |
| `POST /api/auth/register` | Không | Public | Validate email/idDisplay, hash password | Không có |
| `POST /api/auth/reset-password` | Không | Public | Reset theo OTP/email | Không có |

## 2. Media

Base route: `/api/media`

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `GET /api/media` | Không | Không | Public list | Lấy media `IsActive = 1` và `IsValid = 0` | Hiện không lọc `IsPublic` trong `GetPagedAsync`; nếu chỉ muốn public media thì cần siết lại |
| `GET /api/media/{id}` | Không | Không | Public detail | Repository chỉ lọc `IsActive = 1`; DTO trả stream endpoint, không trả path vật lý | Hiện chưa chặn `IsPublic = 0` hoặc `IsValid = 1` ở detail |
| `GET /api/media/my-media` | Có | Có | Chỉ lấy media của user hiện tại | Lấy theo `OwnerId`, `IsActive = 1`; owner vẫn thấy media bị khóa `IsValid = 1` | Hợp lý cho trang quản lý của artist |
| `GET /api/media/artist/{userId}` | Không | Không | Public xem media theo artist id | Dùng chung `GetByOwnerAsync`, chỉ lọc `OwnerId` và `IsActive = 1` | Public vẫn có thể thấy media private/vi phạm; nên siết `IsPublic = 1 AND IsValid = 0` |
| `GET /api/media/stream/{id}` | Không | Không | Public stream primary asset | Lọc `IsActive = 1 AND IsValid = 0`, trả file bằng `PhysicalFile`, có Range | Chưa chặn media private; nếu theo rule mới thì cần yêu cầu login và check owner/public |
| `GET /api/media/{id}/audio/stream` | Không | Không | Public stream audio | Lọc `IsActive = 1 AND IsValid = 0`, không lộ path server | Chưa chặn `IsPublic = 0` |
| `GET /api/media/{id}/video/stream` | Không | Không | Public stream video | Lọc `IsActive = 1 AND IsValid = 0`, không lộ path server | Chưa chặn `IsPublic = 0` |
| `GET /api/media/{id}/poster` | Không | Không | Public poster | Lọc `IsActive = 1 AND IsValid = 0`, trả ảnh nếu file tồn tại | Chưa chặn `IsPublic = 0` |
| `POST /api/media/upload` | Có, role `Artist,Admin` | Có | Handler chỉ cho upload với `OwnerId = CurrentUserId`; owner phải `IsArtist = true` | Validate file theo type, lưu audio/video/cover vào `wwwroot/uploads`, featured artist phải là artist | Admin vẫn nằm trong role attribute; nên đổi thành `Artist` |
| `POST /api/media/upload/audio` | Có, role `Artist,Admin` | Có | Tương tự upload chung, ép type audio | Validate audio file | Admin không được upload theo rule mới |
| `POST /api/media/upload/video` | Có, role `Artist,Admin` | Có | Tương tự upload chung, ép type video | Validate video file | Admin không được upload theo rule mới |
| `PUT /api/media/{id}` | Có, role `Artist,Admin` | Có | Handler chỉ owner media được sửa | Update title/description/genre/cover/canvas/access/public; giữ URL file | Admin không được sửa media theo rule mới |
| `DELETE /api/media/{id}` | Có, role `Artist,Admin` | Có | Handler chỉ owner media được xóa | Soft delete `IsActive = 0` | Admin không được xóa media theo rule mới |

## 3. Playlist

Base route: `/api/playlists`, controller có `[Authorize]`.

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `GET /api/playlists` | Có | Có | Chỉ lấy playlist của user hiện tại | Lọc `UserId = CurrentUserId`, `IsActive = 1` | Không có |
| `GET /api/playlists/me` | Có | Có | Alias của endpoint trên | Như trên | Không có |
| `GET /api/playlists/{id}` | Có | Có | Playlist private chỉ owner xem; public user đăng nhập nào cũng xem | Lọc playlist `IsActive = 1` | Nếu muốn khách chưa login xem playlist public thì cần bỏ/đổi `[Authorize]` |
| `POST /api/playlists` | Có | Có | Tạo playlist cho user hiện tại | Validate entity; có `IsPublic`, `ContentType`, `ReleaseDate` | Admin hiện chưa bị chặn rõ nếu token hợp lệ |
| `PUT /api/playlists/{id}` | Có | Có | Chỉ owner được sửa | Update title/description/cover/public/content type/release date; không đổi release date nếu đã phát hành | Chưa có logic chuyển private -> public thì public hóa media private |
| `DELETE /api/playlists/{id}` | Có | Có | Chỉ owner được xóa | Soft delete `IsActive = 0` | Không có |
| `POST /api/playlists/{id}/tracks` | Có | Có | Chỉ owner playlist được thêm track | Check playlist tồn tại, media tồn tại active và không vi phạm, không trùng | Chưa enforce public/private rule của playlist/media; chưa giới hạn max 20; hiện thêm vào cuối, chưa đưa order mới lên `1` |
| `DELETE /api/playlists/{id}/tracks/{mediaId}` | Có | Có | Chỉ owner playlist được xóa track | Check track tồn tại; reorder các track còn lại | Không có |
| `PATCH /api/playlists/{playlistId}/tracks/{mediaItemId}/order` | Có | Có | Chỉ owner playlist đổi thứ tự | Check `newOrder` từ 1 đến số track hiện có; reorder tránh trùng order | Không có |

## 4. Favorite

Base route: `/api/favorites`, controller có `[Authorize]`.

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `GET /api/favorites/reactions` | Không | Không | Public enum metadata | Trả danh sách reaction hợp lệ để Swagger/frontend render lựa chọn | Không có |
| `POST /api/favorites/{mediaId}` | Có | Có, qua `ICurrentUserContext` | User chỉ tạo/cập nhật reaction cho chính mình | Body tùy chọn `{ reaction }`; không gửi body thì mặc định `Like`; check media tồn tại `IsActive = 1`, `IsPublic = 1`, `IsValid = 0`; không tạo duplicate | Không có |
| `DELETE /api/favorites/{mediaId}` | Có | Có | User chỉ xóa reaction của chính mình | Nếu chưa có favorite thì không crash | Không có |
| `GET /api/favorites/me` | Có | Có | Chỉ lấy favorite của user hiện tại | Map media qua `GetByIdAsync` | `GetByIdAsync` hiện không chặn `IsValid`; cần cân nhắc nếu muốn ẩn media vi phạm khỏi favorite |
| `GET /api/favorites/status/{mediaId}` | Có | Có | Chỉ check status của user hiện tại | Trả bool trạng thái reaction | Không có |

## 5. Play History

Base route: `/api/history`, controller có `[Authorize]`.

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `GET /api/history/recent` | Có | Có | Chỉ lấy history của user hiện tại | Lấy tối đa 10 item gần đây qua repository | Cần đảm bảo rule replay: media cũ lên order 1, trim còn 10 |
| `POST /api/history/{mediaId}` | Có | Có | User chỉ ghi lịch sử cho chính mình | Check media tồn tại; dùng `StoppedAt` theo schema hiện tại | Cần rà lại behavior update/create để đúng rule mới |

## 6. Share

Base route: `/api/shares`, controller có `[Authorize]`.

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `POST /api/shares` | Có | Có | Sender luôn là user hiện tại; không tự share; receiver phải tồn tại active | Normalize `ShareType`; tạo `MediaShares`; tạo notification có `TargetType/TargetId`; push SignalR | Share + notification hiện vẫn qua 2 repository riêng, chưa cùng transaction |
| `POST /api/shares` media/song/video | Có | Có | Chỉ owner media được share | Media phải `OwnerId = SenderId`, `IsActive = 1`, `IsPublic = 1`, `IsValid = 0` | Đúng rule không share private/vi phạm |
| `POST /api/shares` album | Có | Có | Chỉ owner album được share | Album phải `ArtistId = SenderId`, `IsActive = 1`, `IsPublic = 1` | Chưa check album track private/vi phạm |
| `POST /api/shares` playlist | Có | Có | Chỉ owner playlist được share | Playlist phải `UserId = SenderId`, `IsActive = 1` | Theo rule mới của bạn, private playlist owner được share nhưng receiver không share tiếp được; hiện đáp ứng bằng owner check |
| `GET /api/shares/inbox` | Có | Có | Chỉ lấy share có `ReceiverId = CurrentUserId` | Join sender/receiver/item để trả danh sách | Chưa có read/unread vì schema share chưa có cột read |
| `GET /api/shares/sent` | Có | Có | Chỉ lấy share có `SenderId = CurrentUserId` | Join sender/receiver/item để trả danh sách đã gửi | Không có |

## 7. Notification / SignalR

Base route: `/api/notifications`, controller có `[Authorize]`.

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `GET /api/notifications` | Có | Có | Chỉ lấy notification của user hiện tại | Lọc `UserId`, `IsActive = 1`, limit mặc định | DTO có `TargetType/TargetId` để frontend điều hướng |
| `GET /api/notifications/unread` | Có | Có | Chỉ lấy unread của user hiện tại | Lọc `IsRead = 0`, `IsActive = 1` | Không có |
| `GET /api/notifications/unread-count` | Có | Có | Chỉ đếm unread của user hiện tại | Đếm `IsRead = 0`, `IsActive = 1` | Không có |
| `PATCH /api/notifications/{notificationId}/read` | Có | Có | Chỉ mark notification thuộc user hiện tại | Update theo `Id + UserId + IsActive = 1` | Không có |
| `PATCH /api/notifications/read-all` | Có | Có | Chỉ mark toàn bộ notification của user hiện tại | Update unread active notification của user | Không có |
| `DELETE /api/notifications/{notificationId}` | Có | Có | Chỉ receiver/user sở hữu notification được xóa | Soft delete `IsActive = 0` | Không có |

Hub route: `/hubs/notifications`

| Chức năng | Đăng nhập | User từ token | Phân quyền hiện có | Cần xem thêm |
|---|---|---|---|---|
| Connect hub | Có JWT | Có | Program hỗ trợ JWT qua query `access_token` cho `/hubs/notifications` | Frontend phải gửi token khi connect |
| Join user group | Có | Có | Chỉ join group trùng user id trong token | Không có |

## 8. Search

Base route: `/api/search`

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `GET /api/search?keyword=...&page=1&pageSize=10` | Không | Không | Public search | Keyword không được trống; media/playlist/artist search có phân trang; media lọc `IsPublic = 1`, `IsActive = 1`, `IsValid = 0` | Không có |
| `GET /api/search/trending?top=10` | Không | Không | Public discovery | Lấy top media theo `ViewCount`, lọc `IsPublic = 1`, `IsActive = 1`, `IsValid = 0` | Không có |

## 9. User / Follow

Base route: `/api/users`, controller có `[Authorize]` nhưng một số endpoint `[AllowAnonymous]`.

| Endpoint | Đăng nhập | User từ token | Phân quyền hiện có | Kiểm tra nghiệp vụ hiện có | Cần xem thêm |
|---|---|---|---|---|---|
| `GET /api/users/{id}` | Không | Không | Public profile theo id | Trả user DTO, không trả password hash | Không có |
| `GET /api/users/{id}/followers-count` | Không | Không | Public follower count | Đếm follower active | Không có |
| `GET /api/users/me/profile` | Có | Có | Chỉ lấy profile của user hiện tại | Controller lấy user id từ JWT, handler yêu cầu đăng nhập và map sang `UserProfileDto` | Không có |
| `PUT /api/users/me/profile` | Có | Có | Chỉ user hiện tại được cập nhật hồ sơ của chính mình | Controller lấy user id từ JWT, handler kiểm tra ownership lần nữa trước khi update | Không có |
| `PATCH /api/users/{id}/verify-artist` | Có, role `Admin` | Có | Chỉ Admin được xác thực user thành Artist | Có `[Authorize(Roles = "Admin")]` và handler kiểm tra role Admin qua `ICurrentUserContext` | Chưa có workflow listener gửi yêu cầu xác thực trước khi admin duyệt |
| `GET /api/users/artists` | Có | Có trong handler | User đã đăng nhập xem danh sách nghệ sĩ active | Repository lọc `IsArtist = 1 AND IsActive = 1` | Nếu muốn public cho khách chưa đăng nhập thì cần đổi rule handler |
| `GET /api/users/by-handle/{idDisplay}` | Có | Không | User đã đăng nhập tìm user theo handle công khai | Repository lọc `IdDisplay` và `IsActive = 1`, không trả password/email | Không có |
| `POST /api/users/follow` | Có | Có trong handler | Chỉ current user được follow thay chính mình | Không tự follow; check user tồn tại; restore follow soft-deleted nếu có | Không có |
| `DELETE /api/users/unfollow` | Có | Có trong handler | Chỉ current user được unfollow thay chính mình | Soft delete follow | Không có |
| `GET /api/users/{followerId}/is-following/{followeeId}` | Có | Có trong handler | Chỉ current user được check trạng thái của chính `followerId` | Check follow active | Không có |
| `GET /api/users/{id}/following` | Có | Có trong handler | Chỉ user hiện tại xem following của chính mình | Lọc follow active | Nếu muốn public following thì cần đổi rule |
| `GET /api/users/{id}/followers` | Có | Không bắt owner trong controller | Lấy followers của user id truyền vào | Lọc follow active | Cần xác nhận followers public hay private |

## 10. Rule lệch với nghiệp vụ đã chốt

1. Admin không được upload/sửa/xóa media, không tạo/sửa playlist hoặc add track.
   - Code hiện tại media vẫn `[Authorize(Roles = "Artist,Admin")]`.
   - Playlist controller chỉ `[Authorize]`, chưa chặn role Admin.

2. Stream media private.
   - Rule bạn chốt: chỉ owner stream private, media muốn stream cho người khác phải public.
   - Code hiện tại stream `[AllowAnonymous]`, chỉ chặn `IsActive = 1` và `IsValid = 0`.

3. Public detail/list theo artist.
   - `GET /api/media/{id}` và `GET /api/media/artist/{userId}` chưa chặn private/vi phạm đầy đủ.

4. Playlist add track.
   - Chưa enforce max 20.
   - Chưa enforce public playlist chỉ nhận public media.
   - Chưa enforce private playlist được nhận public/private media.
   - Chưa đưa track mới lên order `1` như rule mới.

5. Share + notification transaction.
   - Rule bạn muốn: tách nhưng dùng transaction.
   - Code hiện tại tạo share và notification ở 2 repository call riêng.

6. Search media.
   - Đã thêm `IsValid = 0` vào search/trending media để không hiện media đang bị khóa.

7. Artist verification request.
   - Đã có endpoint Admin xác thực trực tiếp user thành Artist.
   - Chưa có bảng/API request listener -> artist.
   - Chưa có workflow listener gửi yêu cầu rồi admin duyệt request trong code.
