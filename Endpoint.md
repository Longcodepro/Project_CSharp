---
# API Endpoint Documentation

## Tổng quan
- Tổng số endpoint: 36
- Base URL: https://localhost:7263

## AlbumController

### GET /api/Album
- **Chức năng**: Lấy tất cả albums.
- **Xác thực**: Không cần
- **Request**: Không
- **Response**: StatusCode(501 Not Implemented)

### POST /api/Album
- **Chức năng**: Tạo album mới.
- **Xác thực**: Không cần
- **Request**: Không
- **Response**: StatusCode(501 Not Implemented)

## AuthController

### POST /api/auth/login
- **Chức năng**: Đăng nhập, trả về JWT nếu thành công.
- **Xác thực**: Không cần
- **Request**: `LoginCommand` (body: `IdDisplay`, `Password`)
- **Response**: `ApiResponse<AuthResponseDto>` (chứa JWT token)

### POST /api/auth/send-otp
- **Chức năng**: Gửi mã OTP đến email (purpose: "register" hoặc "reset_password").
- **Xác thực**: Không cần
- **Request**: `SendOtpCommand` (body: `Email`, `Purpose`)
- **Response**: `ApiResponse<null>` (thông báo thành công)

### POST /api/auth/register
- **Chức năng**: Đăng ký tài khoản mới sau khi xác minh OTP.
- **Xác thực**: Không cần
- **Request**: `RegisterCommand` (body: `Email`, `OtpCode`, `IdDisplay`, `DisplayName`, `PasswordHash`)
- **Response**: `ApiResponse<AuthResponseDto>` (chứa JWT token)

### POST /api/auth/reset-password
- **Chức năng**: Đặt lại mật khẩu sau khi xác minh OTP.
- **Xác thực**: Không cần
- **Request**: `ResetPasswordCommand` (body: `Email`, `OtpCode`, `NewPasswordHash`)
- **Response**: `ApiResponse<null>` (thông báo thành công)

## FavoriteController

### POST /api/Favorite/like
- **Chức năng**: Đánh dấu một bài hát là Like cho người dùng.
- **Xác thực**: Bearer Token
- **Request**: `FavoriteStatusRequest` (body: `UserId`, `MediaItemId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `UserId`, `MediaItemId`, `likeStatus`)

### POST /api/Favorite/dislike
- **Chức năng**: Đánh dấu một bài hát là Dislike cho người dùng (thực chất là xóa Favorite).
- **Xác thực**: Bearer Token
- **Request**: `FavoriteStatusRequest` (body: `UserId`, `MediaItemId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `UserId`, `MediaItemId`, `likeStatus`)

### DELETE /api/Favorite
- **Chức năng**: Xóa trạng thái Like hoặc Dislike của một bài hát khỏi danh sách Favorite của người dùng.
- **Xác thực**: Bearer Token
- **Request**: Params: `userId`, `mediaItemId`
- **Response**: `ApiResponse<object>` (thông báo thành công, `userId`, `mediaItemId`)

### GET /api/Favorite/liked/{userId}
- **Chức năng**: Lấy danh sách các bài hát mà người dùng đã Like.
- **Xác thực**: Bearer Token
- **Request**: Path: `userId`
- **Response**: `ApiResponse<List<FavoriteItem>>`

### GET /api/Favorite/status
- **Chức năng**: Kiểm tra trạng thái Like/Dislike hiện tại của một bài hát đối với người dùng.
- **Xác thực**: Bearer Token
- **Request**: Params: `userId`, `mediaItemId`
- **Response**: `ApiResponse<object>` (chứa `userId`, `mediaItemId`, `isFavorite`)

## FollowController

### POST /api/Follow
- **Chức năng**: Follow một nghệ sĩ/người dùng khác.
- **Xác thực**: Bearer Token
- **Request**: `FollowRequest` (body: `FollowerId`, `FolloweeId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `FollowerId`, `FolloweeId`)

### DELETE /api/Follow
- **Chức năng**: Bỏ follow một nghệ sĩ/người dùng khác.
- **Xác thực**: Bearer Tokens
- **Request**: `FollowRequest` (body: `FollowerId`, `FolloweeId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `FollowerId`, `FolloweeId`)

