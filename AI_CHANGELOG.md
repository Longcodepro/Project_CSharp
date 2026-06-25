## 001 - Fix ShareController and NotificationController Unauthorized Access

- Mục đích:
  - Chuẩn hóa cách xử lý lỗi 401 Unauthorized trong ShareController và NotificationController.
  - Đảm bảo API trả về `ApiResponse<object?>.Fail` khi người dùng chưa đăng nhập.

- File đã sửa:
  - `backend/src/TuneVault.API/Controllers/ShareController.cs`
  - `backend/src/TuneVault.API/Controllers/NotificationController.cs`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `API_CONTRACT.md`
  - `backend/src/TuneVault.API/Controllers/BaseApiController.cs`
  - `backend/src/TuneVault.Application/Common/ApiResponse.cs`

- Kiểm tra:
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm install`: not run
  - `npm run dev`: not run

- Ghi chú:
  - Đã đảm bảo các endpoint yêu cầu xác thực trả về 401 với format chuẩn.

## 002 - Fix CollectionLikesController 500 Error and Add SQL Schema

- Mục đích:
  - Sửa lỗi 500 Internal Server Error trong CollectionLikesController khi truy vấn `GetRecentCollectionLikesQueryHandler`.
  - Thêm SQL schema cho bảng `CollectionLikes` vào `DATABASE_SCHEMA.md`.

- File đã sửa:
  - `backend/src/TuneVault.API/Controllers/CollectionLikesController.cs`
  - `backend/src/TuneVault.Application/Features/CollectionLike/Queries/GetRecentCollectionLikes/GetRecentCollectionLikesQueryHandler.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/CollectionLikeRepository.cs`
  - `docs/archive/DATABASE_SCHEMA.md`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `API_CONTRACT.md`
  - `backend/src/TuneVault.Application/Abstractions/ICurrentUserService.cs`
  - `backend/src/TuneVault.Domain/Interfaces/ICollectionLikeRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Persistence/IDbConnectionFactory.cs`
  - `backend/src/TuneVault.Domain/Entities/CollectionLike.cs`
  - `backend/src/TuneVault.Domain/Enums/CollectionLikeTargetType.cs`

- Kiểm tra:
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm install`: not run
  - `npm run dev`: not run

- Ghi chú:
  - Đã thêm schema cho bảng `CollectionLikes` và sửa lỗi truy vấn. Cần đảm bảo database được migrate với schema mới.

## 003 - Fix JWT Clock Skew Issue

- Mục đích:
  - Khắc phục lỗi xác thực JWT do lệch thời gian (Clock Skew) bằng cách thêm `ClockSkew = TimeSpan.Zero` vào cấu hình JWT.

- File đã sửa:
  - `backend/src/TuneVault.API/Program.cs`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `API_CONTRACT.md`
  - `backend/src/TuneVault.API/Controllers/AuthController.cs`
  - `backend/src/TuneVault.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
  - `backend/src/TuneVault.Infrastructure/Authentication/JwtTokenGenerator.cs`

- Kiểm tra:
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm install`: not run
  - `npm run dev`: not run

- Ghi chú:
  - Lỗi Clock Skew đã được xử lý, giúp JWT hoạt động ổn định hơn.

## 004 - Remove Redundant TotalFollowers Update Logic

- Mục đích:
  - Loại bỏ logic cập nhật `TotalFollowers` trùng lặp trong `FollowUserCommandHandler.cs` và `UnfollowUserCommandHandler.cs`.
  - Đảm bảo `TotalFollowers` chỉ được cập nhật bởi `FollowRepository.cs` để tránh xung đột và duy trì tính nhất quán.

