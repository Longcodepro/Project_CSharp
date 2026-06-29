TRƯỜNG ĐẠI HỌC SÀI GÒN

KHOA CÔNG NGHỆ THÔNG TIN

BÁO CÁO CÁ NHÂN ĐỒ ÁN

ĐỀ TÀI: TuneVault - Media Streaming Web Application

Môn học: C# and .NET Development

Sinh viên thực hiện: Nguyễn Thành Long

MSSV: 3124410195

Nhóm: Nhóm 7

Giảng viên hướng dẫn: Từ Lãng Phiêu

TP. Hồ Chí Minh, năm 2026

<!-- PAGEBREAK -->

# 1. Giới thiệu phần việc cá nhân

Em tham gia project TuneVault với vai trò phát triển các chức năng nền tảng của hệ thống media streaming. Phần em tập trung thực hiện gồm 5 chức năng đầu trong yêu cầu đồ án: xác thực người dùng, hồ sơ người dùng, thư viện media, audio player, video player; đồng thời xử lý phần xác thực và phân quyền bằng JWT.

Báo cáo này chỉ trình bày các phần em đã trực tiếp tham gia thực hiện, bám theo mã nguồn hiện có trong repo. Nội dung tập trung vào luồng xử lý chính, endpoint tiêu biểu và các file liên quan để thuận tiện cho việc đối chiếu khi chấm đồ án.

# 2. Tổng quan kỹ thuật phần em làm

Backend của TuneVault được xây dựng bằng ASP.NET Core Web API và tổ chức theo Clean Architecture gồm 4 lớp `API`, `Application`, `Domain`, `Infrastructure`. Dữ liệu được lưu trong SQL Server, truy cập qua Dapper thay vì Entity Framework để chủ động viết SQL và kiểm soát truy vấn.

Phần xác thực sử dụng JWT. Frontend được xây dựng bằng React/Vite và gọi API thông qua file `frontend/Services/MediaService.tsx`. File media vật lý được lưu dưới `backend/src/TuneVault.API/wwwroot/uploads`, còn metadata như tiêu đề, loại media, owner, thời lượng, trạng thái public được lưu trong SQL Server.

Luồng tổng quát của phần em làm:

Frontend React/Vite -> Controller ASP.NET Core -> Command/Query Handler trong Application -> Repository Dapper trong Infrastructure -> SQL Server hoặc `wwwroot/uploads` -> trả `ApiResponse` về frontend.

## 2.1. Bảng tóm tắt endpoint chính

| Nhóm chức năng | Endpoint | Mục đích |
| --- | --- | --- |
| Auth | `POST /api/auth/login` | Đăng nhập và nhận access token, refresh token |
| Auth | `POST /api/auth/register` | Đăng ký tài khoản mới sau khi xác minh OTP |
| Auth | `POST /api/auth/send-otp` | Gửi OTP cho đăng ký hoặc đổi mật khẩu |
| Auth | `POST /api/auth/change-password` | Đổi mật khẩu cho tài khoản đang đăng nhập |
| Auth | `POST /api/auth/refresh` | Gia hạn phiên đăng nhập |
| Auth | `POST /api/auth/logout` | Đăng xuất và xóa cookie xác thực |
| User/Profile | `GET /api/users/me/profile` | Lấy hồ sơ của người dùng hiện tại |
| User/Profile | `PUT /api/users/me/profile` | Cập nhật hồ sơ, bio, avatar |
| Media | `GET /api/media` | Lấy danh sách media công khai |
| Media | `GET /api/media/{id}` | Lấy chi tiết một media |
| Media | `GET /api/media/my-media` | Lấy media của người dùng hiện tại |
| Media | `POST /api/media/upload` | Upload media tổng quát |
| Media | `POST /api/media/upload/audio` | Upload media loại audio |
| Media | `POST /api/media/upload/video` | Upload media loại video |
| Audio | `GET /api/media/{id}/audio/stream` | Stream audio có xác thực |
| Video | `GET /api/media/{id}/video/stream` | Stream video có xác thực |
| History | `GET /api/history/recent` | Lấy lịch sử phát gần đây |
| History | `POST /api/history/{mediaId}` | Ghi nhận một lượt phát |
| History | `PATCH /api/history/{mediaId}/stop` | Lưu vị trí dừng phát |
| History | `GET /api/history/{mediaId}/resume` | Lấy vị trí phát tiếp |