### GET /api/Follow/status
- **Chức năng**: Kiểm tra một người dùng có đang follow một nghệ sĩ/người dùng khác hay không.
- **Xác thực**: Bearer Token
- **Request**: Params: `followerId`, `followeeId`
- **Response**: `ApiResponse<object>` (chứa `followerId`, `followeeId`, `isFollowing`)

### GET /api/Follow/following/{userId}
- **Chức năng**: Lấy danh sách nghệ sĩ mà người dùng đang follow.
- **Xác thực**: Bearer Token
- **Request**: Path: `userId`
- **Response**: `ApiResponse<List<User>>`

### GET /api/Follow/followers/{artistId}
- **Chức năng**: Lấy danh sách người đang follow một nghệ sĩ/người dùng.
- **Xác thực**: Bearer Token
- **Request**: Path: `artistId`
- **Response**: `ApiResponse<List<User>>`

### GET /api/Follow/followers-count/{artistId}
- **Chức năng**: Đếm số follower của một nghệ sĩ/người dùng.
- **Xác thực**: Bearer Token
- **Request**: Path: `artistId`
- **Response**: `ApiResponse<object>` (chứa `artistId`, `followerCount`)

## HistoryController

### GET /api/History/recent/{userId}
- **Chức năng**: Lấy danh sách bài hát nghe gần đây của một người dùng.
- **Xác thực**: Bearer Token
- **Request**: Path: `userId`, Params: `limit` (mặc định 10)
- **Response**: `ApiResponse<List<PlayHistoryItem>>`

### POST /api/History
- **Chức năng**: Ghi nhận một lần nghe bài hát của người dùng vào PlayHistory.
- **Xác thực**: Bearer Token
- **Request**: `RecordPlayHistoryRequest` (body: `UserId`, `MediaItemId`, `StoppedAt`)
- **Response**: `ApiResponse<object>` (chứa `success`, `message`, `UserId`, `MediaItemId`, `StoppedAt`)

## MediaController

### GET /api/media/{id}
- **Chức năng**: Lấy thông tin bài hát theo ID.
- **Xác thực**: Không cần
- **Request**: Path: `id`
- **Response**: `ApiResponse<MediaResponseDto>`

### POST /api/media
- **Chức năng**: Tải lên một bài hát mới.
- **Xác thực**: Bearer Token, Role: Artist hoặc Admin
- **Request**: `UploadMediaRequestDto` (body: `Title`, `Description`, `ArtistId`, `Genre`, `Duration`, `FileUrl`)
- **Response**: `ApiResponse<MediaResponseDto>`

### PUT /api/media/{id}
- **Chức năng**: Cập nhật thông tin bài hát theo ID.
- **Xác thực**: Bearer Token, Role: Artist hoặc Admin
- **Request**: Path: `id`, Body: `UpdateMediaRequestDto` (body: `Title`, `Description`, `Genre`, `Duration`, `FileUrl`)
- **Response**: `ApiResponse<MediaResponseDto>`

### DELETE /api/media/{id}
- **Chức năng**: Xóa bài hát theo ID.
- **Xác thực**: Bearer Token, Role: Artist hoặc Admin
- **Request**: Path: `id`
- **Response**: `ApiResponse<object>` (thông báo thành công)

## NotificationController

### GET /api/Notification/{userId}
- **Chức năng**: Lấy danh sách thông báo còn hiển thị của một người dùng.
- **Xác thực**: Bearer Token
- **Request**: Path: `userId`, Params: `limit` (mặc định 50)
- **Response**: `ApiResponse<List<Notification>>`

### GET /api/Notification/unread/{userId}
- **Chức năng**: Lấy danh sách thông báo chưa đọc của một người dùng.
- **Xác thực**: Bearer Token
- **Request**: Path: `userId`, Params: `limit` (mặc định 50)
- **Response**: `ApiResponse<List<Notification>>`