- File đã sửa:
  - `backend/src/TuneVault.Application/Features/User/Commands/FollowUser/FollowUserCommandHandler.cs`
  - `backend/src/TuneVault.Application/Features/User/Commands/UnfollowUser/UnfollowUserCommandHandler.cs`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `backend/src/TuneVault.Domain/Entities/User.cs`
  - `backend/src/TuneVault.Domain/Interfaces/IUserRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/UserRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/FollowRepository.cs`
  - `backend/src/TuneVault.Application/Features/User/DTOs/UserPublicDetailDto.cs`
  - `backend/src/TuneVault.Application/Features/User/Queries/GetUserById/GetUserByIdQueryHandler.cs`
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/Services/MediaService.tsx`

- Kiểm tra:
  - `dotnet build backend/src/TuneVault.sln`: pending
  - `npm install`: not run
  - `npm run dev`: not run

- Ghi chú:
  - Logic cập nhật `TotalFollowers` đã được đơn giản hóa và tập trung vào `FollowRepository`.

## 005 - Redesign profile, cookie auth refresh, and change-password OTP flow

- Mục đích:
  - Đồng bộ dữ liệu profile owner/public để hiển thị đúng `email`, `follower`, `following`.
  - Chuyển auth sang cookie httpOnly có refresh token để không còn phụ thuộc `localStorage.jwt_token`.
  - Thêm luồng đổi mật khẩu qua OTP và ẩn `PlayerBar` khi chưa có session hoặc chưa có lịch sử nghe nhạc.

- File đã sửa:
  - `backend/src/TuneVault.Application/Features/User/DTOs/UserPublicDetailDto.cs`
  - `backend/src/TuneVault.Application/Features/User/Queries/GetProfile/GetUserProfileQueryHandler.cs`
  - `backend/src/TuneVault.Application/Features/User/Commands/UpdateProfile/UpdateProfileCommandHandler.cs`
  - `backend/src/TuneVault.Application/Features/User/Queries/GetUserById/GetUserByIdQueryHandler.cs`
  - `backend/src/TuneVault.Application/Features/User/Commands/VerifyAsArtist/VerifyAsArtistCommandHandler.cs`
  - `backend/src/TuneVault.Application/Interfaces/IJwtTokenGenerator.cs`
  - `backend/src/TuneVault.Infrastructure/Authentication/JwtTokenGenerator.cs`
  - `backend/src/TuneVault.Application/Features/Auth/DTOs/AuthResponseDto.cs`
  - `backend/src/TuneVault.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`
  - `backend/src/TuneVault.Application/Features/Auth/Commands/Register/RegisterCommand.cs`
  - `backend/src/TuneVault.Application/Features/Auth/Commands/SendOtp/SendOtpCommandHandler.cs`
  - `backend/src/TuneVault.Application/Features/Auth/Commands/ChangePassword/ChangePasswordCommand.cs`
  - `backend/src/TuneVault.Application/Features/Auth/Commands/ChangePassword/ChangePasswordCommandHandler.cs`
  - `backend/src/TuneVault.API/Controllers/AuthController.cs`
  - `backend/src/TuneVault.API/Program.cs`
  - `backend/src/TuneVault.Infrastructure/Services/GmailSmtpEmailService.cs`
  - `frontend/Services/MediaService.tsx`
  - `frontend/src/React/App.jsx`
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/React/Components/Sidebar.jsx`
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/src/React/Components/AuthLoginModal.jsx`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `API_CONTRACT.md`
  - `docs/archive/DESIGN-spotify.md`
  - `docs/archive/ENDPOINT_PERMISSION_AUDIT.md`
  - `docs/archive/DATABASE_SCHEMA.md`

- Kiểm tra:
  - `npm install`: pass
  - `npm run build`: pass
  - `npm run dev`: pass
  - `dotnet build backend/src/TuneVault.sln`: pass

- Ghi chú:
  - `PlayerBar` chỉ hiển thị khi user đã đăng nhập và có track thật, còn trạng thái rỗng sẽ ẩn hoàn toàn.
- Public profile shelf hiển thị nhiều item theo chiều ngang và backend đã trả đủ `email`/`followingCount`.

## 006 - Normalize enum and database constraints

- Mục đích:
  - Đồng bộ enum backend với giá trị DB thực tế.
  - Loại bỏ magic number/string ở luồng friend, share, notification.
  - Bổ sung `CHECK` constraint cho các cột enum-like trong snapshot/migration.

- File đã sửa:
  - `backend/src/TuneVault.Domain/Enums/MediaType.cs`
  - `backend/src/TuneVault.Domain/Enums/AccessLevel.cs`
  - `backend/src/TuneVault.Domain/Enums/ShareType.cs`
  - `backend/src/TuneVault.Domain/Enums/FriendStatus.cs`
  - `backend/src/TuneVault.Domain/Enums/NotificationTargetType.cs`
  - `backend/src/TuneVault.Domain/Entities/Friend.cs`
  - `backend/src/TuneVault.Application/Features/Friend/Abstractions/IFriendRepository.cs`
  - `backend/src/TuneVault.Application/Features/Friend/Commands/AcceptFriendRequest/AcceptFriendRequestCommand.cs`
  - `backend/src/TuneVault.Application/Features/Friend/Commands/CancelFriendRequest/CancelFriendRequestCommand.cs`
  - `backend/src/TuneVault.Application/Features/Friend/Commands/RejectFriendRequest/RejectFriendRequestCommand.cs`
  - `backend/src/TuneVault.Application/Features/Friend/Commands/RemoveFriend/RemoveFriendCommand.cs`
  - `backend/src/TuneVault.Application/Features/Friend/Commands/SendFriendRequest/SendFriendRequestCommand.cs`
  - `backend/src/TuneVault.Application/Features/Notification/Commands/NotificationInsertModel.cs`
  - `backend/src/TuneVault.Application/Features/Share/Commands/ShareMedia/IMediaShareCommandRepository.cs`
  - `backend/src/TuneVault.Application/Features/Share/Commands/ShareMedia/ShareMediaCommandHandler.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/FriendRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/MediaShareRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/NotificationRepository.cs`
  - `backend/src/TuneVault.Infrastructure/Database/schemas/V10_AddEnumCheckConstraints.sql`
  - `Untitled.sql`
  - `docs/archive/DATABASE_SCHEMA.md`
  - `CURRENT_STATUS.md`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `API_CONTRACT.md`
  - `Untitled.sql`
  - `docs/archive/DATABASE_SCHEMA.md`

- Kiểm tra:
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm install`: not run
  - `npm run dev`: not run