## 2.2. Bảng file code liên quan tiêu biểu

| Nhóm chức năng | File liên quan tiêu biểu |
| --- | --- |
| Auth/JWT | `backend/src/TuneVault.API/Controllers/AuthController.cs`, `backend/src/TuneVault.API/Program.cs`, `backend/src/TuneVault.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs`, `backend/src/TuneVault.Application/Features/Auth/Commands/Register/RegisterCommand.cs`, `backend/src/TuneVault.Infrastructure/Authentication/JwtTokenGenerator.cs`, `frontend/Services/MediaService.tsx`, `frontend/src/React/Components/AuthLoginModal.jsx` |
| Hồ sơ người dùng | `backend/src/TuneVault.API/Controllers/UsersController.cs`, `backend/src/TuneVault.Application/Features/User/Queries/GetProfile/GetUserProfileQueryHandler.cs`, `backend/src/TuneVault.Application/Features/User/Commands/UpdateProfile/UpdateProfileCommandHandler.cs`, `backend/src/TuneVault.Infrastructure/Repositories/UserRepository.cs`, `frontend/src/React/Components/ProfileView.jsx` |
| Thư viện media | `backend/src/TuneVault.API/Controllers/MediaController.cs`, `backend/src/TuneVault.Application/Features/Media/Commands/UploadMedia/UploadMediaCommandHandler.cs`, `backend/src/TuneVault.Domain/Entities/MediaItem.cs`, `backend/src/TuneVault.Infrastructure/Repositories/MediaRepository.cs`, `frontend/src/React/Components/Home.jsx`, `frontend/src/React/Components/LibraryDetailView.jsx`, `frontend/src/React/Components/ManageStudio.jsx` |
| Audio player | `backend/src/TuneVault.API/Controllers/MediaController.cs`, `backend/src/TuneVault.API/Controllers/HistoryController.cs`, `backend/src/TuneVault.Infrastructure/Repositories/PlayHistoryRepository.cs`, `frontend/src/React/Components/PlayerBar.jsx`, `frontend/src/React/Components/NowPlayingView.jsx`, `frontend/src/React/App.tsx` |
| Video player | `backend/src/TuneVault.API/Controllers/MediaController.cs`, `backend/src/TuneVault.Infrastructure/Repositories/MediaRepository.cs`, `frontend/src/React/Components/VideoPlayerView.jsx`, `frontend/src/React/Components/NowPlayingView.jsx`, `frontend/src/React/App.tsx` |

# 3. Chức năng 1: Xác thực người dùng và JWT

Đây là phần em làm kỹ nhất vì nó là nền cho các chức năng còn lại. Ở backend, `AuthController` cung cấp các API `login`, `register`, `send-otp`, `change-password`, `refresh`, `logout`. Trong đó luồng đăng ký có xác minh OTP trước khi tạo tài khoản mới; luồng đăng nhập kiểm tra `idDisplay` và mật khẩu băm bằng BCrypt rồi mới sinh JWT.

`LoginCommandHandler` lấy người dùng từ `UserRepository`, kiểm tra mật khẩu bằng `BCrypt.Verify`, sau đó gọi `JwtTokenGenerator` để sinh access token và refresh token. `RegisterCommand` cũng đi theo hướng tương tự nhưng có bước xác minh OTP qua `IOtpLogRepository`, kiểm tra trùng email và `idDisplay`, sau đó mới thêm user mới vào SQL Server.

