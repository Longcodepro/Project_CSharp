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
- Auth module cơ bản:
  - login/register/reset password.
  - cookie-based access/refresh session.
  - đổi mật khẩu bằng OTP cho user đang đăng nhập.

### Chưa hoàn thành hoặc cần làm tiếp

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

## 6. Cập nhật gần nhất

- Public profile nghệ sĩ đã giữ bio hiển thị đầy đủ, chỉ khóa chỉnh sửa.
- Auth hiện đã chuyển sang cookie httpOnly:
  - Access token đọc từ cookie `tunevault_access_token`.
  - Refresh token dùng `/api/auth/refresh`.
  - Frontend không còn lưu JWT trong `localStorage`.
- Nút follow/hủy follow trên profile public đã được nối lại theo auth state thật từ App thay vì tự khóa sai ở component.
- Màn `Hồ sơ` hiện có 2 chế độ:
  - `self`: avatar, stats, name/id/email, bio chỉnh sửa và nút xác nhận trước khi lưu.
  - `public`: bấm vào artist card ở Home sẽ mở cùng màn profile nhưng chỉ xem, có nút `Follow` / `Hủy follow`.
- Màn profile public và owner đã được đổi layout lại để:
  - owner: avatar + name + idname + bio editable, còn follower/following/email chỉ đọc.
  - public: email, follower/following hiển thị đúng dữ liệu, có follow button và các shelf nội dung tạo bởi user.
- `PlayerBar` chỉ hiện khi đang đăng nhập và đang có track thật, không hiện ở trạng thái chưa có lịch sử.
- Artist card ở Home đã gắn click để mở profile nghệ sĩ read-only.
- Public profile artist giờ lấy thêm `createdAt` và `totalFollowers` để hiển thị stats đồng bộ với layout profile.
- Màn `Hồ sơ` đã được chốt lại theo layout cuối:
  - Avatar ở đầu trang.
  - Một hàng stats gồm follower, đang follow, ngày bắt đầu.
  - Tên, idname và email hiển thị dạng chỉ đọc.
  - Bio chiếm nguyên phần dưới và có nút bút để bật/tắt chỉnh sửa.
  - Nút lưu cuối cùng mở hộp xác nhận trước khi cập nhập.
- `GET /api/users/me/profile` và response cập nhật profile đã trả thêm `email` để frontend chỉ hiển thị, không cho sửa.
- `đang follow` trên profile được đếm từ endpoint `GET /api/users/{id}/following`.
- Màn `Hồ sơ` đã được tinh gọn theo yêu cầu mới:
  - Avatar được làm lớn hơn và camera icon mở thẳng file explorer để chọn ảnh local.
  - Chỉ còn một nút `Cập nhập` để lưu thay đổi.
  - Upload avatar bị chặn nếu file không phải ảnh.
  - Cột thông tin bên phải vẫn được giữ nguyên để người dùng xem.
- Đã sửa lỗi màn hình đen khi mở modal đăng nhập từ header.
- Đã sửa import `MediaService.tsx` sai đường dẫn ở các component React trong `frontend/src/React/Components`.
- `AuthLoginModal` đã được đưa về đúng thứ tự hooks để tránh crash runtime.
- `npm install`: pass
- `npm run build`: pass
- `npm run dev`: pass, dev server đang chạy tại `http://127.0.0.1:5174/`
- `npm run lint`: fail do môi trường đang dùng ESLint global 6.4.0 thay vì binary của project
- `node ./node_modules/eslint/bin/eslint.js .`: warning only, còn 1 warning `react-hooks/exhaustive-deps` trong `frontend/src/React/App.jsx`
- OTP inputs trong modal đăng ký đã được chỉnh về hàng ngang, mỗi ô cố định `48x56px`.
- Nút OTP trong bước xác nhận sẽ đổi sang `Gửi mã mới` khi hết 1:30 và gửi lại OTP từ cùng nút đó.
- Panel `Friends` và `Notifications` trên frontend đã được nối với backend thật:
  - `Friends` dùng `GET /api/friends/me` và `POST /api/friends/requests/{receiverId}`.
  - `Notifications` dùng `GET /api/notifications`, `GET /api/notifications/unread`, `PATCH /api/notifications/{notificationId}/read`, `PATCH /api/notifications/read-all`.