### GET /api/Notification/unread-count/{userId}
- **Chức năng**: Đếm số lượng thông báo chưa đọc.
- **Xác thực**: Bearer Token
- **Request**: Path: `userId`
- **Response**: `ApiResponse<object>` (chứa `userId`, `unreadNotificationCount`)

### PATCH /api/Notification/{notificationId}/read
- **Chức năng**: Đánh dấu một thông báo là đã đọc.
- **Xác thực**: Bearer Token
- **Request**: Path: `notificationId`, Params: `userId`
- **Response**: `ApiResponse<object>` (thông báo thành công, `notificationId`, `userId`)

### DELETE /api/Notification/{notificationId}
- **Chức năng**: Xóa mềm một thông báo (chuyển IsActive = 0).
- **Xác thực**: Bearer Token
- **Request**: Path: `notificationId`, Params: `userId`
- **Response**: `ApiResponse<object>` (thông báo thành công, `notificationId`, `userId`, `isActive = false`)

### DELETE /api/Notification/all/{userId}
- **Chức năng**: Xóa mềm toàn bộ thông báo của một người dùng (chuyển toàn bộ IsActive = 0).
- **Xác thực**: Bearer Token
- **Request**: Path: `userId`
- **Response**: `ApiResponse<object>` (thông báo thành công, `userId`, `deletedCount`, `isActive = false`)

### POST /api/Notification/artist-new-media
- **Chức năng**: Tạo thông báo demo khi nghệ sĩ đăng bài mới.
- **Xác thực**: Bearer Token
- **Request**: `ArtistNewMediaNotificationRequest` (body: `UserId`, `ArtistId`, `MediaItemId`, `Title`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `notificationId`, `UserId`)

### POST /api/Notification/system
- **Chức năng**: Tạo thông báo hệ thống cho một người dùng.
- **Xác thực**: Bearer Token
- **Request**: `SystemNotificationRequest` (body: `UserId`, `Title`, `Message`, `SenderId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `notificationId`, `UserId`)

## PlaylistController

### GET /api/Playlist
- **Chức năng**: Lấy tất cả playlists.
- **Xác thực**: Không cần
- **Request**: Không
- **Response**: StatusCode(501 Not Implemented)

### POST /api/Playlist
- **Chức năng**: Tạo playlist mới.
- **Xác thực**: Không cần
- **Request**: Không
- **Response**: StatusCode(501 Not Implemented)

### POST /api/Playlist/{playlistId}/tracks
- **Chức năng**: Thêm bài hát vào playlist.
- **Xác thực**: Không cần
- **Request**: Path: `playlistId`
- **Response**: StatusCode(501 Not Implemented)

## SearchController

### GET /api/Search
- **Chức năng**: Tìm kiếm theo từ khóa.
- **Xác thực**: Không cần
- **Request**: Params: `keyword`
- **Response**: StatusCode(501 Not Implemented)

## ShareController

### POST /api/Share/track
- **Chức năng**: Chia sẻ một bài hát (track) cho người dùng khác.
- **Xác thực**: Bearer Token
- **Request**: `ShareItemRequest` (body: `SenderId`, `ReceiverId`, `SharedItemId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `shareId`, `SenderId`, `ReceiverId`, `shareType`, `SharedItemId`)