Luồng đăng nhập chính:

`Frontend -> AuthController -> LoginCommandHandler -> UserRepository -> SQL Server -> JwtTokenGenerator -> Frontend`

Luồng lấy người dùng hiện tại:

`Frontend -> API có [Authorize] -> CurrentUserService lấy user id từ claims -> UserRepository -> SQL Server -> trả profile về frontend`

Ở `Program.cs`, backend cấu hình `AddAuthentication().AddJwtBearer(...)`, khai báo `TokenValidationParameters`, đồng thời đọc token từ header Bearer hoặc cookie `tunevault_access_token`. Khi token không hợp lệ, middleware trả lỗi `401`; khi tài khoản không đủ quyền, middleware trả `403`.

Các API tiêu biểu cần token gồm:

- `POST /api/auth/change-password`
- `GET /api/users/me/profile`
- `PUT /api/users/me/profile`
- `GET /api/media/my-media`
- `GET /api/media/{id}/audio/stream`
- `GET /api/media/{id}/video/stream`
- Toàn bộ nhóm `/api/history/*`

Ở frontend, `MediaService.tsx` lưu phiên đăng nhập trong `localStorage` với các key như `auth_session`, `auth_access_token`, `auth_refresh_token`. Khi gọi API, service tự gắn header `Authorization: Bearer ...` nếu có token, đồng thời vẫn gửi `credentials: include` để backend có thể dùng cookie refresh token. Nếu request bị `401`, hàm `request()` sẽ thử gọi `POST /api/auth/refresh`, cập nhật lại phiên đăng nhập rồi gửi lại request cũ.

`AuthLoginModal.jsx` hiện đã có các luồng:

- Đăng nhập bằng `idDisplay` và mật khẩu
- Gửi OTP đăng ký
- Đăng ký tài khoản sau khi nhập OTP
- Gửi OTP đổi mật khẩu
- Đổi mật khẩu cho người dùng hiện tại

`App.tsx` dùng trạng thái `isAuthenticated` dựa trên `auth_session` trong `localStorage`, sau đó gọi `getMyProfile()` để đồng bộ avatar và email của user hiện tại. Riêng API `reset-password` đã có ở backend, nhưng ở giao diện hiện em chưa thấy luồng sử dụng rõ ràng, nên phần này cần kiểm thử thêm.

# 4. Chức năng 2: Hồ sơ người dùng

Chức năng hồ sơ người dùng tập trung ở `UsersController`. API `GET /api/users/me/profile` lấy hồ sơ đầy đủ của chính người dùng đang đăng nhập. API `PUT /api/users/me/profile` cho phép cập nhật `idDisplay`, `displayName`, `bio` và avatar qua `multipart/form-data`.

Trong backend, `GetUserProfileQueryHandler` lấy user theo id hiện tại, đồng thời đọc thêm số lượng follower và following từ `UserRepository` để trả về `UserProfileDto`. `UpdateProfileCommandHandler` kiểm tra đúng chủ sở hữu hồ sơ, tránh cập nhật sang user khác; đồng thời kiểm tra trùng `idDisplay` trước khi ghi lại xuống SQL Server.

Avatar được xử lý ở `UsersController` thông qua `IFileStorageService`. Nếu người dùng upload avatar mới thì file được lưu vào thư mục `uploads/avatars`; nếu yêu cầu xóa avatar thì controller cập nhật lại profile và xóa file cũ khi cần.

Ở frontend, `ProfileView.jsx` hiển thị hồ sơ, cho phép sửa `displayName`, `idDisplay`, `bio`, chọn ảnh đại diện mới hoặc bỏ avatar hiện tại. Sau khi lưu thành công, frontend gọi lại `getMyProfile()` để đồng bộ dữ liệu đang hiển thị.

Các endpoint chính của phần này:

- `GET /api/users/me/profile`
- `PUT /api/users/me/profile`
- `GET /api/users/by-handle/{idDisplay}`
- `GET /api/users/{id}`