- Ghi chú:
  - `Favorites` hiện vẫn là bảng mở rộng cho `Media`, `Album`, `Playlist` qua `TargetId`/`TargetType`.
  - `change_password` đã được chốt lại là giá trị hợp lệ của OTP purpose ở backend và schema constraints.

## 007 - Drop Favorites.MediaItemId and migrate codebase to TargetId

- Mục đích:
  - Xóa hẳn cột `MediaItemId` khỏi bảng `Favorites`.
  - Đồng bộ entity, repository và query lấy favorites sang `TargetId`/`TargetType`.
  - Cung cấp migration SQL để database cũ có thể drop cột an toàn.

- File đã sửa:
  - `backend/src/TuneVault.Domain/Entities/Favorite.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/FavoriteRepository.cs`
  - `backend/src/TuneVault.Application/Features/Favorite/Queries/GetFavorites/GetFavoritesQueryHandler.cs`
  - `backend/src/TuneVault.Infrastructure/Database/schemas/V11_DropFavoritesMediaItemId.sql`

## 008 - Stabilize friend panel avatars and fix App collection import

- Mục đích:
  - Đồng bộ avatar fallback trong panel bạn bè về ảnh mặc định của project thay vì ảnh ngẫu nhiên bên ngoài.
  - Sửa lỗi build frontend do `App.jsx` import một hàm `getMediaCollection` không tồn tại trong `MediaService.tsx`.
  - Dọn luôn các import/render mồ côi của `VideoPlayerView` và `MediaInfoPanel` khỏi `App.jsx` để bundler không còn báo lỗi module.
  - Giữ lại logic phát theo playlist/album bằng cách gọi các API thật `getPlaylistById`, `getAlbumById`, `getTrackById`.

- File đã sửa:
  - `frontend/src/React/Components/FriendActivity.jsx`
  - `frontend/src/React/App.jsx`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `frontend/Services/MediaService.tsx`
  - `frontend/src/React/Components/FriendActivity.jsx`
  - `frontend/src/React/App.jsx`
  - `backend/src/TuneVault.Infrastructure/Repositories/FriendRepository.cs`

