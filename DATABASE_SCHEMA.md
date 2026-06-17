# DATABASE_SCHEMA.md — TuneVault Database Schema

> File này được tạo từ `Untitled.sql` hiện tại. Dùng làm nguồn tham chiếu cho AI Agent khi viết Dapper Repository, DTO mapping và endpoint.

> Lưu ý: Vì project dùng Dapper, AI **không được đoán tên bảng/cột**. Nếu bảng/cột không có trong file này thì phải hỏi lại trước khi code.

## Tổng quan

- Database/schema trong script: `TuneVault.dbo`
- Tổng số bảng đọc được: **18**
- ORM: Dapper, không dùng EF Core migration.
- SQL schema do developer tạo/cập nhật thủ công. AI chỉ được đề xuất thay đổi SQL khi được yêu cầu rõ.

## Danh sách bảng

| # | Table | Ghi chú nhanh |
|---:|---|---|
| 1 | `AccountTiers` | Gói tài khoản |
| 2 | `Admins` | Tài khoản quản trị |
| 3 | `Ads` | Quảng cáo |
| 4 | `OtpLogs` | OTP cho xác thực/quên mật khẩu |
| 5 | `Users` | Tài khoản người dùng chính, artist flag, follower count |
| 6 | `Albums` | Album của artist |
| 7 | `Follows` | Follow user/artist, soft delete bằng IsActive |
| 8 | `Friends` | Yêu cầu bạn bè |
| 9 | `MediaItems` | Metadata audio/video/poster/canvas |
| 10 | `MediaShares` | Chia sẻ media/playlist |
| 11 | `Notifications` | Thông báo trong hệ thống |
| 12 | `PlayHistory` | Lịch sử nghe/xem |
| 13 | `Playlists` | Playlist của user |
| 14 | `UserAccountTiers` | Gói đã mua của user |
| 15 | `AlbumTracks` | Track trong album |
| 16 | `Favorites` | Like/favorite media |
| 17 | `MediaArtists` | N-N giữa media và artist |
| 18 | `PlaylistTracks` | Track trong playlist |

---

## `AccountTiers`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `Code` | `varchar(10)` | NOT NULL | `` |
| `Name` | `nvarchar(100)` | NOT NULL | `` |
| `PriceAmount` | `decimal(10,2)` | NOT NULL | `` |
| `PriceCurrency` | `varchar(5)` | NOT NULL | `'USD'` |
| `MaxUploadMb` | `int` | NOT NULL | `` |
| `CanDownload` | `bit` | NOT NULL | `0` |
| `CanSkipAds` | `bit` | NOT NULL | `0` |
| `MaxDevices` | `int` | NOT NULL | `1` |
| `DurationInDays` | `int` | NOT NULL | `` |
| `CreatedAt` | `datetime2` | NOT NULL | `` |
| `ActiveFrom` | `datetime2` | NOT NULL | `` |
| `ActiveTo` | `datetime2` | NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |

**Constraints**

- `CONSTRAINT PK__AccountT__3214EC07646FBE11 PRIMARY KEY (Id)`
- `CONSTRAINT UQ__AccountT__A25C5AA71DA9CC75 UNIQUE (Code)`

## `Admins`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `Name` | `nvarchar(150)` | NOT NULL | `` |
| `Email` | `varchar(255)` | NOT NULL | `` |
| `PasswordHash` | `varchar(255)` | NOT NULL | `` |
| `PhoneNumber` | `varchar(20)` | NOT NULL | `` |
| `Role` | `varchar(50)` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |

**Constraints**

- `CONSTRAINT PK__Admins__3214EC07A436EA86 PRIMARY KEY (Id)`
- `CONSTRAINT UQ__Admins__A9D10534A895CC10 UNIQUE (Email)`

## `Ads`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `Title` | `nvarchar(200)` | NOT NULL | `` |
| `Advertiser` | `nvarchar(200)` | NOT NULL | `` |
| `AdType` | `tinyint` | NOT NULL | `` |
| `MediaUrl` | `varchar(500)` | NULL | `` |
| `ClickThroughUrl` | `varchar(500)` | NULL | `` |
| `DurationSeconds` | `int` | NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |
| `CreatedAt` | `datetime2` | NOT NULL | `` |

**Constraints**

- `CONSTRAINT PK__Ads__3214EC0798C30D80 PRIMARY KEY (Id)`

## `OtpLogs`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `Email` | `varchar(255)` | NOT NULL | `` |
| `OtpCode` | `varchar(10)` | NOT NULL | `` |
| `Purpose` | `varchar(20)` | NOT NULL | `` |
| `CreatedAt` | `datetime2` | NOT NULL | `getutcdate()` |
| `ExpiresAt` | `datetime2` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |

**Constraints**

- `CONSTRAINT PK_OtpLogs PRIMARY KEY (Id)`

**Indexes**

- `IX_OtpLogs_Email_Purpose_IsActive` on `Email ASC , Purpose ASC , IsActive ASC`

## `Users`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `IdDisplay` | `varchar(50)` | NOT NULL | `` |
| `DisplayName` | `nvarchar(150)` | NOT NULL | `` |
| `Email` | `varchar(255)` | NOT NULL | `` |
| `PasswordHash` | `varchar(255)` | NOT NULL | `` |
| `AvatarUrl` | `varchar(500)` | NULL | `` |
| `Bio` | `nvarchar(500)` | NULL | `` |
| `IsArtist` | `bit` | NOT NULL | `0` |
| `TotalFollowers` | `int` | NOT NULL | `0` |
| `CreatedAt` | `datetime2` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |

**Constraints**

- `CONSTRAINT PK__Users__3214EC0727F2F97F PRIMARY KEY (Id)`
- `CONSTRAINT UQ__Users__A9D1053481C47493 UNIQUE (Email)`
- `CONSTRAINT UQ__Users__BEDCC22A18BF3D40 UNIQUE (IdDisplay)`

## `Albums`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `ArtistId` | `varchar(10)` | NOT NULL | `` |
| `Title` | `nvarchar(255)` | NOT NULL | `` |
| `Description` | `nvarchar(1000)` | NULL | `` |
| `CoverImageUrl` | `varchar(500)` | NULL | `` |
| `CreatedAt` | `datetime2` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |
| `IsPublic` | `bit` | NOT NULL | `1` |
| `ReleaseDate` | `datetime2` | NULL | `` |
| `ContentType` | `tinyint` | NULL | `` |

**Constraints**

- `CONSTRAINT PK__Albums__3214EC07685C75A6 PRIMARY KEY (Id)`
- `CONSTRAINT FK__Albums__ArtistId__6E01572D FOREIGN KEY (ArtistId) REFERENCES TuneVault.dbo.Users(Id)`

## `Follows`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `FollowerId` | `varchar(10)` | NOT NULL | `` |
| `FolloweeId` | `varchar(10)` | NOT NULL | `` |
| `FollowedAt` | `datetime2` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |

**Constraints**

- `CONSTRAINT PK__Follows__3214EC07CB894B37 PRIMARY KEY (Id)`
- `CONSTRAINT FK__Follows__Followe__01142BA1 FOREIGN KEY (FollowerId) REFERENCES TuneVault.dbo.Users(Id)`
- `CONSTRAINT FK__Follows__Followe__02084FDA FOREIGN KEY (FolloweeId) REFERENCES TuneVault.dbo.Users(Id)`

**Indexes**

- `IX_Follows_FolloweeId` on `FolloweeId ASC`

## `Friends`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `RequestedById` | `varchar(10)` | NOT NULL | `` |
| `RequestedToId` | `varchar(10)` | NOT NULL | `` |
| `Status` | `tinyint` | NOT NULL | `1` |
| `CreatedAt` | `datetime2` | NOT NULL | `` |

**Constraints**

- `CONSTRAINT PK__Friends__3214EC078FA7D32C PRIMARY KEY (Id)`
- `CONSTRAINT FK__Friends__Request__05D8E0BE FOREIGN KEY (RequestedById) REFERENCES TuneVault.dbo.Users(Id)`
- `CONSTRAINT FK__Friends__Request__06CD04F7 FOREIGN KEY (RequestedToId) REFERENCES TuneVault.dbo.Users(Id)`

## `MediaItems`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `OwnerId` | `varchar(10)` | NOT NULL | `` |
| `Title` | `nvarchar(255)` | NOT NULL | `` |
| `Description` | `nvarchar(1000)` | NULL | `` |
| `MediaType` | `tinyint` | NOT NULL | `` |
| `AudioUrl` | `varchar(500)` | NULL | `` |
| `VideoUrl` | `varchar(500)` | NULL | `` |
| `CoverImageUrl` | `varchar(500)` | NULL | `` |
| `CanvasUrl` | `varchar(500)` | NULL | `` |
| `Genre` | `nvarchar(100)` | NULL | `` |
| `DurationSeconds` | `int` | NOT NULL | `0` |
| `TrailerSeconds` | `int` | NOT NULL | `0` |
| `AccessLevel` | `tinyint` | NOT NULL | `0` |
| `IsPublic` | `bit` | NOT NULL | `1` |
| `FavoriteCount` | `int` | NOT NULL | `0` |
| `ViewCount` | `int` | NOT NULL | `0` |
| `UploadedAt` | `datetime2` | NOT NULL | `` |
| `ReleaseDate` | `datetime2` | NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |
| `IsValid` | `bit` | NOT NULL | `0` |
| `Url` | `varchar(500)` | NOT NULL | `` |
| `DurationMinutes` | `int` | NOT NULL | `0` |
| `TrailerMinutes` | `int` | NOT NULL | `0` |

