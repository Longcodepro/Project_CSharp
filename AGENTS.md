# AGENTS.md — TuneVault Coding Rules for AI Agents

> Đây là bộ quy tắc BẮT BUỘC cho mọi AI agent làm việc trên project này.
> Đọc TOÀN BỘ file này trước khi viết bất kỳ dòng code nào.
> Không được bỏ qua bất kỳ rule nào dù lý do là gì.
>
> ⚠️ **File này được biên soạn sau khi đọc codebase thực tế (commit hiện tại).**
> Một số quy tắc là **"trạng thái hiện tại"** (mô tả code đang chạy), một số là
> **"chuẩn go-forward"** (code MỚI phải tuân theo). Chỗ nào codebase đang KHÔNG
> nhất quán, file này nói rõ và đánh dấu `⚠️ HIỆN TRẠNG` + `✅ CHUẨN MỚI`.

---

## 0. SỰ THẬT QUAN TRỌNG NHẤT — ĐỌC TRƯỚC TIÊN

Codebase hiện tại **chưa nhất quán**. Có 2 convention song song và nhiều thành phần
trùng/chết. Agent KHÔNG được giả định mọi file hiện có là "đúng chuẩn". Cụ thể:

| Vấn đề | Hiện trạng | Quy tắc go-forward |
|--------|-----------|--------------------|
| Vị trí DTO | Đa số nằm `Application/DTOs/<Feature>/`; chỉ `User` dùng `Features/User/DTOs/` | Code MỚI: đặt trong `Features/<Feature>/DTOs/` |
| Interface JWT | Có 2 (`IJwtTokenGenerator` + `ITokenService`) | Chỉ dùng `IJwtTokenGenerator` |
| Tạo connection | `UserRepository`/`MediaRepository` dùng `DapperContext`; có sẵn `IDbConnectionFactory` | Code MỚI: inject `IDbConnectionFactory` |
| Response wrapper | `ApiResponse` hiện **non-generic**, KHÔNG có field `data` | Phải tạo `ApiResponse<T>` (xem §7) |
| `ApiResponse<T>` | **CHƯA TỒN TẠI** | Tạo trước khi dùng |

> Khi sửa code CŨ trong cùng một file, ưu tiên giữ nguyên style file đó để tránh
> diff rác; khi tạo FILE MỚI, bắt buộc theo chuẩn go-forward.

---

## 1. KIẾN TRÚC — HIỂU TRƯỚC KHI CODE

Project sử dụng **Clean Architecture** với 4 layer theo thứ tự dependency:

```
Domain → Application → Infrastructure → API
```

**Quy tắc dependency tuyệt đối:**
- `Domain` KHÔNG được import bất kỳ layer nào khác
- `Application` chỉ được import `Domain`
- `Infrastructure` được import `Domain` + `Application`
- `API` được import tất cả

**Stack kỹ thuật (xác nhận từ .csproj):**
- **.NET 9** (`net9.0`) + ASP.NET Core + Dapper `2.1.79` + SQL Server (`Microsoft.Data.SqlClient`)
- CQRS + **MediatR `14.1.0`** (mọi business logic đi qua Handler)
- Repository Pattern (Interface ở Domain, Implement ở Infrastructure)
- JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer 9.0.0`)

> ⚠️ **Chưa có package `BCrypt.Net-Next`.** Nếu cần hash/verify mật khẩu phía server,
> phải `dotnet add package BCrypt.Net-Next` vào `TuneVault.Application` (hoặc nơi
> handler dùng) TRƯỚC. Đừng giả định `BCrypt.Verify(...)` biên dịch được ngay.

---

## 2. QUY TẮC CẤU TRÚC FILE

### 2.1 DTO của code MỚI phải nằm TRONG feature

✅ CHUẨN MỚI:
```
Application/Features/Auth/DTOs/LoginRequestDto.cs
Application/Features/User/DTOs/UserProfileDto.cs   ← feature User đã theo chuẩn này
```

⚠️ HIỆN TRẠNG (KHÔNG nhân rộng thêm — đây là nơi cần dọn dần):
```
Application/DTOs/Auth/LoginRequestDto.cs      ← đang được dùng, sẽ di chuyển
Application/DTOs/Media/MediaItemDto.cs        ← đang được dùng (Media chưa migrate)
Application/DTOs/User/UserProfileDto.cs       ← ORPHAN, không ai dùng → xóa (xem §8)
```

> Quy tắc: KHÔNG tạo DTO mới ở `Application/DTOs/...`. Trước khi `using` một DTO,
> kiểm tra namespace nào đang thực sự được handler tham chiếu (đừng dùng bản orphan).

### 2.2 Mỗi Command/Query có folder riêng

```
Features/
  Auth/
    Commands/
      Login/         { LoginCommand.cs, LoginCommandHandler.cs }
      Register/      { RegisterCommand.cs, RegisterCommandHandler.cs }
      SendOtp/       { SendOtpCommand.cs, SendOtpCommandHandler.cs }
      VerifyOtp/     { VerifyOtpCommand.cs, VerifyOtpCommandHandler.cs }
      ResetPassword/ { ResetPasswordCommand.cs, ResetPasswordCommandHandler.cs }
    DTOs/
      LoginRequestDto.cs, AuthResponseDto.cs, SendOtpRequestDto.cs, ...