- Kiểm tra:
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm install`: not run

- Ghi chú:
  - Luồng accept/reject/cancel của friend panel trong code hiện tại đã đi đúng `requestId` và backend vẫn soft delete `IsActive = 0` cho request bị hủy/từ chối.
  - `Untitled.sql`
  - `docs/archive/DATABASE_SCHEMA.md`
  - `CURRENT_STATUS.md`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `Untitled.sql`
  - `docs/archive/DATABASE_SCHEMA.md`
  - `backend/src/TuneVault.Domain/Entities/Favorite.cs`
  - `backend/src/TuneVault.Infrastructure/Repositories/FavoriteRepository.cs`
  - `backend/src/TuneVault.Application/Features/Favorite/Queries/GetFavorites/GetFavoritesQueryHandler.cs`
  - `backend/src/TuneVault.Infrastructure/Database/schemas/V9_ExtendFavoritesToAlbumPlaylistTargets.sql`

- Kiểm tra:
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm install`: not run
  - `npm run dev`: not run

- Ghi chú:
  - Database cũ cần chạy `V11_DropFavoritesMediaItemId.sql` hoặc thực thi lệnh drop cột tương đương.

## 007 - Fix auth host mismatch and refresh storm

- Mục đích:
  - Sửa lỗi đăng nhập xong bị logout ngay do frontend gọi API qua `127.0.0.1` trong khi app chạy ở `localhost`.
  - Tránh vòng lặp `401 -> refresh -> 401` khi nhiều request auth bắn cùng lúc.

- File đã sửa:
  - `frontend/Services/MediaService.tsx`
  - `frontend/.env.development`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `frontend/Services/MediaService.tsx`
  - `backend/src/TuneVault.API/Controllers/AuthController.cs`
  - `backend/src/TuneVault.API/Program.cs`
  - `frontend/src/React/Components/AuthLoginModal.jsx`

- Kiểm tra:
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm run dev`: not run trong lần sửa này

- Ghi chú:
  - Cookie auth giờ sẽ đi cùng request khi frontend chạy ở `localhost:5173`.
  - Luồng refresh đã được khóa single-flight để giảm request refresh chồng nhau.

## 008 - Stop false logout on secondary 403/401

- Mục đích:
  - Ngăn frontend tự xóa session khi một request phụ trả về `403` hoặc một `401` đơn lẻ từ endpoint không phải auth.
  - Giữ session ổn định khi quay từ profile về home, tránh logout oan do data fetch phía sidebar/home.

- File đã sửa:
  - `frontend/Services/MediaService.tsx`

- File đã đọc/tham khảo:
  - `frontend/Services/MediaService.tsx`
  - `frontend/src/React/App.jsx`
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/React/Components/ProfileView.jsx`

- Kiểm tra:
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: not run
  - `npm run dev`: not run

- Ghi chú:
  - Session chỉ còn bị clear bởi luồng refresh/auth thật sự, thay vì một request phụ làm rụng toàn bộ đăng nhập.

## 009 - Fix profile bio persistence and follower counts

- Mục đích:
  - Sửa lỗi cập nhật bio không lưu xuống database dù API không báo lỗi.
  - Sửa số follower trên profile cá nhân và public profile để đọc từ quan hệ follow active.

- File đã sửa:
  - `backend/src/TuneVault.Infrastructure/Repositories/UserRepository.cs`
  - `backend/src/TuneVault.Application/Features/User/Commands/UpdateProfile/UpdateProfileCommandHandler.cs`
  - `backend/src/TuneVault.Application/Features/User/Queries/GetProfile/GetUserProfileQueryHandler.cs`
  - `backend/src/TuneVault.Application/Features/User/Queries/GetUserById/GetUserByIdQueryHandler.cs`

- File đã đọc/tham khảo:
  - `backend/src/TuneVault.Domain/Entities/User.cs`
  - `backend/src/TuneVault.Domain/Interfaces/IUserRepository.cs`
  - `backend/src/TuneVault.API/Controllers/UsersController.cs`
  - `backend/src/TuneVault.API/DTOs/Users/UpdateProfileFormRequestDto.cs`
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/Services/MediaService.tsx`

- Kiểm tra:
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm run build`: not run
  - `npm run dev`: not run

- Ghi chú:
  - `Users.Bio` trước đó không nằm trong câu `UPDATE Users`, nên đổi tên lưu được còn bio thì mất sau khi đọc lại.
  - `TotalFollowers` trả về profile hiện được đếm bằng `GetFollowersAsync(...)` thay vì dùng field `Users.TotalFollowers`.

## 010 - Match public profile resources to home content cards

