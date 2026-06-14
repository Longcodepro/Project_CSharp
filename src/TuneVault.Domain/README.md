# Domain Entities Summary

Tài liệu này chỉ được tổng hợp từ thư mục `src/TuneVault.Domain/Entities` để hỗ trợ thiết kế database.

## Quy ước chung

- Các field `Id` kiểu `string` thường là khóa chính.
- Các field `...Id` thường là khóa ngoại sang entity khác.
- Các collection như `Tracks` là quan hệ 1-n và không nên map thành một cột đơn.
- Một số kiểu như `TierPrice`, `TierCapabilities`, `MediaUrl`, `MediaDuration` là value object, cần cân nhắc map thành nhiều cột hoặc owned type.
- Một số entity dùng enum như `MediaType`, `AccessLevel`, `NotificationType`, `FriendStatus`, `FavoriteReaction`, `ShareType`, `AdType`.

## Entities

### 1) `AccountTier`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `Code` | `string` | No | Mã gói |
| `Name` | `string` | No | Tên gói |
| `Price` | `TierPrice` | No | Value object |
| `Capabilities` | `TierCapabilities` | No | Value object |
| `DurationInDays` | `int` | No | Thời hạn gói |
| `CreatedAt` | `DateTime` | No | Ngày tạo |
| `ActiveFrom` | `DateTime` | No | Bắt đầu hiệu lực |
| `ActiveTo` | `DateTime?` | Yes | Kết thúc hiệu lực |
| `IsActive` | `bool` | No | Trạng thái |

### 2) `Ad` trong file `AdMedia.cs`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `Title` | `string` | No | Tiêu đề quảng cáo |
| `Advertiser` | `string` | No | Nhà quảng cáo |
| `Type` | `AdType` | No | Enum |
| `Media` | `AdMedia` | No | Value object / media cấu hình |
| `IsActive` | `bool` | No | Trạng thái |
| `CreatedAt` | `DateTime` | No | Ngày tạo |

### 3) `Admin`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `Name` | `string` | No | Họ tên |
| `Email` | `string` | No | Unique nên cân nhắc |
| `PasswordHash` | `string` | No | Hash mật khẩu |
| `PhoneNumber` | `string` | No | Số điện thoại |
| `Role` | `string` | No | Vai trò |
| `IsActive` | `bool` | No | Trạng thái |

### 4) `Album`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `ArtistId` | `string` | No | FK tới `User`/artist |
| `Title` | `string` | No | Tên album |
| `Description` | `string?` | Yes | Mô tả |
| `CoverImageUrl` | `string?` | Yes | Ảnh bìa |
| `CreatedAt` | `DateTime` | No | Ngày tạo |
| `Tracks` | `IReadOnlyCollection<AlbumTrack>` | No | Quan hệ 1-n |

### 5) `AlbumTrack`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `AlbumId` | `string` | No | FK `Album` |
| `MediaItemId` | `string` | No | FK `MediaItem` |
| `TrackOrder` | `int` | No | Thứ tự bài |
| `AddedAt` | `DateTime` | No | Ngày thêm |

### 6) `Favorite`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `UserId` | `string` | No | FK `User` |
| `MediaItemId` | `string` | No | FK `MediaItem` |
| `Reaction` | `FavoriteReaction` | No | Enum |
| `LikedAt` | `DateTime` | No | Thời điểm thích |

### 7) `Follow`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `FollowerId` | `string` | No | FK người theo dõi |
| `FolloweeId` | `string` | No | FK người được theo dõi |
| `FollowedAt` | `DateTime` | No | Thời điểm theo dõi |
| `IsActive` | `bool` | No | Đang follow hay đã unfollow |

### 8) `Friend`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `RequestedById` | `string` | No | Người gửi lời mời |
| `RequestedToId` | `string` | No | Người nhận lời mời |
| `Status` | `FriendStatus` | No | Enum |
| `CreatedAt` | `DateTime` | No | Ngày tạo |

### 9) `MediaArtist`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `MediaItemId` | `string` | No | FK `MediaItem` |
| `ArtistId` | `string` | No | FK `User`/artist |
| `Role` | `string` | No | Vai trò của nghệ sĩ |