# 5. Chức năng 3: Thư viện Media

Chức năng thư viện media nằm chủ yếu trong `MediaController`, `UploadMediaCommandHandler`, `MediaItem` và `MediaRepository`. Backend hỗ trợ lấy danh sách media công khai, lấy chi tiết từng media, lấy media của người dùng hiện tại, lấy media theo nghệ sĩ và upload media mới.

Khi upload, controller nhận `multipart/form-data`, kiểm tra loại file, sau đó lưu file vật lý vào các thư mục con trong `wwwroot/uploads` như:

- `media` cho audio
- `video` cho video
- `media-covers` cho ảnh bìa
- `canvas` cho canvas đi kèm

Sau khi lưu file, controller dựng `UploadMediaRequestDto` rồi gửi xuống `UploadMediaCommandHandler`. Handler kiểm tra user hiện tại có đúng là owner hay không, kiểm tra loại media (`Audio`, `Video`, `Song`), sau đó tạo `MediaItem` entity và lưu metadata vào SQL Server qua `MediaRepository`.

`MediaItem` trong tầng Domain quản lý các thuộc tính như `Title`, `Description`, `Type`, `CoverImageUrl`, `CanvasUrl`, `Genre`, `Duration`, `IsPublic`, `FavoriteCount`, `ViewCount`, `UploadedAt`, `ReleaseDate`. Đây là phần giúp tách logic nghiệp vụ khỏi controller.

Ở frontend:

- `Home.jsx` gọi API để lấy media công khai và hiển thị ở trang chủ
- `LibraryDetailView.jsx` dùng cho phần hiển thị chi tiết bộ sưu tập
- `ManageStudio.jsx` là nơi quản lý media đã upload, cập nhật thông tin và thao tác upload

Các endpoint chính:

- `GET /api/media`
- `GET /api/media/{id}`
- `GET /api/media/my-media`
- `GET /api/media/artist/{userId}`
- `POST /api/media/upload`
- `POST /api/media/upload/audio`
- `POST /api/media/upload/video`
- `PUT /api/media/{id}`

# 6. Chức năng 4: Audio Player

Audio player được triển khai theo hướng dùng một audio source trung tâm ở frontend và kết nối với API stream ở backend. API `GET /api/media/{id}/audio/stream` trả file audio vật lý để frontend phát. Ở phía React, `App.tsx` quản lý queue, track hiện tại, trạng thái `isPlaying`, seek, restore bài đang nghe gần nhất và đồng bộ với `PlayerBar.jsx`.

`PlayerBar.jsx` hiện có các thao tác cơ bản:

- Play/Pause
- Bài trước/Bài sau
- Seek theo tiến độ
- Điều chỉnh âm lượng và mute
- Mở rộng player

Khi người dùng phát một bài, `App.tsx` gọi `recordPlayHistory(mediaId)` để gửi `POST /api/history/{mediaId}`. Trong quá trình nghe hoặc khi dừng bài, frontend gọi `recordPlaybackStop(mediaId, stoppedAt)` để lưu lại vị trí dừng. Lần sau khi vào lại, frontend gọi `GET /api/history/recent` và `GET /api/history/{mediaId}/resume` để khôi phục bài nghe gần nhất.

Ở backend, `HistoryController` kết hợp với `RecordPlayHistoryCommandHandler` và `PlayHistoryRepository` để:

- Ghi lịch sử nghe mới
- Đưa bài vừa nghe lên đầu lịch sử
- Giữ tối đa 10 bản ghi gần nhất
- Lưu thời điểm dừng phát để hỗ trợ resume

Nhìn chung, phần audio player đã có đủ luồng phát nhạc, lưu lịch sử và phát tiếp từ vị trí cũ. Tuy nhiên, độ ổn định khi mạng chậm hoặc file audio lớn vẫn cần kiểm thử thêm.