- Mục đích:
  - Thiết kế lại phần tài nguyên do profile tạo theo kiểu card/section của tab `Tất cả` ở trang chủ.
  - Giữ dữ liệu chỉ thuộc profile đang xem và không render danh sách nghệ sĩ ở cuối.

- File đã sửa:
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/CSS/Home.css`

- File đã đọc/tham khảo:
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/CSS/Home.css`

- Kiểm tra:
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: not run
  - `npm run dev`: not run

- Ghi chú:
  - Mỗi section profile dùng `content-section`, `content-row-scroll`, `content-card`, `content-cover` giống trang chủ.
  - Album/playlist trong profile đã được nối lại `onOpenCollection` từ `Home`.

## 011 - Fix friend workflow actions and profile open

- Mục đích:
  - Sửa luồng accept/reject/cancel/remove friend trên panel bạn bè.
  - Cho phép click avatar hoặc tên trong panel bạn bè để mở public profile vào body content chính.

- File đã sửa:
  - `frontend/src/React/Components/FriendActivity.jsx`
  - `frontend/src/React/App.jsx`
  - `frontend/src/React/Components/ProfileView.jsx`
  - `frontend/src/CSS/FriendActivity.css`

- File đã đọc/tham khảo:
  - `PLANS/011-friend-workflow-and-profile-open.md`
  - `API_CONTRACT.md`
  - `docs/archive/ENDPOINT_PERMISSION_AUDIT.md`
  - `backend/src/TuneVault.API/Controllers/FriendsController.cs`
  - `backend/src/TuneVault.Application/Features/Friend/Commands/*`
  - `backend/src/TuneVault.Infrastructure/Repositories/FriendRepository.cs`

- Kiểm tra:
  - `npm run build`: pass
  - `dotnet build backend/src/TuneVault.sln`: pass
  - `npm run dev`: not run

- Ghi chú:
  - `FriendActivity` đã map cả camelCase và PascalCase cho `requestId`, `userId`, `idDisplay`, `displayName`, `avatarUrl`.
  - Action button được khóa khi đang xử lý để tránh double submit.
  - `ProfileView` ưu tiên `profileTarget.id` khi mở profile từ panel bạn bè để tránh mở nhầm kết quả search.

## 012 - PlayerBar reduction and interaction overhaul

- Mục đích:
  - Giữ lại trên thanh play những control cần thiết, bỏ `shuffle` và `repeat`.
  - Hoàn thiện hành vi từng nút: pause dừng phát thật, next/previous duyệt đúng queue, volume sync xuống audio element.
  - Thêm nút xem video (mở VideoPlayerView), nút thông tin media (mở MediaInfoPanel), nút `+` add vào playlist (mở PlaylistModal).
  - Thêm nút favorite heart với reaction picker hover, gọi endpoint lưu DB.
  - Xử lý edge case: media video không có audio fallback thì không cho phát.

- File đã sửa:
  - `frontend/src/React/Components/PlayerBar.jsx`
  - `frontend/src/React/App.jsx`
  - `frontend/Services/MediaService.tsx`
  - `frontend/src/React/Components/NowPlayingView.jsx`
  - `frontend/src/React/Components/PlaylistModal.jsx`
  - `frontend/src/CSS/PlaylistModal.css`
  - `frontend/src/React/Components/VideoPlayerView.jsx`
  - `frontend/src/CSS/VideoPlayerView.css`
  - `frontend/src/React/Components/MediaInfoPanel.jsx`
  - `frontend/src/CSS/MediaInfoPanel.css`
  - `frontend/src/CSS/PlayerBar.css`
  - `frontend/src/CSS/App_style.css`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `API_CONTRACT.md`
  - `PLANS/007-playerbar-reduction-and-interactions.md`
  - `docs/archive/DESIGN-spotify.md`
  - `docs/archive/ENDPOINT_PERMISSION_AUDIT.md`

- Kiểm tra:
  - `npm install`: not run
  - `npm run build`: pending
  - `npm run dev`: pending
  - `dotnet build backend/src/TuneVault.sln`: pending