**Constraints**

- `CONSTRAINT PK__MediaIte__3214EC0789C51905 PRIMARY KEY (Id)`
- `CONSTRAINT FK__MediaItem__Owner__60A75C0F FOREIGN KEY (OwnerId) REFERENCES TuneVault.dbo.Users(Id)`

**Indexes**

- `IX_MediaItems_Genre` on `Genre ASC`
- `IX_MediaItems_OwnerId` on `OwnerId ASC`
- `IX_MediaItems_Url` on `Url ASC`

## `MediaShares`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `SenderId` | `varchar(10)` | NOT NULL | `` |
| `ReceiverId` | `varchar(10)` | NOT NULL | `` |
| `SharedItemId` | `varchar(10)` | NOT NULL | `` |
| `ShareType` | `tinyint` | NOT NULL | `` |
| `Message` | `nvarchar(500)` | NULL | `` |
| `SharedAt` | `datetime2` | NOT NULL | `` |

**Constraints**

- `CONSTRAINT PK__MediaSha__3214EC079D9959F1 PRIMARY KEY (Id)`
- `CONSTRAINT FK__MediaShar__Recei__0B91BA14 FOREIGN KEY (ReceiverId) REFERENCES TuneVault.dbo.Users(Id)`
- `CONSTRAINT FK__MediaShar__Sende__0A9D95DB FOREIGN KEY (SenderId) REFERENCES TuneVault.dbo.Users(Id)`

## `Notifications`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `UserId` | `varchar(10)` | NOT NULL | `` |
| `NotifyType` | `tinyint` | NOT NULL | `` |
| `Title` | `nvarchar(200)` | NOT NULL | `` |
| `Message` | `nvarchar(500)` | NOT NULL | `` |
| `IsRead` | `bit` | NOT NULL | `0` |
| `CreatedAt` | `datetime2` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |
| `SenderId` | `varchar(10)` | NULL | `` |
| `TargetType` | `tinyint` | NULL | `` |
| `TargetId` | `varchar(10)` | NULL | `` |

**Constraints**

- `CONSTRAINT PK__Notifica__3214EC07CD589234 PRIMARY KEY (Id)`
- `CONSTRAINT FK__Notificat__UserI__0E6E26BF FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)`

## `PlayHistory`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `UserId` | `varchar(10)` | NOT NULL | `` |
| `MediaItemId` | `varchar(10)` | NOT NULL | `` |
| `HistoryOrder` | `int` | NOT NULL | `` |
| `StoppedAt` | `datetime2` | NULL | `` |

**Constraints**

- `CONSTRAINT PK__PlayHist__3214EC076FBC4541 PRIMARY KEY (Id)`
- `CONSTRAINT FK__PlayHisto__Media__1332DBDC FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id)`
- `CONSTRAINT FK__PlayHisto__UserI__123EB7A3 FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)`

**Indexes**

- `IX_PlayHistory_UserId` on `UserId ASC`

## `Playlists`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `UserId` | `varchar(10)` | NOT NULL | `` |
| `Title` | `nvarchar(255)` | NOT NULL | `` |
| `Description` | `nvarchar(500)` | NULL | `` |
| `CoverImageUrl` | `varchar(500)` | NULL | `` |
| `IsPublic` | `bit` | NOT NULL | `1` |
| `ReleaseDate` | `datetime2` | NULL | `` |
| `ContentType` | `tinyint` | NULL | `` |
| `CreatedAt` | `datetime2` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |

**Constraints**

- `CONSTRAINT PK__Playlist__3214EC0783895B2B PRIMARY KEY (Id)`
- `CONSTRAINT FK__Playlists__UserI__74AE54BC FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)`

## `UserAccountTiers`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `UserId` | `varchar(10)` | NOT NULL | `` |
| `TierId` | `varchar(10)` | NOT NULL | `` |
| `PriceAmount` | `decimal(10,2)` | NOT NULL | `` |
| `PriceCurrency` | `varchar(5)` | NOT NULL | `'USD'` |
| `PurchasedAt` | `datetime2` | NOT NULL | `` |
| `ActivatedAt` | `datetime2` | NOT NULL | `` |
| `ExpiresAt` | `datetime2` | NOT NULL | `` |
| `IsActive` | `bit` | NOT NULL | `1` |

**Constraints**