# 7. Chức năng 5: Video Player

Video player dùng chung dữ liệu media với thư viện media nhưng có màn hình phát riêng cho nội dung video. Backend cung cấp API `GET /api/media/{id}/video/stream` để stream file video và `GET /api/media/{id}/poster` để lấy ảnh bìa/poster.

Trong `MediaController`, cả audio stream và video stream đều gọi `PhysicalFile(..., enableRangeProcessing: true)`. Điều này cho thấy backend đã hỗ trợ range request cho việc tua và tải từng phần của file media, phù hợp với trình phát video trên trình duyệt.

Ở frontend, `VideoPlayerView.jsx` và `NowPlayingView.jsx` hiển thị video bằng thẻ `<video>`, đồng thời đồng bộ thời gian phát với `audioRef` trong `App.tsx`. Cách làm này giúp giao diện phát video và hệ thống player trung tâm không bị tách rời hoàn toàn.

Các điểm chính của phần video:

- Chọn media có loại `video`
- Stream video qua API có xác thực
- Hỗ trợ poster/thumbnail
- Có seek và đồng bộ thời gian giữa giao diện video với audio player trung tâm

Phần này về mặt code đã thể hiện khá rõ. Dù vậy, việc kiểm thử trên nhiều định dạng video và nhiều trình duyệt khác nhau vẫn cần làm thêm để đánh giá độ ổn định thực tế.

# 8. Phần cấu hình và deploy liên quan

Trong phạm vi em làm, phần cấu hình đáng chú ý nhất là `Program.cs` đã khai báo CORS cho các domain local và có thêm domain frontend production `https://project-c-sharp.vercel.app`. Điều này cho thấy backend đã chuẩn bị cho kịch bản frontend chạy tách riêng với API.

Ngoài ra repo hiện có:

- `backend/src/TuneVault.API/Dockerfile` để build/publish backend
- `frontend/Dockerfile` để build frontend
- `docker-compose.pro.yml` cho cấu hình chạy theo hướng deploy

Backend hiện đọc cấu hình qua `appsettings.json` và environment variables. Tuy nhiên, việc deploy hoàn chỉnh lên môi trường production như Somee hoặc hạ tầng cloud khác vẫn cần kiểm thử thêm. Em không đưa secret, password hay connection string thật vào báo cáo này.

# 9. Những phần không thuộc phạm vi cá nhân

Chức năng Chia sẻ Media và Thông báo không thuộc phạm vi báo cáo cá nhân này. Báo cáo chỉ tập trung vào 5 chức năng đầu và phần xác thực/JWT mà em trực tiếp tham gia thực hiện.

# 10. Tổng kết

Qua quá trình làm TuneVault, em đã trực tiếp tham gia các phần cốt lõi gồm xác thực người dùng bằng JWT, hồ sơ cá nhân, thư viện media, audio player và video player. Đây là những phần nền quan trọng vì chúng ảnh hưởng trực tiếp tới luồng đăng nhập, quản lý media và trải nghiệm phát nội dung trong toàn hệ thống.

Về mặt kỹ thuật, em học được nhiều nhất ở các nội dung sau:

- Tổ chức backend theo Clean Architecture
- Sử dụng Dapper để viết truy vấn SQL chủ động
- Xử lý JWT, claims và middleware xác thực
- Upload và stream media từ `wwwroot`
- Gọi API từ frontend React/Vite và quản lý trạng thái đăng nhập
- Cấu hình CORS và chuẩn bị cho hướng deploy

Hạn chế hiện tại là một số luồng vẫn cần kiểm thử end-to-end kỹ hơn, đặc biệt là tính ổn định của stream media, một số tình huống refresh token và cấu hình deploy production thực tế. Trong thời gian tới, em muốn hoàn thiện thêm phần kiểm thử, tối ưu trải nghiệm player và chuẩn hóa tài liệu kỹ thuật để dễ bảo trì hơn.