- Ghi chú:
  - Queue mặc định cho player: nếu phát từ media item đơn lẻ, tạo queue theo list media item hợp lệ; nếu phát từ album/playlist, giữ queue theo thứ tự track của collection.
  - Shuffle chỉ áp dụng cho media item ngoài album/playlist bằng queue phát ngẫu nhiên có thứ tự.
  - Video view khi đóng vẫn tiếp tục phát audio nếu track đang phát.
  - Favorite reaction picker hover hiện các reaction từ enum và gọi endpoint lưu DB.
  - Đã kiểm tra backend endpoints trong API_CONTRACT.md: playlist owned list, favorite reactions đều đã có.
  - Edge case video không audio đã được xử lý bằng explicit check trong playMediaItem.
  - `PlayerBar` chỉ hiển thị khi user đã đăng nhập và có track thật.

## 013 - Fix App entrypoint TypeScript and build errors

- Mục đích:
  - Sửa các lỗi TypeScript/build trong `App.tsx` và chuyển frontend entrypoint sang file `.tsx` đã được typed đúng.

- File đã sửa:
  - `frontend/src/React/App.tsx`
  - `frontend/src/main.jsx`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `frontend/Services/MediaService.tsx`
  - `frontend/src/React/App.jsx`

- Kiểm tra:
  - `npm install`: pass
  - `npm run build`: pass
  - `npm run dev`: pass
  - `dotnet build`: not run

- Ghi chú:
  - `main.jsx` đã chuyển sang import `App.tsx` để tránh build đọc nhầm `App.jsx` còn sót syntax TypeScript.
  - `App.tsx` đã được gắn type cho state/callback chính, sửa `handleToggleFavorite`, và chuẩn hóa player queue/profile flow.

## 014 - Tighten remaining App.tsx prop and service casts

- Mục đích:
  - Xử lý các lỗi TypeScript còn sót do service trả về `unknown` và do các component `.jsx` có default prop type quá hẹp.

- File đã sửa:
  - `frontend/src/React/App.tsx`