```

**Mọi Command/Query BẮT BUỘC implement `IRequest<TResponse>`.**
> ⚠️ `RegisterCommand.cs` hiện tại thiếu `: IRequest<...>` → đây là BUG, phải sửa.

### 2.3 Schema SQL — đặt tại đúng chỗ, đánh version tăng dần

```
Infrastructure/Database/schemas/V3_AddOtpLogs.sql   ← migration mới
```
Đã có `V1_TuneVault.sql`, `V2_AddMissingColumns.sql`. File mới tiếp theo là `V3_...`.
KHÔNG nhúng DDL vào file C# hay comment.

---

## 3. QUY TẮC DAPPER & PERSISTENCE

### 3.1 Code MỚI inject `IDbConnectionFactory` (không inject `DapperContext`)

`DapperContext` thực chất chỉ là wrapper mỏng delegate xuống `IDbConnectionFactory`
(xác nhận trong `Persistence/DapperContext.cs`). Hai cái cùng expose `CreateConnection()`.
Để code mới nhất quán, **dùng thẳng `IDbConnectionFactory`**.

```csharp
// ✅ ĐÚNG cho repository MỚI
public sealed class OtpLogRepository : IOtpLogRepository
{
    private readonly IDbConnectionFactory _db;
    public OtpLogRepository(IDbConnectionFactory db) => _db = db;