### 10) `MediaItem`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `OwnerId` | `string` | No | FK chủ sở hữu |
| `Title` | `string` | No | Tên nội dung |
| `Description` | `string?` | Yes | Mô tả |
| `Type` | `MediaType` | No | Enum |
| `Url` | `MediaUrl` | No | Value object |
| `CoverImageUrl` | `string?` | Yes | Ảnh bìa |
| `CanvasUrl` | `string?` | Yes | Canvas/artwork |
| `Genre` | `string?` | Yes | Thể loại |
| `Duration` | `MediaDuration` | No | Value object |
| `DurationTrailer` | `MediaDuration` | No | Value object |
| `AccessLevel` | `AccessLevel` | No | Enum |
| `IsPublic` | `bool` | No | Public/private |
| `FavoriteCount` | `int` | No | Số lượt thích |
| `ViewCount` | `int` | No | Số lượt xem |
| `UploadedAt` | `DateTime` | No | Ngày upload |
| `ReleaseDate` | `DateTime?` | Yes | Ngày phát hành |

### 11) `MediaShare`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `SenderId` | `string` | No | Người gửi |
| `ReceiverId` | `string` | No | Người nhận |
| `SharedItemId` | `string` | No | Nội dung được chia sẻ |
| `ShareType` | `ShareType` | No | Enum |
| `Message` | `string?` | Yes | Lời nhắn |
| `SharedAt` | `DateTime` | No | Thời điểm chia sẻ |

### 12) `Notification`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `UserId` | `string` | No | FK `User` |
| `Type` | `NotificationType` | No | Enum |
| `Title` | `string` | No | Tiêu đề |
| `Message` | `string` | No | Nội dung |
| `IsRead` | `bool` | No | Đã đọc hay chưa |
| `CreatedAt` | `DateTime` | No | Ngày tạo |

### 13) `Playlist`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `UserId` | `string` | No | FK `User` |
| `Title` | `string` | No | Tên playlist |
| `Description` | `string?` | Yes | Mô tả |
| `CoverImageUrl` | `string?` | Yes | Ảnh bìa |
| `IsPublic` | `bool` | No | Public/private |
| `CreatedAt` | `DateTime` | No | Ngày tạo |
| `Tracks` | `IReadOnlyCollection<PlaylistTrack>` | No | Quan hệ 1-n |

### 14) `PlaylistTrack`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `PlaylistId` | `string` | No | FK `Playlist` |
| `MediaItemId` | `string` | No | FK `MediaItem` |
| `TrackOrder` | `int` | No | Thứ tự bài |
| `AddedAt` | `DateTime` | No | Ngày thêm |

### 15) `PlayHistory`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `UserId` | `string` | No | FK `User` |
| `MediaItemId` | `string` | No | FK `MediaItem` |
| `HistoryOrder` | `int` | No | Thứ tự lịch sử |
| `StoppedAt` | `DateTime?` | Yes | Thời điểm dừng |

### 16) `User`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `IdDisplay` | `string` | No | Tên hiển thị ngắn / handle |
| `DisplayName` | `string` | No | Tên hiển thị |
| `Email` | `string` | No | Unique nên cân nhắc |
| `PasswordHash` | `string` | No | Hash mật khẩu |
| `AvatarUrl` | `string?` | Yes | Ảnh đại diện |
| `Bio` | `string?` | Yes | Tiểu sử |
| `IsArtist` | `bool` | No | Có phải nghệ sĩ |
| `TotalFollowers` | `int` | No | Tổng follower |
| `CreatedAt` | `DateTime` | No | Ngày tạo |
| `IsActive` | `bool` | No | Trạng thái |

### 17) `UserAccountTier`

| Field | Type | Nullable | Ghi chú |
| --- | --- | --- | --- |
| `Id` | `string` | No | PK |
| `UserId` | `string` | No | FK `User` |
| `TierId` | `string` | No | FK `AccountTier` |
| `PriceAtPurchase` | `TierPrice` | No | Value object |
| `PurchasedAt` | `DateTime` | No | Ngày mua |
| `ActivatedAt` | `DateTime` | No | Ngày kích hoạt |
| `ExpiresAt` | `DateTime` | No | Ngày hết hạn |
| `IsActive` | `bool` | No | Trạng thái |

## Ghi chú thiết kế database

- `MediaArtist` có vẻ là bảng liên kết nhiều-nhiều, có thể dùng khóa chính ghép `(MediaItemId, ArtistId)` thay vì thêm `Id`.
- `Album.Track` và `Playlist.Track` nên tách thành bảng riêng như hiện tại.
- Các field kiểu `DateTime?` nên map sang cột nullable.
- Nếu bạn muốn, có thể tiếp tục chuyển tài liệu này thành sơ đồ database hoặc script migration.
