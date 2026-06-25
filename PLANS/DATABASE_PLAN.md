# DATABASE_PLAN.md — Kế hoạch đồng bộ Database với Code TuneVault

> Dùng file này khi task có liên quan đến Dapper, Repository, SQL query, DTO mapping, endpoint đọc/ghi database, hoặc khi lỗi runtime đến từ tên bảng/cột không khớp.

## 1. Mục tiêu

Giữ cho 4 thứ luôn đồng bộ:

```text
DATABASE_SCHEMA.md
↓
Domain Entity / DTO
↓
Dapper Repository SQL
↓
API Response Contract / Frontend TypeScript type
```

Vì project dùng Dapper nên AI Agent không có migration model để tự suy luận. Mọi tên bảng/cột phải được kiểm tra với `DATABASE_SCHEMA.md` trước khi code.

## 2. Khi nào phải đọc file này

AI phải đọc `PLANS/DATABASE_PLAN.md` khi làm các việc sau:

- Tạo hoặc sửa Repository Dapper.
- Viết query `SELECT`, `INSERT`, `UPDATE`, `DELETE`.
- Tạo Entity, DTO, Command, Query liên quan database.
- Sửa lỗi `Invalid column name`, `Invalid object name`, FK/PK conflict.
- Làm Auth, Media, Playlist, Favorite, History, Share, Notification, Album.
- Chuẩn hóa response endpoint có dữ liệu lấy từ database.
- Làm streaming audio/video vì cần xác định cột file path/url.

## 3. Nguyên tắc nguồn sự thật

Thứ tự ưu tiên khi có mâu thuẫn:

1. SQL database thật / `DATABASE_SCHEMA.md`.
2. Code đang chạy được trong repository.
3. `AGENTS.md` và `PLANS/*.md`.
4. Yêu cầu mới nhất của developer.

Nếu SQL và code đang khác nhau, AI không được tự chọn một bên. Phải báo rõ:

```text
Mình thấy code đang dùng cột X nhưng DATABASE_SCHEMA.md chỉ có cột Y.
Bạn muốn sửa code theo database hiện tại hay cập nhật SQL schema?
```

## 4. Luật Dapper bắt buộc

### Được phép

```csharp
const string sql = """
    SELECT Id, DisplayName, Email
    FROM TuneVault.dbo.Users
    WHERE Id = @Id AND IsActive = 1
""";

var user = await connection.QueryFirstOrDefaultAsync<UserDto>(
    new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
```

### Không được phép

```csharp
var sql = $"SELECT * FROM Users WHERE Id = '{id}'";
```

## 5. Checklist trước khi viết Repository

- [ ] Đã mở `DATABASE_SCHEMA.md`.
- [ ] Đã xác nhận table name đúng.
- [ ] Đã xác nhận column name đúng.
- [ ] Không dùng `SELECT *` cho DTO response.
- [ ] Query write có transaction nếu có nhiều bước hoặc ảnh hưởng nhiều bảng.
- [ ] Có `CancellationToken` trong method.
- [ ] Có parameterized query, không string interpolation.
- [ ] DTO không lộ `PasswordHash`, secret hoặc field nội bộ không cần thiết.

## 6. Quy trình thêm feature có database

### Bước 1 — Kiểm tra schema

Trước khi code, AI phải xác định:

- Bảng đã tồn tại chưa?
- Cột cần dùng đã tồn tại chưa?
- Có FK/index cần lưu ý không?
- Có soft delete không?

### Bước 2 — Nếu thiếu bảng/cột

Vì developer muốn tự tạo SQL bằng tay, AI không tự sửa schema nếu chưa được yêu cầu.

AI phải hỏi:

```text
Feature này cần cột/bảng chưa có trong DATABASE_SCHEMA.md.
Bạn muốn mình đề xuất script SQL hay bạn sẽ tự tạo thủ công trước?
```

### Bước 3 — Viết code theo schema thật

- Domain: Entity / interface nếu cần.
- Application: Command/Query/DTO/Validator/Handler.
- Infrastructure: Dapper Repository.
- API: Controller chỉ gọi MediatR.

### Bước 4 — Đồng bộ tài liệu

Nếu developer xác nhận schema đã đổi, cập nhật:

- `DATABASE_SCHEMA.md`
- `API_CONTRACT.md` nếu response thay đổi
- `CURRENT_STATUS.md` nếu feature hoàn thành/chưa hoàn thành

## 7. Các điểm cần chú ý từ SQL hiện tại

### 7.1 Media streaming

Bảng `MediaItems` hiện có các cột:

- `AudioUrl`
- `VideoUrl`
- `CoverImageUrl`
- `CanvasUrl`
- `Url`
- `DurationSeconds`
- `DurationMinutes`
- `TrailerSeconds`
- `TrailerMinutes`

Trước khi code streaming, AI phải hỏi hoặc xác nhận quy ước:

```text
Audio stream dùng AudioUrl hay Url?
Video stream dùng VideoUrl hay Url?
Poster dùng CoverImageUrl đúng không?
```

Không được tự chọn nếu code hiện tại không thể hiện rõ.

### 7.2 Play history

Bảng `PlayHistory` hiện có:

- `HistoryOrder`
- `StoppedAt`

Chưa thấy `PlayedAt`. Nếu muốn lưu thời điểm phát nhạc, cần xác nhận schema trước.

### 7.3 Favorite

Bảng `Favorites` hiện không có `IsActive`. Nếu cần toggle favorite mềm, phải hỏi trước khi code theo hướng soft delete.

### 7.4 Share

`MediaShares.SharedItemId` không có FK trực tiếp đến `MediaItems` hoặc `Playlists` vì còn phụ thuộc `ShareType`. Khi query phải xử lý cẩn thận theo `ShareType`.

### 7.5 Notification

`Notifications` có `SenderId` nhưng trong SQL hiện tại không thấy FK cho `SenderId`. Không được giả định FK nếu chưa có trong schema.

## 8. Database sync commands trên Linux Mint

Các lệnh tham khảo, chỉ chạy khi developer yêu cầu:

```bash
# Kiểm tra container SQL Server
docker ps

# Vào sqlcmd trong container nếu image có tool
sudo docker exec -it <sqlserver-container-name> /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<SA_PASSWORD>' -C
```

Không hardcode password thật vào code, docs hoặc commit.

## 9. Definition of Done cho task database

Một task liên quan database chỉ xong khi:

- [ ] Query khớp `DATABASE_SCHEMA.md`.
- [ ] Không có string interpolation trong SQL.
- [ ] Không trả entity thô ra API.
- [ ] Response dùng `{ success, message, data }`.
- [ ] Error message tiếng Việt, không lộ lỗi SQL Server/Dapper.
- [ ] Build backend pass.
- [ ] Nếu schema thay đổi, tài liệu schema đã được cập nhật.