    public async Task<bool> VerifyAndConsumeAsync(string email, string otp, string purpose)
    {
        using var conn = _db.CreateConnection();
        // ...
    }
}
```

> ⚠️ HIỆN TRẠNG: `UserRepository` và `MediaRepository` đang inject `DapperContext`.
> KHÔNG bắt buộc refactor chúng trong task khác; nhưng repository MỚI dùng factory.

### 3.2 Luôn `using var conn = ...` — không để connection leak

```csharp
using var conn = _db.CreateConnection();           // ✅
var conn = _db.CreateConnection();                 // ❌ thiếu using
```

### 3.3 Tham số hóa SQL bằng `@Param` — KHÔNG nối chuỗi

```csharp
await conn.ExecuteAsync("UPDATE Users SET IsActive = 0 WHERE Id = @Id", new { Id = id }); // ✅
await conn.ExecuteAsync($"... WHERE Id = '{id}'");                                          // ❌ SQL Injection
```

### 3.4 Hỗ trợ CancellationToken qua `CommandDefinition`

Theo đúng pattern repository hiện có:
```csharp
return await conn.QuerySingleOrDefaultAsync<User>(
    new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
```

---

## 4. QUY TẮC INTERFACE JWT — CHỈ DÙNG MỘT

Project có **2 abstraction trùng chức năng**. Quy tắc thống nhất:

- ✅ **Dùng `IJwtTokenGenerator`** — đã được implement đầy đủ và đúng bởi
  `Infrastructure/Authentication/JwtTokenGenerator.cs`.
- ❌ **`ITokenService`** (`Application/Abstractions/`) — implement `TokenService` chỉ là
  STUB trả về chuỗi `"chuỗi_token_của_bạn"`. KHÔNG dùng. Sẽ bị xóa (§8).

```csharp
// ✅ ĐÚNG — namespace thực tế của interface là Application.Interfaces
using TuneVault.Application.Interfaces;
private readonly IJwtTokenGenerator _jwt;
string token = _jwt.GenerateToken(userId, username, roles);   // roles: IEnumerable<string>

// ❌ SAI
using TuneVault.Application.Abstractions;
private readonly ITokenService _tokenService;                 // KHÔNG DÙNG
```

> ⚠️ **CẢNH BÁO BẪY NAMESPACE:** File `IJwtTokenGenerator.cs` nằm trong thư mục
> `Domain/Interfaces/` NHƯNG khai báo `namespace TuneVault.Application.Interfaces;`.
> Vì vậy `using` đúng là `TuneVault.Application.Interfaces`, KHÔNG phải `...Domain.Interfaces`.
> (Đây là một bất nhất vị trí-file vs namespace — xem báo cáo, không tự ý đổi trong task này.)

> ⚠️ **`IJwtTokenGenerator` CHƯA được đăng ký trong DI** (`Program.cs`). Bất kỳ handler
> nào inject nó sẽ ném lỗi DI lúc runtime cho tới khi thêm:
> `services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();`

### 4.1 Nội dung token (theo `JwtTokenGenerator` thực tế)

`GenerateToken(string userId, string username, IEnumerable<string> roles)` sinh claims:
- `sub` = userId
- `unique_name` = username (IdDisplay với User, Email với Admin)
- nhiều `ClaimTypes.Role` = mỗi role một claim
- `jti` = Guid ngẫu nhiên

Đọc cấu hình từ section **`JwtSettings:*`** (SecretKey/Issuer/Audience/ExpireDays), encode key bằng **UTF8**.

> 🔴 **BUG CẤU HÌNH ĐÃ BIẾT (ưu tiên cao):** Phần validate token trong `Program.cs` đọc
> key từ `Jwt:SecretKey` và encode bằng **ASCII**, trong khi generator ký bằng
> `JwtSettings:SecretKey` + **UTF8**. Hai key/encoding KHÁC NHAU → token sẽ KHÔNG
> validate được. Phải đồng bộ (cùng config key + cùng encoding) khi bật auth.

---

## 5. QUY TẮC BUSINESS LOGIC & DOMAIN

### 5.1 Không viết business logic / truy vấn DB trong Controller hay Repository

- Controller: chỉ nhận request → `_mediator.Send(...)` → wrap response. Không query, không if-nghiệp-vụ.
- Repository: chỉ I/O dữ liệu (SQL). Không quyết định nghiệp vụ.
- **Mọi business logic nằm trong Handler.**

### 5.2 Domain Entity tự validate — không validate thủ công ở Handler

`User` entity tự throw `DomainException` trong constructor và business method. Ví dụ ràng buộc thực tế:
- `IdDisplay`: 3–15 ký tự, bắt đầu bằng chữ cái, chỉ `[a-zA-Z0-9_]`, lưu lowercase.
- `DisplayName`: ≤ 24 ký tự, bắt buộc.
- `Email`: regex cơ bản, lưu lowercase.
- `PasswordHash`: ≥ **60 ký tự** (chuẩn độ dài BCrypt hash) — nếu không sẽ throw.
- Business method có sẵn: `UpdateProfile`, `VerifyAsArtist`, `ChangePassword(newHash)`, `Increment/DecrementFollowers`.

```csharp
var user = new User(id, idDisplay, displayName, email, passwordHash); // tự validate
user.ChangePassword(newPasswordHash);                                 // tự validate ≥60 ký tự
```

> Hệ quả: nếu lưu mật khẩu, giá trị truyền vào `PasswordHash` PHẢI là hash ≥60 ký tự
> (không được nhét plaintext — entity sẽ ném `DomainException`).

### 5.3 Phân quyền & Role

Role dùng trong token (theo `LoginCommandHandler` cũ):
- `"Admin"` (kèm role hành chính cụ thể của Admin, vd `"SuperAdmin"`)
- `"Artist"` — `User.IsArtist == true`
- `"Listener"` — `User.IsArtist == false`

> ⚠️ Lưu ý bất nhất chuỗi role: một số DTO/handler khác dùng `"User"` thay cho
> `"Listener"`. Khi viết mới, thống nhất **`"Listener"`** cho người nghe phổ thông và
> ghi chú nếu phải map.

### 5.4 Lỗi nghiệp vụ → `DomainException`

Global handler trong `Program.cs` bắt `DomainException` → **400** (JSON `{ statusCode, message }`),
mọi `Exception` khác → **500**. Vì vậy:
- Lỗi nghiệp vụ (OTP sai, email tồn tại, không đủ quyền…) → `throw new DomainException("...")`.
- Lỗi xác thực đăng nhập → có thể `throw new UnauthorizedAccessException(...)` **nhưng** hiện
  global handler CHƯA map nó về 401 (sẽ rơi vào 500). Nếu muốn 401 chuẩn, hoặc bổ sung map
  trong global handler, hoặc trả `ApiResponse.Fail` + status code tại controller. Xem §7.

---

## 6. QUY TẮC AUTH & OTP

### 6.1 Đăng nhập — dùng IdDisplay (+ nhánh Email cho Admin)

```
Request: { "idDisplay": "john_doe", "password": "..." }
Flow:
  1. Tìm Admin theo định danh (Email) — NẾU nhánh Admin được bật (xem cảnh báo dưới).
  2. Nếu không phải Admin → User: _userRepository.GetByIdDisplayAsync(idDisplay, ct)
  3. Cả hai null → lỗi xác thực.
  4. Xác minh mật khẩu (xem §6.5 về mô hình hash — CẦN CHỐT).
  5. _jwt.GenerateToken(userId, username, roles)
```

> 🔴 **`IAdminRepository` CHƯA CÓ IMPLEMENTATION** và chưa đăng ký DI. Bất kỳ handler
> nào inject `IAdminRepository` sẽ KHÔNG construct được. Trước khi dùng nhánh Admin:
> hoặc (a) implement + đăng ký `AdminRepository`, hoặc (b) tạm bỏ nhánh Admin khỏi Login.
> KHÔNG inject một interface không có implementation rồi để runtime nổ.

> ⚠️ `LoginCommandHandler` cũ gọi `_userRepository.GetByUsernameAsync(...)` — method này
> KHÔNG tồn tại trên `IUserRepository`. Phương thức đúng là **`GetByIdDisplayAsync`**.

### 6.2 OTP — bảng `OtpLogs`

- Mỗi lần gửi OTP → INSERT 1 row mới vào `OtpLogs`.
- OTP hết hạn sau **5 phút** kể từ `CreatedAt` (lưu `ExpiresAt = CreatedAt + 5min`).
- Verify: yêu cầu `IsActive = 1` VÀ `ExpiresAt > GETUTCDATE()` (so theo UTC).
- Verify thành công → UPDATE `IsActive = 0` (consume, không xóa row).
- **DEV MODE:** KHÔNG gửi email thật — trả OTP trong response để test.

### 6.3 Đăng ký — flow OTP
```
1. POST /api/auth/send-otp   { email, purpose: "register" }
2. POST /api/auth/register   { email, otp, idDisplay, displayName, password|passwordHash }
   → verify OTP → tạo User → (tùy chọn) trả token đăng nhập luôn
```

### 6.4 Reset password — flow OTP
```
1. POST /api/auth/send-otp        { email, purpose: "reset_password" }
2. POST /api/auth/reset-password  { email, otp, newPassword|newPasswordHash }
```
`send-otp` dùng chung; phân biệt bằng field `purpose: "register" | "reset_password"`.

### 6.5 🔴 MÔ HÌNH MẬT KHẨU — CẦN CHỐT TRƯỚC KHI CODE

Spec gốc mâu thuẫn: DTO ghi "frontend tự BCrypt hash rồi gửi `PasswordHash`", nhưng Login
lại "BCrypt.Verify(rawPassword, hash)". **Hai điều này không thể đồng thời đúng** — BCrypt
sinh salt ngẫu nhiên nên hash phía client mỗi lần một khác, server không thể verify mật khẩu
thô với một hash do client tạo.

**Khuyến nghị Tech Lead (mặc định áp dụng): HASH PHÍA SERVER.**
- Client luôn gửi **mật khẩu thô** (`password` / `newPassword`).
- Server: `BCrypt.HashPassword(raw)` khi register/reset; `BCrypt.Verify(raw, user.PasswordHash)` khi login.
- Cần thêm package `BCrypt.Net-Next` (xem §1).
- DTO nên đặt tên field là `Password` (thô), không phải `PasswordHash`.

> Nếu chủ dự án muốn giữ "hash phía client", thì Login KHÔNG được dùng BCrypt.Verify mà phải
> so khớp hash trực tiếp với một thuật toán **deterministic** (vd SHA-256, KHÔNG phải BCrypt) —
> kém an toàn hơn. Phải chốt rõ một hướng; task mặc định theo hash phía server.

---

## 7. QUY TẮC RESPONSE FORMAT

### 7.1 Hiện trạng
`Common/Responses/ApiResponse.cs` đang là **non-generic**, KHÔNG có field `data`:
```csharp
public record ApiResponse(bool Success, string? Message = null);     // chỉ Success + Message
public record PagedResponse<T>(...) : ApiResponse(true, Message);     // cho list phân trang
```

### 7.2 ✅ Chuẩn go-forward — thêm `ApiResponse<T>`
Trước khi để endpoint trả dữ liệu kèm bao bọc, **tạo** generic version (cùng file `ApiResponse.cs`):
```csharp
public record ApiResponse<T>(bool Success, T? Data, string? Message = null)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message);
    public static ApiResponse<T> Fail(string message) => new(false, default, message);
}
```
Hình dạng JSON mục tiêu:
```jsonc
// Success
{ "success": true, "data": { ... }, "message": null }
// Error
{ "success": false, "data": null, "message": "Mô tả lỗi" }
```

### 7.3 HTTP status codes
- `200` — thành công
- `400` — lỗi nghiệp vụ (sai OTP, email đã tồn tại…) → thường do `DomainException`
- `401` — chưa đăng nhập / sai thông tin đăng nhập / token không hợp lệ
- `403` — không đủ quyền
- `404` — không tìm thấy resource

> Global handler hiện chỉ tự map `DomainException → 400` và `Exception → 500`.
> Các status khác (401/403/404) phải do controller/handler chủ động trả.

---

## 8. DANH SÁCH FILE CẦN XÓA / DỌN DẸP

> **KHÔNG TỰ XÓA** — chỉ liệt kê và hỏi developer trước. Đây là kết quả rà soát thực tế.

| File | Lý do | Mức độ chắc chắn |
|------|-------|------------------|
| `Application/Abstractions/ITokenService.cs` | Trùng `IJwtTokenGenerator`; impl chỉ là stub | Cao |
| `Infrastructure/Services/TokenService.cs` | Stub trả `"chuỗi_token_của_bạn"`, không dùng | Cao |
| (DI) dòng đăng ký `ITokenService → TokenService` trong `Program.cs` | Đi kèm 2 file trên | Cao |
| `Application/DTOs/User/UserProfileDto.cs` | ORPHAN — handler dùng bản `Features/User/DTOs/UserProfileDto.cs` | Cao |
| `Application/DTOs/User/UpdateProfileRequestDto.cs` | Nghi orphan — kiểm tra reference trước khi xóa | Trung bình |
| `Application/DTOs/Auth/*` (sau khi migrate) | Sẽ thay bằng `Features/Auth/DTOs/*` — **chỉ xóa SAU khi đã chuyển + sửa using** | Trung bình |

> ⚠️ KHÔNG xóa `DapperContext.cs` ngay: còn `UserRepository`/`MediaRepository` phụ thuộc.
> Chỉ xóa sau khi migrate hết sang `IDbConnectionFactory`.

---

## 9. CHECKLIST TRƯỚC KHI SUBMIT CODE

- [ ] DTO MỚI nằm trong `Features/<Feature>/DTOs/`?
- [ ] Command/Query có `: IRequest<T>` và có Handler tương ứng?
- [ ] Repository MỚI inject `IDbConnectionFactory` (không `DapperContext`)?
- [ ] Dùng `IJwtTokenGenerator` (using `TuneVault.Application.Interfaces`), KHÔNG `ITokenService`?
- [ ] Đã đăng ký DI cho mọi interface mới inject (repo, `IJwtTokenGenerator`)?
- [ ] Không inject interface chưa có implementation (vd `IAdminRepository`) nếu chưa hiện thực hóa?
- [ ] Mọi connection Dapper có `using var conn`?
- [ ] SQL dùng `@Param`, không nối chuỗi?
- [ ] Business logic ở Handler/Domain, controller chỉ `_mediator.Send`?
- [ ] Response trả `ApiResponse<T>` đúng format (đã tạo generic nếu chưa có)?
- [ ] Migration SQL mới có file riêng `V{n}_Tên.sql`, version tăng dần?
- [ ] Nếu dùng BCrypt: đã thêm package `BCrypt.Net-Next`?
- [ ] `PasswordHash` truyền vào `User` luôn ≥ 60 ký tự (không nhét plaintext)?

---

## 10. NHỮNG CÁI BẪY ĐÃ BIẾT (đọc kỹ kẻo mất thời gian)

1. `IJwtTokenGenerator` ở thư mục Domain nhưng namespace là `Application.Interfaces`.
2. `IJwtTokenGenerator` chưa đăng ký DI → phải thêm khi dùng.
3. `IAdminRepository` không có implementation.
4. Chưa có `BCrypt.Net-Next`.
5. Key ký token (`JwtSettings:SecretKey`, UTF8) ≠ key validate (`Jwt:SecretKey`, ASCII).
6. `app.UseAuthentication()` / `app.UseAuthorization()` đang bị comment trong `Program.cs`.
7. `ApiResponse` chưa có generic/`data`.
8. `RegisterCommand` thiếu `: IRequest<>`.
9. `LoginCommand` hiện trả `string` (không phải `AuthResponseDto`); `AuthResponseDto` hiện có
   shape `(AccessToken, UserId, UserName, Email)` — khác shape đề xuất trong task.
10. Login cũ so mật khẩu **plaintext** (`hash != request.Password`) và gọi method không tồn tại.