- Friends/Notifications đã được hoàn thiện các action nhỏ:
  - Friends có tab bạn bè, lời mời nhận, lời mời đã gửi; hỗ trợ chấp nhận, từ chối, hủy, xóa bạn và tìm user theo `UserName`/`DisplayName`.
  - Notifications có badge unread count, mark all read, mark từng notification đã đọc và xóa notification.
- Media cards/player đã được sửa để nhận đúng asset URL do backend trả về:
  - Poster/cover ảnh từ `/api/media/{id}/poster` render được thay vì rơi về default cover.
  - Media mapping trong frontend đọc thêm cả camelCase lẫn PascalCase để giảm lỗi hụt dữ liệu.
- Seed data và upload convention đã được đồng bộ:
  - Audio files lưu vào `wwwroot/uploads/media`.
  - Seed `V5_SeedData.sql` trỏ đúng các file thật đang có trong repo.
- Stream endpoint đã có fallback legacy path:
  - Nếu DB còn path cũ có thêm folder trung gian, backend sẽ thử basename trong folder chuẩn trước khi báo 404.
- Poster endpoint đã có fallback theo `stem`:
  - Nếu DB và file thật lệch `.jpg`/`.png`, backend sẽ thử các đuôi hợp lệ khác trong folder chuẩn.
- `npm run build`: pass
- `npm run dev`: pass, dev server lên ở `http://localhost:5174/`
- `dotnet build backend/src/TuneVault.sln`: pass
- Auth hotfix gần nhất:
  - Frontend trước đó đang trỏ API sang `http://127.0.0.1:5128/api` trong khi UI chạy ở `http://localhost:5173`, làm cookie `tunevault_access_token` và `tunevault_refresh_token` không được gửi đúng site.
  - `MediaService.tsx` đã normalize host loopback về host của browser và khóa refresh thành một luồng duy nhất để tránh `401` lặp.
  - Dev env đã đổi về `http://localhost:5128/api` để đồng bộ với frontend local.
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: pass
- Auth hardening bổ sung:
  - Frontend không còn tự clear session khi một request phụ trả về `403`, và cũng không clear ngay trên `401` của endpoint thường.
  - Mục tiêu là tránh logout oan khi quay về home mà sidebar/load phụ có một request bị từ chối.
  - `npm run build`: pass
- Màn `Quản lý media / album / playlist` đã thay thế màn tạo demo:
  - Media tạo/chỉnh sửa dùng file local thật, tự đọc duration từ file, hỗ trợ `datetime-local`, `isPublic`, và cập nhật luôn file audio/video khi edit.
  - Album/playlist có danh sách track, thêm/xóa/đổi thứ tự ngay trong giao diện quản lý.
- Track reorder của album/playlist đã chuyển sang kiểu kéo-thả trực quan:
  - Bỏ nút lên/xuống và ô chọn vị trí.
  - Chỉ giữ nút xóa track và nút thêm track.
- Sidebar trái đã ngừng dùng dữ liệu mẫu:
  - Nguồn hiển thị là endpoint thật từ backend.
  - Item media thiếu cover sẽ fallback sang poster endpoint thay vì để ảnh trống.
- Poster resolver backend đã được bổ sung fallback cho file ảnh không có đuôi:
  - Tránh lỗi một số cover file thật nằm trong `wwwroot/uploads/media-covers` nhưng DB lưu path cũ/lệch extension.
- Header avatar hiện được đồng bộ từ `GET /api/users/me/profile`:
  - Không còn phụ thuộc vào cache `localStorage.user_avatar` cũ.
  - Chỉ rơi về avatar mặc định khi profile trả `null` hoặc không tải được avatar.
- Màn `Quản lý media / album / playlist` đã được refresh giao diện:
  - Có hero section, tab bar gọn hơn, và bố cục dễ nhìn hơn cho luồng tạo/chỉnh sửa.
