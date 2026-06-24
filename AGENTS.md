# AGENTS.md — TuneVault Coding Agent Guide

> **Mục đích:** File này là hướng dẫn bắt buộc cho mọi AI coding agent (Codex, Cline, Cursor, ChatGPT, Claude, Copilot Agent, v.v.) khi làm việc trên project **TuneVault**.
>
> Agent phải đọc toàn bộ file này **trước khi phân tích, sửa, refactor hoặc tạo code**.
>
> Project hiện tại ưu tiên **Backend ASP.NET Core trước**. Frontend chỉ sửa khi thật sự cần để khớp contract API hoặc để kiểm chứng luồng backend.

---

## Mục lục

1. [Agent Mindset](#1-agent-mindset)
2. [Bối cảnh dự án hiện tại](#2-bối-cảnh-dự-án-hiện-tại)
3. [Môi trường phát triển](#3-môi-trường-phát-triển)
4. [Yêu cầu dự án từ đề bài môn học](#4-yêu-cầu-dự-án-từ-đề-bài-môn-học)
5. [Công nghệ sử dụng](#5-công-nghệ-sử-dụng)
6. [Kiến trúc bắt buộc](#6-kiến-trúc-bắt-buộc)
7. [Quy trình làm việc bắt buộc](#7-quy-trình-làm-việc-bắt-buộc)
8. [Quy trình hỏi lại khi thiếu thông tin](#8-quy-trình-hỏi-lại-khi-thiếu-thông-tin)
9. [Build, kiểm tra cú pháp và xác minh](#9-build-kiểm-tra-cú-pháp-và-xác-minh)
10. [Chuẩn response API](#10-chuẩn-response-api)
11. [Chuẩn error handling](#11-chuẩn-error-handling)
12. [Luật code bắt buộc](#12-luật-code-bắt-buộc)
13. [Phong cách comment và XML documentation](#13-phong-cách-comment-và-xml-documentation)
14. [Luật về Dapper và Database](#14-luật-về-dapper-và-database)
15. [Luật bảo mật](#15-luật-bảo-mật)
16. [Media streaming roadmap](#16-media-streaming-roadmap)
17. [Frontend policy](#17-frontend-policy)
18. [Refactoring policy](#18-refactoring-policy)
19. [Definition of Done](#19-definition-of-done)
20. [Build failed protocol](#20-build-failed-protocol)
21. [Checklist trước khi kết thúc task](#21-checklist-trước-khi-kết-thúc-task)

---

## 1. Agent Mindset

Agent phải làm việc như một **software engineer hỗ trợ người sửa code cuối cùng**, không phải như một công cụ generate code nhanh.

Ưu tiên theo thứ tự:

1. Đúng yêu cầu.
2. Không phá code đang chạy.
3. Giữ kiến trúc Clean Architecture.
4. Chuẩn hóa response và error message.
5. Code dễ đọc, dễ sửa, dễ bảo trì.
6. Không tự bịa dữ liệu còn thiếu.
7. Không refactor lan rộng nếu task không yêu cầu.

Agent **không được tối ưu cho việc viết nhiều code**. Agent phải tối ưu cho việc giúp project ổn định hơn.

Khi không chắc:

```text
Dừng lại → giải thích phần chưa rõ → hỏi lại người dùng.
```

Không đoán.
Không tự sáng tác business rule.
Không tự thêm database schema.
Không tự thay đổi kiến trúc lớn.

---

## 2. Bối cảnh dự án hiện tại

### 2.1 Tổng quan

**TuneVault** là nền tảng phát nhạc và video trực tuyến, tham khảo Spotify, được xây dựng với:

- Backend: ASP.NET Core Web API.
- Architecture: Clean Architecture.
- Data access: Dapper.
- Database: SQL Server.
- Frontend: React/Vite, hiện đã có nhưng chưa ổn định.

### 2.2 Trạng thái thực tế cần lưu ý

Project hiện tại có một số vấn đề cần agent luôn ghi nhớ:

- Nhiều người cùng code nên style chưa thống nhất.
- Response của các endpoint chưa hoàn toàn thống nhất.
- Một số endpoint đang trả anonymous object thay vì `ApiResponse<T>`.
- Một số lỗi có nguy cơ trả message hệ thống trực tiếp ra API.
- Frontend đã có nhưng chưa ổn định.
- Media upload đã có một phần nhưng streaming audio/video/poster xuống frontend chưa hoàn chỉnh.
- Người dùng hiện là người sửa code cuối cùng, nên agent phải ưu tiên cách sửa an toàn, dễ review.

### 2.3 Ưu tiên hiện tại

Project ưu tiên:

```text
Backend ASP.NET Core trước.
Frontend chỉ sửa khi cần.
```

Thứ tự ưu tiên backend nên theo hướng:

1. Chuẩn hóa response API.
2. Chuẩn hóa error handling.
3. Hoàn thiện Auth/JWT/Authorization nếu còn thiếu.
4. Hoàn thiện Media upload metadata.
5. Hoàn thiện audio streaming.
6. Hoàn thiện video streaming.
7. Hoàn thiện poster/thumbnail.
8. Hoàn thiện Playlist/Favorite/History.
9. Hoàn thiện Share/Notification/SignalR.
10. Sau cùng mới tính AI feature, deploy, Docker pipeline.

---

## 3. Môi trường phát triển

### 3.1 Hệ điều hành chính

Người dùng phát triển trên:

```text
Linux Mint
```

Agent phải dùng lệnh phù hợp với Linux/bash.

### 3.2 Quy tắc shell command

Được dùng:

```bash
dotnet restore
dotnet build
dotnet test
npm install
npm run dev
npm run build
```

Không được dùng trừ khi người dùng yêu cầu:

```powershell
PowerShell commands
```

Không tạo:

```text
.bat files
Windows-only scripts
PowerShell-only scripts
```

Dùng đường dẫn Linux:

```text
src/TuneVault.API/Program.cs
```

Không dùng đường dẫn Windows:

```text
src\TuneVault.API\Program.cs
```

### 3.3 Database local

Database chính là:

```text
SQL Server chạy bằng Docker
```

Agent không được mặc định người dùng đang dùng SQL Server LocalDB của Windows.

Nếu cần hướng dẫn chạy DB, dùng hướng Docker/Linux. Ví dụ:

```bash
docker ps
docker compose ps
docker compose up -d sqlserver
```

Tuy nhiên, agent **không tự sửa Docker config** nếu task không liên quan deploy hoặc database environment.

---

## 4. Yêu cầu dự án từ đề bài môn học

### 4.1 Thông tin chung

| Mục | Chi tiết |
|---|---|
| Môn học | C# and .NET Development |
| Tên bài | Media Streaming Web Application — TuneVault |
| Tổng điểm | 10 điểm |
| Backend | 8 điểm |
| Frontend | 2 điểm |
| Deadline | 23:59, ngày 20/06/2026 |

### 4.2 Mười chức năng bắt buộc

| # | Chức năng | Mô tả |
|---|---|---|
| 1 | Xác thực | Đăng ký, đăng nhập, đăng xuất, JWT |
| 2 | Hồ sơ người dùng | Xem/sửa profile, avatar, bio |
| 3 | Thư viện Media | Upload audio/video, metadata, phân loại |
| 4 | Audio Player | Stream audio, pause, seek, queue, history |
| 5 | Video Player | Stream video, poster thumbnail, Range request |
| 6 | Playlist | CRUD playlist, thêm/xóa track, public/private |
| 7 | Tìm kiếm & Khám phá | Search theo tên/nghệ sĩ/playlist, trending |
| 8 | Chia sẻ Media | Gửi bài/playlist cho user khác |
| 9 | Thông báo | SignalR real-time + lưu DB + mark as read |
| 10 | Tương tác & Lịch sử | Favorite, play history 10 bài gần nhất |

> Share và Notification/SignalR là phần được chấm kỹ. Không được làm sơ sài hoặc fake implementation.

### 4.3 Backend rubric

| Mã | Tiêu chí | Điểm |
|---|---|---|
| B1 | Clean Architecture đúng 4 layer | 1.0 |
| B2 | Dapper, repository interface, parameterized query, transaction | 1.5 |
| B3 | ≥20 endpoint có ý nghĩa, DTO, Swagger/Postman | 1.0 |
| B4 | JWT Auth, `[Authorize]`, ownership check | 1.0 |
| B5 | Upload multipart, stream audio/video, Range header | 1.0 |
| B6 | Share media API, shared with/by me | 1.0 |
| B7 | SignalR Hub + Notification entity + mark as read | 0.5 |
| B8 | Pipeline đủ cho các chức năng | Điều kiện chất lượng |

---

## 5. Công nghệ sử dụng

### 5.1 Backend

| Công nghệ | Ghi chú |
|---|---|
| .NET / ASP.NET Core | Web API, minimal hosting model |
| Dapper | Data access chính, không dùng EF Core |
| SQL Server | Chạy bằng Docker trên Linux Mint |
| MediatR | CQRS, handler, pipeline behavior |
| FluentValidation | Validate command/query/request |
| JWT Bearer | Authentication/Authorization |
| BCrypt.Net-Next | Hash password |
| SignalR | Real-time notifications |
| Swagger/Swashbuckle | API documentation |

### 5.2 Frontend

| Công nghệ | Ghi chú |
|---|---|
| React/Vite | Frontend hiện có nhưng chưa ổn định |
| TypeScript hoặc JavaScript | Tuân theo codebase hiện tại, không tự migrate lớn |
| Axios | Service layer khi cần gọi API |
| SignalR client | Dùng khi làm notifications |

### 5.3 Không được tự ý thay đổi

Không tự chuyển:

- Dapper sang EF Core.
- SQL Server sang MySQL/PostgreSQL.
- Clean Architecture sang kiến trúc khác.
- React/Vite sang framework khác.
- Response format sang format khác.

---

## 6. Kiến trúc bắt buộc

### 6.1 Solution structure

```text
TuneVault/
├── src/
│   ├── TuneVault.Domain
│   ├── TuneVault.Application
│   ├── TuneVault.Infrastructure
│   ├── TuneVault.API
│   └── TuneVault.sln
└── client
```

### 6.2 Dependency rule

Được phép:

```text
API            → Application, Domain
Application    → Domain
Infrastructure → Application, Domain
```

Không được:

```text
Domain         → Application / Infrastructure / API
Application    → Infrastructure
Controller     → Repository trực tiếp
Controller     → DbConnection trực tiếp
Controller     → SQL trực tiếp
```

### 6.3 Pipeline chuẩn cho feature backend

Mỗi feature nên đi theo flow:

```text
Controller
  → MediatR Command/Query
  → Validator
  → Handler
  → Repository interface
  → Repository implementation bằng Dapper
  → DTO response
  → ApiResponse<T>
```

Controller không xử lý business logic nặng. Controller chỉ:

- Nhận request.
- Lấy user id từ claims nếu cần.
- Gọi MediatR.
- Trả response chuẩn.

---

## 7. Quy trình làm việc bắt buộc

Khi nhận task, agent phải làm theo thứ tự:

### Bước 1 — Đọc ngữ cảnh

Đọc:

```text
AGENTS.md
```

Nếu sau này có file plan, đọc thêm:

```text
PLANS/MASTER_PLAN.md
PLANS/BACKEND_PLAN.md
PLANS/FRONTEND_PLAN.md
```

Nếu file plan chưa tồn tại thì không tự tạo, trừ khi người dùng yêu cầu.

### Bước 2 — Xác định phạm vi sửa

Trước khi code, xác định:

- Module nào đang được sửa.
- Layer nào bị ảnh hưởng.
- Có cần database schema không.
- Có cần frontend không.
- Có ảnh hưởng response format không.
- Có ảnh hưởng authentication/authorization không.

### Bước 3 — Lập kế hoạch ngắn

Với task vừa hoặc lớn, agent nên nêu kế hoạch trước:

```text
1. Kiểm tra controller hiện tại.
2. Kiểm tra command/query/handler.
3. Sửa response về ApiResponse<T>.
4. Chạy dotnet build.
```

Không viết code ngay nếu task còn mơ hồ.

### Bước 4 — Sửa tối thiểu cần thiết

Chỉ sửa đúng module/task đang làm.

Không rewrite toàn bộ project.
Không format lại hàng loạt file không liên quan.
Không đổi naming toàn project nếu không được yêu cầu.

### Bước 5 — Build và báo cáo

Sau khi sửa:

```bash
cd src
dotnet restore TuneVault.sln
dotnet build TuneVault.sln
```

Nếu có test:

```bash
dotnet test TuneVault.sln
```

Báo cáo:

- Đã sửa file nào.
- Sửa để làm gì.
- Build/test kết quả ra sao.
- Còn việc gì cần người dùng quyết định.

---

## 8. Quy trình hỏi lại khi thiếu thông tin

Agent tuyệt đối không được bịa khi thiếu dữ liệu.

### 8.1 Không được tự bịa

Không tự bịa:

- Tên bảng database.
- Tên cột database.
- Kiểu dữ liệu cột.
- Business rule.
- Endpoint contract.
- DTO field.
- Quyền user.
- Role mới.
- Error message nghiệp vụ chưa được thống nhất.
- File path lưu media.
- Cách sinh ID.
- Cách tính trending/recommendation.

### 8.2 Phải hỏi lại

Nếu thiếu thông tin, agent phải hỏi như một người tư vấn:

```text
Mình đang thiếu thông tin về bảng MediaItem: hiện database đã có cột AudioUrl, VideoUrl, CoverUrl chưa?
Bạn muốn mình chỉ sửa code C# trước hay cần bạn tạo SQL schema rồi mình mới nối repository?
```

Câu hỏi phải rõ, ngắn, có lựa chọn nếu được.

### 8.3 Được giả định khi nào?

Chỉ được giả định các chi tiết nhỏ, ít rủi ro, và phải ghi rõ:

```text
Mình giả định frontend đang gọi API qua Axios service hiện có. Nếu khác, cần chỉnh lại sau.
```

Không được giả định những thứ ảnh hưởng database, security, response contract hoặc điểm chấm.

---

## 9. Build, kiểm tra cú pháp và xác minh

### 9.1 Backend build

Luôn dùng lệnh Linux/bash:

```bash
cd src
dotnet restore TuneVault.sln
dotnet build TuneVault.sln
```

Nếu đang ở root project:

```bash
dotnet restore src/TuneVault.sln
dotnet build src/TuneVault.sln
```

### 9.2 Backend test

Nếu có test project:

```bash
dotnet test src/TuneVault.sln
```

Nếu chưa có test project, không được báo là test pass. Chỉ báo:

```text
Project hiện chưa có test project nên chỉ xác minh bằng dotnet build.
```

### 9.3 Kiểm tra cú pháp C#

`dotnet build` là bước kiểm tra cú pháp chính.

Nếu task chỉ sửa một project:

```bash
dotnet build src/TuneVault.API/TuneVault.API.csproj
dotnet build src/TuneVault.Application/TuneVault.Application.csproj
dotnet build src/TuneVault.Infrastructure/TuneVault.Infrastructure.csproj
dotnet build src/TuneVault.Domain/TuneVault.Domain.csproj
```

Tuy nhiên trước khi kết thúc task vẫn ưu tiên build solution:

```bash
dotnet build src/TuneVault.sln
```

### 9.4 Frontend build

Chỉ chạy khi có sửa frontend hoặc cần kiểm chứng contract:

```bash
cd client
npm install
npm run build
```

Nếu frontend chưa ổn định sẵn, không được tự nhận lỗi frontend là do thay đổi hiện tại nếu chưa kiểm tra kỹ.

### 9.5 Docker SQL Server check

Khi cần kiểm tra database container:

```bash
docker ps
docker compose ps
```

Không tự chạy destructive command như:

```bash
docker compose down -v
```

trừ khi người dùng cho phép rõ ràng.

---

## 10. Chuẩn response API

### 10.1 Response format bắt buộc

Mọi endpoint JSON phải trả theo format:

```json
{
  "success": true,
  "message": "Lấy dữ liệu thành công.",
  "data": {}
}
```

Khi lỗi:

```json
{
  "success": false,
  "message": "Không thể lấy dữ liệu.",
  "data": null
}
```

Nếu cần detail kỹ thuật trong môi trường Development:

```json
{
  "success": false,
  "message": "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.",
  "data": null,
  "detail": "Chi tiết lỗi chỉ dùng cho Development."
}
```

### 10.2 Kiểu C# chuẩn

Dùng `ApiResponse<T>` trong `TuneVault.Application.Common`.

Chuẩn mong muốn:

```csharp
return Ok(ApiResponse<UserDto>.Ok(user, "Lấy thông tin người dùng thành công."));
```

Khi lỗi:

```csharp
return BadRequest(ApiResponse<object?>.Fail("Dữ liệu gửi lên không hợp lệ."));
```

Không trả:

```csharp
return Ok(new { success = true, data = result });
```

trừ khi đang sửa tạm trong module cũ và task không cho phép refactor rộng. Nếu đang chạm vào endpoint đó, hãy chuẩn hóa luôn trong phạm vi endpoint đang sửa.

### 10.3 Message tiếng Việt

Message trả về API phải là tiếng Việt, dễ hiểu, dùng được cho frontend hiển thị.

Ví dụ tốt:

```text
Đăng nhập thành công.
Không tìm thấy bài hát.
Bạn không có quyền sửa playlist này.
File tải lên không hợp lệ.
```

Ví dụ không tốt:

```text
Object reference not set to an instance of an object.
Violation of PRIMARY KEY constraint.
SqlException: Invalid column name.
```

### 10.4 Streaming endpoint exception

Endpoint stream audio/video có thể trả `FileResult`, `PhysicalFile`, `FileStreamResult` thay vì `ApiResponse<T>` khi thành công, vì response chính là binary stream.

Tuy nhiên khi lỗi vẫn phải trả JSON chuẩn nếu có thể:

```json
{
  "success": false,
  "message": "Không tìm thấy file media.",
  "data": null
}
```

---

## 11. Chuẩn error handling

### 11.1 Không lộ lỗi hệ thống

Không bao giờ trả trực tiếp ra client:

- Stack trace.
- SQL exception.
- Dapper exception.
- Connection string.
- File path nội bộ.
- JWT secret.
- Exception message hệ thống chưa được xử lý.

Sai:

```json
{
  "message": "Violation of PRIMARY KEY constraint 'PK_MediaItem'..."
}
```

Đúng:

```json
{
  "success": false,
  "message": "Không thể tạo media. Vui lòng kiểm tra dữ liệu và thử lại.",
  "data": null
}
```

### 11.2 Mapping lỗi đề xuất

| Loại lỗi | HTTP status | Message tiếng Việt |
|---|---:|---|
| Validation error | 400 | Dữ liệu gửi lên không hợp lệ. |
| DomainException | 400 | Dùng message nghiệp vụ đã kiểm soát. |
| Unauthorized | 401 | Bạn cần đăng nhập để thực hiện thao tác này. |
| Forbidden | 403 | Bạn không có quyền thực hiện thao tác này. |
| Not found | 404 | Không tìm thấy dữ liệu yêu cầu. |
| Conflict | 409 | Dữ liệu đã tồn tại hoặc bị xung đột. |
| Unexpected exception | 500 | Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau. |

### 11.3 Exception middleware

Ưu tiên xử lý lỗi tập trung trong middleware hoặc `UseExceptionHandler`.

Controller không nên try/catch mọi lỗi nếu middleware đã xử lý được.

Controller chỉ nên catch khi:

- Cần map lỗi nghiệp vụ rất cụ thể.
- Cần trả status đặc biệt.
- Code cũ chưa được chuẩn hóa và đang refactor cục bộ.

---

## 12. Luật code bắt buộc

### 12.1 Naming convention

| Loại | Convention | Ví dụ |
|---|---|---|
| Class, record, enum | PascalCase | `MediaRepository` |
| Interface | Prefix `I` | `IMediaRepository` |
| Method | PascalCase | `GetByIdAsync` |
| Property | PascalCase | `DisplayName` |
| Private field | `_camelCase` | `_mediaRepository` |
| Local variable | camelCase | `mediaItem` |
| DTO | Suffix `Dto` | `UserProfileDto` |
| Command | Suffix `Command` | `UploadMediaCommand` |
| Query | Suffix `Query` | `GetMediaByIdQuery` |
| Handler | Suffix `Handler` | `UploadMediaCommandHandler` |
| Validator | Suffix `Validator` | `UploadMediaCommandValidator` |
| Controller | Suffix `Controller` | `MediaController` |
| Repository | Suffix `Repository` | `MediaRepository` |

### 12.2 Async convention

Async method phải có suffix `Async`, trừ method bắt buộc từ interface/framework không dùng suffix.

Ví dụ:

```csharp
Task<MediaItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
```

### 12.3 CancellationToken

Repository, service, handler nên nhận `CancellationToken` nếu có I/O operation.

Không bỏ qua token nếu method phía dưới hỗ trợ.

### 12.4 Không dùng magic string/magic number bừa bãi

Nếu giá trị là business rule, đưa vào constant hoặc options.

Ví dụ:

```csharp
private const int MaxBioLength = 300;
```

### 12.5 Không để TODO/FIXME/HACK trong code mới

Không kết thúc task với TODO trong code mới.

Nếu cần làm sau, ghi vào báo cáo cuối hoặc PLAN file khi người dùng yêu cầu.

---

## 13. Phong cách comment và XML documentation

### 13.1 XML summary bắt buộc

Mọi class, interface, record, enum, public method và method quan trọng phải có XML documentation.

Ví dụ:

```csharp
/// <summary>
/// Handler xử lý upload media mới cho nghệ sĩ.
/// Lưu metadata qua repository và dùng file storage service cho file vật lý.
/// </summary>
public sealed class UploadMediaCommandHandler
    : IRequestHandler<UploadMediaCommand, MediaItemDto>
{
}
```

### 13.2 Comment phải giống người viết code

Comment không được quá máy móc kiểu AI.

Tránh comment vô nghĩa:

```csharp
// Set user id
request.UserId = userId;

// Check if media is null
if (media is null)
{
}
```

Ưu tiên comment có lý do:

```csharp
// Unfollow dùng soft delete để người dùng có thể follow lại
// mà không làm mất lịch sử quan hệ trước đó.
```

```csharp
// Không trả path vật lý của file ra frontend vì đây là thông tin nội bộ server.
```

```csharp
// Range processing bắt buộc để trình phát có thể seek tới đoạn khác của audio/video.
```

### 13.3 Comment nên giải thích “vì sao”, không giải thích “đang làm gì”

Code đã nói nó đang làm gì. Comment nên giải thích:

- Vì sao cần check này.
- Business rule nằm ở đâu.
- Ràng buộc frontend/backend.
- Quyết định kiến trúc.
- Cạm bẫy dễ sửa sai.

### 13.4 Không lạm dụng comment

Không comment từng dòng.

Code rõ ràng thì không cần comment.

---

## 14. Luật về Dapper và Database

### 14.1 Dapper là bắt buộc

Project dùng Dapper. Không thêm EF Core.

Không dùng:

```csharp
DbContext
DbSet<T>
Migration EF Core
```

### 14.2 SQL phải parameterized

Đúng:

```csharp
const string sql = "SELECT * FROM MediaItems WHERE OwnerId = @OwnerId";
var result = await connection.QueryAsync<MediaItem>(sql, new { OwnerId = ownerId });
```

Sai:

```csharp
var sql = $"SELECT * FROM MediaItems WHERE OwnerId = '{ownerId}'";
```

### 14.3 Write operation phải dùng transaction khi cần nhất quán

Các thao tác `INSERT`, `UPDATE`, `DELETE` liên quan nhiều bảng phải dùng transaction.

Ví dụ:

- Share media + tạo notification.
- Upload media + insert artists/albums liên quan.
- Add track vào playlist + update order.

### 14.4 Không tự tạo/sửa SQL schema nếu chưa được yêu cầu

Người dùng có thể tự tạo database script bằng tay.

Agent không được tự ý thêm/chỉnh database schema nếu task không yêu cầu rõ.

Nếu code cần cột/bảng chưa chắc tồn tại, phải hỏi:

```text
Bảng MediaItem hiện đã có cột AudioUrl, VideoUrl, CoverUrl chưa?
Bạn muốn mình chờ bạn tạo SQL trước hay tạo script gợi ý riêng?
```

### 14.5 Không hardcode connection string

Connection string phải lấy từ config/environment.

Không commit secret.
Không lộ password SQL Server.

---

## 15. Luật bảo mật

Không bao giờ:

- Hardcode JWT secret.
- Hardcode SQL password.
- Hardcode API key.
- Commit `appsettings.Development.json` nếu chứa secret thật.
- Trả `PasswordHash` ra API.
- Trả entity thô ra API.
- Cho controller gọi SQL trực tiếp.
- Dùng string interpolation cho SQL.

Luôn:

- Dùng `[Authorize]` cho endpoint cần đăng nhập.
- Check ownership trước khi sửa/xóa resource.
- Map entity sang DTO.
- Trả error message đã chuẩn hóa.
- Dùng config/environment variables cho secret.

---

## 16. Media streaming roadmap

### 16.1 Thứ tự ưu tiên

Làm theo thứ tự:

1. Audio streaming.
2. Video streaming.
3. Poster/thumbnail serving.
4. Frontend player integration.

### 16.2 Không fake streaming

Không tạo endpoint stream chỉ trả URL nếu yêu cầu là streaming thực sự.

Streaming đúng cần hỗ trợ:

- Progressive playback.
- HTTP Range requests.
- Seek forward/backward.
- Content-Type đúng.
- Không lộ path vật lý server.

### 16.3 ASP.NET Core streaming rule

Với file local, ưu tiên:

```csharp
return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
```

hoặc `FileStreamResult` có range processing nếu phù hợp.

### 16.4 Error của stream endpoint

Khi không tìm thấy media:

```json
{
  "success": false,
  "message": "Không tìm thấy media.",
  "data": null
}
```

Khi file vật lý mất:

```json
{
  "success": false,
  "message": "Không tìm thấy file media trên server.",
  "data": null
}
```

Không trả:

```text
/home/user/project/uploads/audio/file.mp3 not found
```

---

## 17. Frontend policy

Frontend đã có nhưng chưa ổn định.

Agent chỉ sửa frontend khi:

- Backend contract đã ổn định.
- Cần cập nhật Axios service theo response mới.
- Cần test luồng upload/stream/login.
- Người dùng yêu cầu rõ.

Không tự:

- Rewrite toàn bộ UI.
- Chuyển framework.
- Migrate JavaScript sang TypeScript toàn bộ nếu không được yêu cầu.
- Thêm state management library mới nếu chưa cần.

Nếu sửa frontend, phải cố gắng giữ style hiện tại và chỉ thay đổi phần liên quan task.

---

## 18. Refactoring policy

Người dùng chọn hướng an toàn:

```text
Chỉ sửa module đang làm, hạn chế đụng code cũ.
```

Vì vậy agent phải:

- Sửa cục bộ.
- Không refactor lan rộng.
- Không rename hàng loạt.
- Không đổi namespace hàng loạt.
- Không format lại file không liên quan.
- Không chuyển toàn bộ controller sang style mới nếu task chỉ sửa một endpoint.

Tuy nhiên, nếu đang chạm vào một endpoint/module có response chưa chuẩn, agent nên chuẩn hóa trong phạm vi đó.

Ví dụ:

- Task sửa `MediaController.Stream` → được chuẩn hóa response lỗi của `Stream`.
- Task sửa `AuthController.Login` → được chuẩn hóa response của Login.
- Không tự sửa toàn bộ `PlaylistController`, `FavoriteController`, `ShareController` nếu task không liên quan.

---

## 19. Definition of Done

Một task chỉ được coi là xong khi:

```text
[ ] Đúng yêu cầu người dùng.
[ ] Không bịa thông tin còn thiếu.
[ ] Không phá Clean Architecture.
[ ] Không sửa lan rộng ngoài module/task.
[ ] Response endpoint mới/sửa đã dùng ApiResponse<T> nếu là JSON endpoint.
[ ] Error message trả về API là tiếng Việt.
[ ] Không lộ exception hệ thống ra client.
[ ] Không trả entity thô.
[ ] Không hardcode secret.
[ ] SQL dùng parameterized query.
[ ] Write operation quan trọng dùng transaction.
[ ] Class/method mới có XML summary hợp lý.
[ ] Comment giống người viết code, không máy móc.
[ ] Dependency mới đã đăng ký trong DI.
[ ] Swagger attribute được bổ sung nếu endpoint mới.
[ ] dotnet build thành công hoặc đã báo lỗi theo Build Failed Protocol.
```

---

## 20. Build failed protocol

Sau khi sửa code, agent được thử build và sửa tối đa 2 vòng.

```text
Lần 1: dotnet build
        ├── 0 lỗi → DONE
        └── Có lỗi → phân tích, sửa, build lại

Lần 2: dotnet build
        ├── 0 lỗi → DONE
        └── Vẫn lỗi → DỪNG
```

Nếu sau 2 lần vẫn lỗi, không tiếp tục code lan man.

Báo cáo theo mẫu:

```markdown
## ⛔ Build Failed — Cần can thiệp thủ công

**Số lần thử:** 2/2

### Lỗi còn lại

| # | File | Line | Error Code | Mô tả |
|---|------|------|------------|-------|
| 1 | `src/TuneVault.Application/...` | 47 | CS0246 | Không tìm thấy type `...` |

### Nguyên nhân nghi ngờ

- Interface có thể chưa tồn tại.
- Database field có thể chưa được tạo.
- Namespace hiện tại không khớp.

### Những gì đã thử

1. ...
2. ...

### Cần người dùng xác nhận

- [ ] Bảng/cột database hiện có chưa?
- [ ] Có được tạo interface mới không?
```

---

## 21. Checklist trước khi kết thúc task

Trước khi trả lời người dùng, agent tự kiểm tra:

```text
[ ] Tôi đã đọc AGENTS.md.
[ ] Tôi hiểu task thuộc module nào.
[ ] Tôi không tự bịa dữ liệu thiếu.
[ ] Tôi chỉ sửa đúng phạm vi task.
[ ] Tôi không phá dependency rule.
[ ] Tôi không thêm EF Core.
[ ] Tôi không thêm Windows-only command/script.
[ ] Tôi không hardcode secret.
[ ] Tôi không trả lỗi hệ thống ra API.
[ ] Tôi đã chuẩn hóa response trong phần mình chạm vào.
[ ] Tôi đã dùng message tiếng Việt cho lỗi API.
[ ] Tôi đã thêm XML summary/comment hợp lý.
[ ] Tôi đã chạy build hoặc nói rõ vì sao chưa chạy được.
[ ] Tôi đã báo cáo ngắn gọn file đã sửa và kết quả.
```

---

## Ghi chú cho các file PLAN sau này

Hiện tại chưa tạo PLAN file trong AGENTS.md này.

Khi người dùng yêu cầu, nên tạo các file sau:

```text
PLANS/MASTER_PLAN.md
PLANS/BACKEND_PLAN.md
PLANS/FRONTEND_PLAN.md
PLANS/STREAMING_PLAN.md
PLANS/API_RESPONSE_STANDARDIZATION_PLAN.md
```

Khi các file này tồn tại, agent phải đọc chúng trước khi làm feature mới.

---

## Nguyên tắc cuối cùng

Nếu phải chọn giữa:

```text
Code nhanh nhưng có thể sai
```

và:

```text
Hỏi lại để hiểu đúng rồi mới code
```

thì luôn chọn:

```text
Hỏi lại để hiểu đúng rồi mới code.
```