- `CONSTRAINT PK__UserAcco__3214EC073ABA82B7 PRIMARY KEY (Id)`
- `CONSTRAINT FK__UserAccou__TierI__5BE2A6F2 FOREIGN KEY (TierId) REFERENCES TuneVault.dbo.AccountTiers(Id)`
- `CONSTRAINT FK__UserAccou__UserI__5AEE82B9 FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)`

## `AlbumTracks`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `AlbumId` | `varchar(10)` | NOT NULL | `` |
| `MediaItemId` | `varchar(10)` | NOT NULL | `` |
| `TrackOrder` | `int` | NOT NULL | `` |
| `AddedAt` | `datetime2` | NOT NULL | `` |

**Constraints**

- `CONSTRAINT PK__AlbumTra__3214EC0789C8D90A PRIMARY KEY (Id)`
- `CONSTRAINT FK__AlbumTrac__Album__70DDC3D8 FOREIGN KEY (AlbumId) REFERENCES TuneVault.dbo.Albums(Id)`
- `CONSTRAINT FK__AlbumTrac__Media__71D1E811 FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id)`

## `Favorites`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `UserId` | `varchar(10)` | NOT NULL | `` |
| `MediaItemId` | `varchar(10)` | NOT NULL | `` |
| `Reaction` | `tinyint` | NOT NULL | `1` |
| `LikedAt` | `datetime2` | NOT NULL | `` |

**Constraints**

- `CONSTRAINT PK__Favorite__3214EC0704049E32 PRIMARY KEY (Id)`
- `CONSTRAINT FK__Favorites__Media__7D439ABD FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id)`
- `CONSTRAINT FK__Favorites__UserI__7C4F7684 FOREIGN KEY (UserId) REFERENCES TuneVault.dbo.Users(Id)`

**Indexes**

- `IX_Favorites_UserId` on `UserId ASC`

## `MediaArtists`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `MediaItemId` | `varchar(10)` | NOT NULL | `` |
| `ArtistId` | `varchar(10)` | NOT NULL | `` |
| `Role` | `nvarchar(100)` | NOT NULL | `'MainArtist'` |

**Constraints**

- `CONSTRAINT PK_MediaArtists PRIMARY KEY (MediaItemId,ArtistId)`
- `CONSTRAINT FK__MediaArti__Artis__6A30C649 FOREIGN KEY (ArtistId) REFERENCES TuneVault.dbo.Users(Id)`
- `CONSTRAINT FK__MediaArti__Media__693CA210 FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id)`

## `PlaylistTracks`

| Column | Type | Nullability | Default |
|---|---|---|---|
| `Id` | `varchar(10)` | NOT NULL | `` |
| `PlaylistId` | `varchar(10)` | NOT NULL | `` |
| `MediaItemId` | `varchar(10)` | NOT NULL | `` |
| `TrackOrder` | `int` | NOT NULL | `` |
| `AddedAt` | `datetime2` | NOT NULL | `` |

**Constraints**

- `CONSTRAINT PK__Playlist__3214EC075859AE87 PRIMARY KEY (Id)`
- `CONSTRAINT FK__PlaylistT__Media__797309D9 FOREIGN KEY (MediaItemId) REFERENCES TuneVault.dbo.MediaItems(Id)`
- `CONSTRAINT FK__PlaylistT__Playl__787EE5A0 FOREIGN KEY (PlaylistId) REFERENCES TuneVault.dbo.Playlists(Id)`

## Quy ước quan trọng cho AI Agent

- Dùng đúng tên bảng số nhiều đang có trong SQL: `Users`, `MediaItems`, `Playlists`, `PlaylistTracks`, `Favorites`, `Notifications`, `Follows`.
- Không tự đổi thành số ít như `User`, `MediaItem`, `PlaylistTrack` trong SQL query. Entity C# có thể là số ít, nhưng table name phải khớp database.
- Khóa chính hầu hết là `Id varchar(10)`, riêng `MediaArtists` dùng khóa kép `(MediaItemId, ArtistId)`.
- Dữ liệu media hiện đang có nhiều cột đường dẫn: `AudioUrl`, `VideoUrl`, `CoverImageUrl`, `CanvasUrl`, `Url`. Khi làm streaming phải xác nhận quy ước dùng cột nào trước khi sửa code.
- `Follows` có `IsActive`, dùng soft delete. `Favorites` hiện **chưa có `IsActive`** trong SQL, không được tự viết query lọc `IsActive = 1` cho bảng này nếu chưa thêm cột.
- `PlayHistory` hiện có `HistoryOrder`, `StoppedAt`, nhưng chưa có `PlayedAt`. Không được tự dùng `PlayedAt` nếu SQL chưa cập nhật.