- Màn `Hồ sơ` đã được thêm thành body mode riêng:
  - Avatar nằm ở giữa phần trên cùng.
  - Có form chỉnh `displayName`, `bio`, avatar file, xóa avatar.
  - Có cảnh báo khi thoát màn hình nếu còn thay đổi chưa lưu.
- Màn `Hồ sơ` đã được tinh chỉnh lại theo yêu cầu mới:
  - `displayName` và `idDisplay` đều có thể chỉnh sửa, mỗi ô có nút bút nhỏ để focus nhanh.
  - Nút `Cập nhập` duy nhất sẽ lưu toàn bộ thay đổi cùng lúc.
  - Layout được chia thành cụm thông tin chính và cụm stats để cân bố cục hơn.
  - Nút follow ở profile public không còn tự khóa sai khi người dùng đã đăng nhập.
  - Sau khi follow/unfollow, frontend re-fetch lại profile public để số follower cập nhật từ backend thật.
  - Profile cá nhân lấy thêm số `đang follow` từ endpoint `GET /api/users/{id}/following` để hiển thị đúng.
  - Email profile cá nhân chỉ hiển thị, không thêm ghi chú phụ trên giao diện.
- Màn `Hồ sơ` đã được redesign lại theo 2 mode rõ ràng:
  - `Edit Profile`: card chia 3 vùng logic gồm ảnh đại diện, thông tin cơ bản và thống kê.
  - `Public Profile`: bố cục kiểu hero-card với avatar → tên → username → follow → bio → stats.
  - Typography và spacing được tăng độ thoáng để đọc dễ hơn trên desktop và tablet.
  - Bản mới đã chuyển `Edit Profile` sang layout dọc: avatar ở giữa, form bên dưới, stats nằm ngang, bio riêng phía dưới.
  - Public profile đã bỏ toàn bộ text hướng dẫn chỉnh sửa, giữ đúng thứ tự avatar → tên → username → follow → stats → bio.
- Màn `Hồ sơ` đã được chốt theo layout sạch hơn:
  - Owner view dùng avatar centered cỡ lớn, nút `Thay ảnh`, form `displayName` / `idDisplay` / email, stats ngang, bio textarea có counter.
  - Public view chỉ hiển thị thông tin, không có helper text chỉnh sửa, nút follow nổi bật hơn.
  - Bio được nâng trần lên 500 ký tự để khớp UI counter và validate backend.
- Bộ chọn file trong màn quản lý nội dung đã được nới lỏng:
  - Ảnh dùng `image/*`.
  - Video dùng `video/*`.
  - Audio dùng `audio/*`.
  - Backend vẫn tự validate định dạng cuối cùng khi upload.
- Frontend sweep gần nhất đã ổn định thêm các điểm sau:
  - `ProfileView` không còn lỗi biến vượt scope khi mở hồ sơ public và đã nạp media public thật.
  - `Home` đổi nhãn `Podcard` thành `Podcast` để đúng thuật ngữ hơn.
  - `FriendActivity` chỉ render tab đang chọn thay vì hiển thị toàn bộ tab cùng lúc.
  - `Sidebar` và `NotificationActivity` đã chịu được payload enum kiểu số và fallback ảnh tốt hơn.
  - `npm run build`: pass
  - `npm run dev`: pass, dev server lên ở `http://127.0.0.1:5175/` vì `5174` đang bị chiếm
  - `node ./node_modules/eslint/bin/eslint.js .`: pass với 2 warning `react-hooks/exhaustive-deps` còn lại ở `App.jsx` và `ManageStudio.jsx`
- Đợt audit enum/database gần nhất đã được chuẩn hóa thêm:
  - `MediaType`, `AccessLevel`, `ShareType`, `FriendStatus` đã được explicit value/namespace rõ ràng.
  - `NotificationInsertModel` và luồng share/notification đã chuyển sang dùng enum nội bộ thay vì magic number.
  - `Untitled.sql` và migration mới đã bổ sung `CHECK` constraint cho các cột enum-like chính.
  - `dotnet build backend/src/TuneVault.sln`: pass
