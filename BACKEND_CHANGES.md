# Ghi chú thay đổi Backend — TuneVault

> Tài liệu này ghi lại toàn bộ các thay đổi đã thực hiện ở phía backend (ASP.NET Core / C#) trong đợt cập nhật này.
> Ngày cập nhật: 2026-06-25

---

## 1. SearchRepository.cs

**Đường dẫn:** `backend/src/TuneVault.Infrastructure/Repositories/SearchRepository.cs`

### Vấn đề trước đây
Hàm `SearchMediaAsync` chỉ tìm kiếm bài hát theo cột `m.Title`. Nếu người dùng gõ tên nghệ sĩ (ví dụ: "Sơn Tùng"), kết quả trống vì không JOIN bảng `Users` để kiểm tra `DisplayName`.

### Thay đổi thực hiện
Cập nhật câu SQL trong `SearchMediaAsync` để:
- **JOIN thêm hai bảng:** `MediaArtists ma` và `Users u` (LEFT JOIN theo `ma.MediaItemId = m.Id` và `ma.ArtistId = u.Id`).
- **Mở rộng điều kiện WHERE:** Từ `m.Title LIKE @Keyword` thành `(m.Title LIKE @Keyword OR u.DisplayName LIKE @Keyword)`.

```sql
-- TRƯỚC:
WHERE m.IsPublic = 1
  AND m.IsActive = 1
  AND m.IsValid = 0
  AND m.Title LIKE @Keyword

-- SAU:
FROM MediaItems m
LEFT JOIN MediaArtists ma ON m.Id = ma.MediaItemId
LEFT JOIN Users u ON ma.ArtistId = u.Id
WHERE m.IsPublic = 1
  AND m.IsActive = 1
  AND m.IsValid = 0
  AND (m.Title LIKE @Keyword OR u.DisplayName LIKE @Keyword)
```

### Lý do
Cho phép người dùng tìm kiếm bài hát bằng cách gõ tên nghệ sĩ thay vì chỉ tìm được qua tiêu đề bài hát.

---

## 2. MediaItemDto.cs — Record `MediaArtistDto`

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Media/DTOs/MediaItemDto.cs`

### Thay đổi thực hiện
Bổ sung thêm trường `ArtistName` vào record `MediaArtistDto`:

```csharp
// TRƯỚC:
public record MediaArtistDto(
    string ArtistId,
    string Role
);

// SAU:
public record MediaArtistDto(
    string ArtistId,
    string Role,
    string? ArtistName = null   // <-- thêm mới
);
```

### Lý do
Frontend cần tên hiển thị của nghệ sĩ (`DisplayName`) để render đúng ở PlayerBar / NowPlayingView, thay vì hiển thị ID hoặc "TuneVault".

---

## 3. Domain Model — Class `MediaArtist`

**Đường dẫn:** `backend/src/TuneVault.Domain/Entities/MediaArtist.cs` (hoặc tương đương)

### Thay đổi thực hiện
Thêm thuộc tính `ArtistName` vào entity `MediaArtist`:

```csharp
// THÊM MỚI:
public string? ArtistName { get; set; }
```

### Lý do
Cho phép repository map trực tiếp `DisplayName` từ bảng `Users` vào entity khi JOIN, trước khi chuyển thành DTO.

---

## 4. MediaRepository.cs — Hàm `GetArtistsByMediaIdAsync`

**Đường dẫn:** `backend/src/TuneVault.Infrastructure/Repositories/MediaRepository.cs`

### Thay đổi thực hiện
Cập nhật câu SQL để JOIN bảng `Users` và chọn thêm `u.DisplayName AS ArtistName`:

```sql
-- TRƯỚC:
SELECT ma.ArtistId, ma.Role
FROM MediaArtists ma
WHERE ma.MediaItemId = @MediaItemId

-- SAU:
SELECT ma.ArtistId, ma.Role, u.DisplayName AS ArtistName
FROM MediaArtists ma
LEFT JOIN Users u ON ma.ArtistId = u.Id
WHERE ma.MediaItemId = @MediaItemId
```

Cập nhật phần map kết quả (Dapper dynamic object → `MediaArtist`):
```csharp
ArtistName = row.ArtistName ?? null
```

### Lý do
Trả về tên hiển thị thực tế của nghệ sĩ trong mọi truy vấn liên quan đến media.

---

## 5. Các QueryHandler & Mapper — Cập nhật khởi tạo `MediaArtistDto`

Tất cả các vị trí `new MediaArtistDto(...)` trong codebase đã được cập nhật để truyền thêm tham số `ArtistName`.

### 5a. MediaDtoMapper.cs

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Media/Mappers/MediaDtoMapper.cs`

```csharp
// TRƯỚC:
artists.Select(a => new MediaArtistDto(a.ArtistId, a.Role))

// SAU:
artists.Select(a => new MediaArtistDto(a.ArtistId, a.Role, a.ArtistName))
```

---

### 5b. GetMediaByIdQueryHandler.cs

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Media/Queries/GetMediaById/`

```csharp
// TRƯỚC:
new MediaArtistDto(artist.ArtistId, artist.Role)

// SAU:
new MediaArtistDto(artist.ArtistId, artist.Role, artist.ArtistName)
```

---

### 5c. UploadMediaCommandHandler.cs

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Media/Commands/UploadMedia/`

```csharp
// TRƯỚC:
new MediaArtistDto(a.ArtistId, a.Role)

// SAU:
new MediaArtistDto(a.ArtistId, a.Role, a.ArtistName)
```

---

### 5d. UpdateMediaCommandHandler.cs

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Media/Commands/UpdateMedia/`

```csharp
// TRƯỚC:
new MediaArtistDto(a.ArtistId, a.Role)

// SAU:
new MediaArtistDto(a.ArtistId, a.Role, a.ArtistName)
```

---

### 5e. GetRecentHistoryQueryHandler.cs

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Media/Queries/GetRecentHistory/`

```csharp
// TRƯỚC:
new MediaArtistDto(a.ArtistId, a.Role)

// SAU:
new MediaArtistDto(a.ArtistId, a.Role, a.ArtistName)
```

---

## 6. UserDto — Thêm trường `Id`

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Users/DTOs/UserDto.cs`

### Thay đổi thực hiện
Bổ sung thuộc tính `Id` (database primary key) vào DTO:

```csharp
// TRƯỚC:
public class UserDto
{
    public string IdDisplay { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    // ...các trường khác
}

// SAU:
public class UserDto
{
    public string Id { get; init; } = string.Empty;   // <-- thêm mới
    public string IdDisplay { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    // ...các trường khác
}
```

### Lý do
Frontend (`ProfileView.jsx`) cần biết ID thực của user (ví dụ: `U001`) để gọi API `/api/media/artist/{id}` lấy danh sách bài hát/video công khai của nghệ sĩ đó. Trước đây, DTO không trả về `Id`, khiến profile công khai của nghệ sĩ bị trống hoàn toàn.

---

## 7. UserPublicDetailDto — Thêm trường `Id`

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Users/DTOs/UserPublicDetailDto.cs`

### Thay đổi thực hiện

```csharp
// THÊM MỚI:
public string Id { get; init; } = string.Empty;
```

### Lý do
Tương tự `UserDto`, cần trả về `Id` trong trường hợp xem trang profile công khai của nghệ sĩ từ kết quả tìm kiếm hoặc từ kết quả liệt kê nghệ sĩ.

---

## 8. GetAllArtistsQueryHandler.cs — Gán `Id` khi tạo DTO

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Users/Queries/GetAllArtists/`

### Thay đổi thực hiện
Bổ sung gán `Id = user.Id` khi khởi tạo DTO trả về:

```csharp
// TRƯỚC:
new UserDto
{
    IdDisplay = user.IdDisplay,
    DisplayName = user.DisplayName,
    // ...
}

// SAU:
new UserDto
{
    Id = user.Id,           // <-- thêm mới
    IdDisplay = user.IdDisplay,
    DisplayName = user.DisplayName,
    // ...
}
```

---

## 9. GetUserByIdDisplayQueryHandler.cs — Gán `Id` khi tạo DTO

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Users/Queries/GetUserByIdDisplay/`

### Thay đổi thực hiện

```csharp
// THÊM:
Id = user.Id,
```

---

## 10. GetUserByIdQueryHandler.cs — Gán `Id` khi tạo DTO

**Đường dẫn:** `backend/src/TuneVault.Application/Features/Users/Queries/GetUserById/`

### Thay đổi thực hiện

```csharp
// THÊM:
Id = user.Id,
```

---

## Tóm tắt nhanh (Quick Reference)

| # | File | Loại thay đổi | Mục đích |
|---|------|--------------|----------|
| 1 | `SearchRepository.cs` | Sửa SQL — thêm JOIN + OR condition | Tìm kiếm bài hát theo tên nghệ sĩ |
| 2 | `MediaItemDto.cs` | Thêm field `ArtistName` vào record | Trả về tên nghệ sĩ cho frontend |
| 3 | `MediaArtist.cs` | Thêm property `ArtistName` | Domain model hỗ trợ field mới |
| 4 | `MediaRepository.cs` | Sửa SQL — JOIN Users, SELECT DisplayName | Lấy tên nghệ sĩ từ DB khi truy vấn |
| 5a | `MediaDtoMapper.cs` | Truyền `ArtistName` vào constructor | Map đúng DTO |
| 5b | `GetMediaByIdQueryHandler.cs` | Truyền `ArtistName` vào constructor | Map đúng DTO |
| 5c | `UploadMediaCommandHandler.cs` | Truyền `ArtistName` vào constructor | Map đúng DTO |
| 5d | `UpdateMediaCommandHandler.cs` | Truyền `ArtistName` vào constructor | Map đúng DTO |
| 5e | `GetRecentHistoryQueryHandler.cs` | Truyền `ArtistName` vào constructor | Map đúng DTO |
| 6 | `UserDto.cs` | Thêm field `Id` | Frontend gọi API lấy media của nghệ sĩ |
| 7 | `UserPublicDetailDto.cs` | Thêm field `Id` | Frontend gọi API lấy media của nghệ sĩ |
| 8 | `GetAllArtistsQueryHandler.cs` | Gán `Id` trong DTO | Trả về ID cho frontend |
| 9 | `GetUserByIdDisplayQueryHandler.cs` | Gán `Id` trong DTO | Trả về ID cho frontend |
| 10 | `GetUserByIdQueryHandler.cs` | Gán `Id` trong DTO | Trả về ID cho frontend |

---

## Lưu ý quan trọng

> **Không có thay đổi về schema cơ sở dữ liệu.** Tất cả các thay đổi chỉ là thêm JOIN trong truy vấn SQL và bổ sung trường vào DTO/domain model. Không cần migration.

> **Backward compatible:** Các field mới (`ArtistName`, `Id`) đều có giá trị mặc định (`null` hoặc `string.Empty`), không phá vỡ contract cũ với các client chưa sử dụng field này.