- File đã đọc/tham khảo:
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/React/Components/Sidebar.jsx`
  - `frontend/src/React/Components/PlayerBar.jsx`
  - `frontend/src/React/Components/VideoPlayerView.jsx`
  - `frontend/src/React/Components/MediaInfoPanel.jsx`
  - `frontend/src/React/Components/PlaylistModal.jsx`

- Kiểm tra:
  - `npm run build`: pass

- Ghi chú:
  - `App.tsx` now wraps legacy `.jsx` components with `ComponentType<any>` aliases to avoid incorrect `null`/`never[]` prop narrowing from default values.
  - `getMyProfile()` và `getMyPlaylists()` được cast về shape mong muốn trước khi set state.

## 015 - Fix header and artist avatar fallback

- Mục đích:
  - Sửa luồng render avatar ở header và artist card để không còn rơi vào ảnh vỡ khi backend trả field khác casing hoặc URL gốc lỗi.

- File đã sửa:
  - `frontend/src/React/Components/Home.jsx`
  - `frontend/src/React/App.tsx`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `API_CONTRACT.md`
  - `backend/src/TuneVault.API/Controllers/UsersController.cs`
  - `backend/src/TuneVault.API/Program.cs`
  - `backend/src/TuneVault.Application/Features/User/DTOs/UserDto.cs`
  - `backend/src/TuneVault.Application/Features/User/DTOs/UserProfileDto.cs`
  - `backend/src/TuneVault.Application/Features/User/Queries/GetAllArtists/GetAllArtistsQueryHandler.cs`
  - `backend/src/TuneVault.Application/Features/User/Queries/GetProfile/GetUserProfileQueryHandler.cs`
  - `frontend/Services/MediaService.tsx`

- Kiểm tra:
  - `npm install`: not run
  - `npm run build`: pass
  - `npm run dev`: pass
  - `dotnet build`: not run

- Ghi chú:
  - `Home.jsx` now accepts `AvatarUrl`/`AvatarPath`-style fields for artist cards and uses a local SVG fallback when the backend/default image cannot load.
  - `App.tsx` now normalizes more avatar field variants from `/api/users/me/profile` before setting the header avatar state.

## 016 - Package shareable Docker zip with wwwroot uploads

- Mục đích:
  - Đóng gói một bản chia sẻ có thể giải nén và chạy bằng Docker, dùng đúng cấu trúc hiện tại `backend/` + `frontend/` và giữ dữ liệu trong `wwwroot/uploads`.

- File đã sửa:
  - `docker-compose.yml`

- File đã đọc/tham khảo:
  - `backend/src/TuneVault.API/Dockerfile`
  - `frontend/Dockerfile`
  - `backend/src/TuneVault.Infrastructure/Services/LocalFileStorageService.cs`
  - `backend/.env.example`

- Kiểm tra:
  - `zip /tmp/TuneVault_with_wwwroot_data.zip`: pass
  - `zip` content check: pass

- Ghi chú:
  - Gói share cuối cùng nằm ở `/tmp/TuneVault_with_wwwroot_data.zip`.
  - Gói không chứa `backend/.env` thật và không chứa `node_modules`/`bin`/`obj`/`dist`.
  - `docker-compose.yml` đã chuyển sang build từ `./backend/src` và `./frontend`, đồng thời mount trực tiếp `backend/src/TuneVault.API/wwwroot/uploads`.

## 017 - Create branch push split plan

- Mục đích:
  - Tạo file kế hoạch để chia code theo từng nhánh và từng lượt commit trước khi push lên remote.
  - Giúp tách rõ scope giữa `CleanArchitecture`, auth/user, album/playlist/search, interaction/share/notification và frontend.

- File đã sửa:
  - `PLANS/012-branch-push-split-plan.md`

- File đã đọc/tham khảo:
  - `AGENTS.md`
  - `CURRENT_STATUS.md`
  - `RUN_LOCAL.md`
  - `PLANS/README.md`
  - `PLANS/MASTER_PLAN.md`
  - `PLANS/011-friend-workflow-and-profile-open.md`
  - `PLANS/004-profile-auth-player-redesign.md`

- Kiểm tra:
  - `npm install`: not run
  - `npm run dev`: not run
  - `dotnet build`: not run

- Ghi chú:
  - Đây là file kế hoạch vận hành, không thay đổi logic code.

## 018 - Add team-ready branch push guide

- Mục đích:
  - Tạo một bản rút gọn, dễ gửi cho cả nhóm, gom lại nhánh, scope file, commit batches và checklist push.

- File đã sửa:
  - `PLANS/013-team-branch-push-guide.md`

- File đã đọc/tham khảo:
  - `PLANS/012-branch-push-split-plan.md`

- Kiểm tra:
  - `npm install`: not run
  - `npm run dev`: not run
  - `dotnet build`: not run

- Ghi chú:
  - Đây là bản ngắn hơn để share cho team, còn bản `012` giữ nội dung chi tiết hơn.

## 019 - Expand team guide with commit file map

- Mục đích:
  - Bổ sung bản hướng dẫn theo thứ tự từng nhánh, từng commit, và danh sách file/folder cần push trong commit đó.

- File đã sửa:
  - `PLANS/013-team-branch-push-guide.md`

- File đã đọc/tham khảo:
  - `PLANS/012-branch-push-split-plan.md`
  - `PLANS/013-team-branch-push-guide.md`

- Kiểm tra:
  - `npm install`: not run
  - `npm run dev`: not run
  - `dotnet build`: not run

- Ghi chú:
  - Bản `013` hiện là bản phù hợp để gửi cho mọi người vì có cả commit order và file map.

## 020 - Add Windows local run guide

- Mục đích:
  - Tạo tài liệu hướng dẫn chạy TuneVault local trên Windows bằng PowerShell hoặc CMD.

- File đã sửa:
  - `RUN_LOCAL_WINDOWS.md`

- File đã đọc/tham khảo:
  - `RUN_LOCAL.md`

- Kiểm tra:
  - `npm install`: not run
  - `npm run dev`: not run
  - `dotnet build`: not run

- Ghi chú:
  - Tài liệu này là bản tương đương `RUN_LOCAL.md` nhưng đổi cú pháp môi trường sang Windows.

## 021 - Clarify branch count in team guide

- Mục đích:
  - Làm rõ rằng danh sách `git branch -a` có cả remote-tracking refs, nên plan chỉ cần bám theo 5 nhánh chức năng thật.

- File đã sửa:
  - `PLANS/013-team-branch-push-guide.md`

- File đã đọc/tham khảo:
  - `git branch -a`

- Kiểm tra:
  - `npm install`: not run
  - `npm run dev`: not run
  - `dotnet build`: not run

- Ghi chú:
  - Đã thêm mục giải thích `remotes/origin/...` không phải branch riêng để tránh hiểu nhầm cho team.