- Favorites đã bỏ cột `MediaItemId` khỏi schema hiện tại:
  - `Favorite` entity chỉ còn `TargetId` + `TargetType`.
  - `FavoriteRepository` insert/select theo `TargetId`, không còn phụ thuộc cột `MediaItemId`.
  - `GetFavoritesQueryHandler` đọc media theo `favorite.TargetId`.
  - Đã thêm migration `V11_DropFavoritesMediaItemId.sql` để drop cột trên database cũ.
- Profile hotfix gần nhất:
  - `UserRepository.UpdateAsync` đã update thêm cột `Bio`, sửa lỗi bio đổi trên UI nhưng không lưu vào database.
  - Profile owner và public profile đã đếm follower từ bảng `Follows` active thay vì đọc `Users.TotalFollowers`.
  - `dotnet build backend/src/TuneVault.sln`: pass
- Profile public resource layout:
  - Các hàng bài hát, video, album, playlist của profile public đã dùng lại style card/section của tab `Tất cả` ở trang chủ.
  - Mỗi hàng hiển thị 4 item theo chiều ngang và cuộn ngang khi nhiều hơn 4.
  - Không render thêm danh sách nghệ sĩ ở cuối profile public.
  - `npm run build`: pass
- Friend workflow:
  - Panel bạn bè đã chuẩn hóa mapping `requestId`/`userId` để accept, reject, cancel, remove gọi đúng endpoint.
  - Action button được khóa khi đang xử lý để tránh bấm lặp.
  - Click avatar hoặc tên trong panel bạn bè sẽ mở public profile vào body content và đóng panel.
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: pass
- Friend panel / App build hotfix:
  - Avatar rỗng trong panel bạn bè đã dùng ảnh mặc định của project thay vì `pravatar`.
  - `App.jsx` đã bỏ import giả `getMediaCollection` và chuyển sang `getPlaylistById` / `getAlbumById` / `getTrackById` để queue phát nhạc theo collection.
  - Các import/render mồ côi của `VideoPlayerView` và `MediaInfoPanel` đã được gỡ khỏi `App.jsx` để frontend build xanh trở lại.
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: pass
- Frontend entrypoint hotfix:
  - `frontend/src/main.jsx` đã chuyển sang import `App.tsx` thay vì `App.jsx` để build không dính syntax TypeScript còn sót trong file JSX cũ.
  - `App.tsx` đã được type lại cho state/callback chính của player, profile và favorite flow.
  - `npm install`: pass
  - `npm run build`: pass
  - `npm run dev`: pass, dev server lên ở `http://127.0.0.1:5176/`
- App.tsx prop-type cleanup:
  - `App.tsx` đã bọc các component `.jsx` bằng alias `ComponentType<any>` để tránh TS suy luận sai từ default props `null` / `never[]`.
  - `getMyProfile()` và `getMyPlaylists()` được cast trước khi set state để chặn lỗi `unknown` từ service layer.
  - `npm run build`: pass
- Avatar hotfix gần nhất:
  - `Home.jsx` đã mở rộng mapper artist để đọc thêm các field avatar khác casing như `AvatarUrl`, `AvatarPath`, `ImageUrl`.
  - Header avatar và artist card có fallback SVG local nếu file avatar mặc định hoặc URL backend không tải được.
  - `App.tsx` đã normalize thêm các biến thể field avatar từ `GET /api/users/me/profile`.
  - `npm run build`: pass
  - `npm run dev`: pass, dev server lên ở `http://127.0.0.1:5174/`
- Shareable Docker package gần nhất:
  - `docker-compose.yml` đã được cập nhật sang cấu trúc hiện tại `backend/` + `frontend/`.
  - Docker backend mount trực tiếp `backend/src/TuneVault.API/wwwroot/uploads` để giữ dữ liệu avatar/media khi chia sẻ.
  - Gói zip sạch phục vụ chia sẻ nằm tại `/tmp/TuneVault_with_wwwroot_data.zip`.