### POST /api/Share/album
- **Chức năng**: Chia sẻ một album cho người dùng khác.
- **Xác thực**: Bearer Token
- **Request**: `ShareItemRequest` (body: `SenderId`, `ReceiverId`, `SharedItemId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `shareId`, `SenderId`, `ReceiverId`, `shareType`, `SharedItemId`)

### POST /api/Share/playlist
- **Chức năng**: Chia sẻ một playlist cho người dùng khác.
- **Xác thực**: Bearer Token
- **Request**: `ShareItemRequest` (body: `SenderId`, `ReceiverId`, `SharedItemId`)
- **Response**: `ApiResponse<object>` (thông báo thành công, `shareId`, `SenderId`, `ReceiverId`, `shareType`, `SharedItemId`)

### GET /api/Share/inbox/{receiverId}
- **Chức năng**: Lấy danh sách các mục đã được chia sẻ với người dùng.
- **Xác thực**: Bearer Token
- **Request**: Path: `receiverId`
- **Response**: `ApiResponse<List<SharedItem>>`

### PATCH /api/Share/{shareId}/read
- **Chức năng**: Đánh dấu một mục chia sẻ là đã đọc.
- **Xác thực**: Bearer Token
- **Request**: Path: `shareId`, Params: `receiverId`
- **Response**: `ApiResponse<object>` (thông báo thành công, `shareId`, `receiverId`)

### GET /api/Share/unread-count/{receiverId}
- **Chức thực**: Bearer Token
- **Request**: Path: `receiverId`
- **Response**: `ApiResponse<object>` (chứa `receiverId`, `unreadShareCount`)

## UsersController

### GET /api/users/{id}
- **Chức năng**: Lấy thông tin người dùng theo ID.
- **Xác thực**: Không cần
- **Request**: Path: `id`
- **Response**: `ApiResponse<UserDto>`

### GET /api/users/artists
- **Chức năng**: Lấy danh sách tất cả nghệ sĩ.
- **Xác thực**: Không cần
- **Request**: Không
- **Response**: `ApiResponse<List<ArtistDto>>`

### GET /api/users/{id}/followers
- **Chức năng**: Lấy danh sách người theo dõi của một người dùng.
- **Xác thực**: Không cần
- **Request**: Path: `id`
- **Response**: `ApiResponse<List<UserDto>>`

### GET /api/users/{id}/following
- **Chức năng**: Lấy danh sách người mà một người dùng đang theo dõi.
- **Xác thực**: Không cần
- **Request**: Path: `id`
- **Response**: `ApiResponse<List<UserDto>>`

### GET /api/users/{followerId}/is-following/{followeeId}
- **Chức năng**: Kiểm tra trạng thái theo dõi giữa hai người dùng.
- **Xác thực**: Không cần
- **Request**: Path: `followerId`, `followeeId`
- **Response**: `ApiResponse<object>` (chứa `isFollowing`)

### GET /api/users/{id}/profile
- **Chức năng**: Lấy thông tin hồ sơ của người dùng.
- **Xác thực**: Không cần
- **Request**: Path: `id`
- **Response**: `ApiResponse<UserProfileDto>`

### GET /api/users/display/{idDisplay}
- **Chức năng**: Lấy thông tin người dùng theo ID hiển thị.
- **Xác thực**: Không cần
- **Request**: Path: `idDisplay`
- **Response**: `ApiResponse<UserDto>`

### PUT /api/users/profile
- **Chức năng**: Cập nhật thông tin hồ sơ người dùng.
- **Xác thực**: Bearer Token
- **Request**: `UpdateProfileCommand` (body: `DisplayName`, `Bio`, `ProfilePictureUrl`)
- **Response**: `ApiResponse<UserProfileDto>`

### PUT /api/users/security
- **Chức năng**: Cập nhật thông tin bảo mật người dùng.
- **Xác thực**: Bearer Token
- **Request**: `UpdateSecurityCommand` (body: `CurrentPassword`, `NewPassword`)
- **Response**: `ApiResponse<object>` (thông báo thành công)

### POST /api/users/follow
- **Chức năng**: Theo dõi một người dùng khác.
- **Xác thực**: Bearer Token
- **Request**: `FollowUserCommand` (body: `FolloweeId`)
- **Response**: `ApiResponse<object>` (thông báo thành công)

### DELETE /api/users/unfollow
- **Chức năng**: Bỏ theo dõi một người dùng khác.
- **Xác thực**: Bearer Token
- **Request**: `UnfollowUserCommand` (body: `FolloweeId`)
- **Response**: `ApiResponse<object>` (thông báo thành công)

### PATCH /api/users/{id}/verify-artist
- **Chức năng**: Xác minh một người dùng là nghệ sĩ.
- **Xác thực**: Bearer Token, Role: Admin
- **Request**: Path: `id`
- **Response**: `ApiResponse<UserDto>`
---