# TASK: Implement Auth + OTP Feature — TuneVault

> ⚠️ **Đọc `AGENTS.md` ở thư mục gốc TRƯỚC.** Mọi quy tắc trong đó áp dụng cho task này.
> Đặc biệt đọc kỹ §0 (bất nhất codebase), §4 (JWT), §6.5 (mô hình mật khẩu), §10 (bẫy đã biết).
>
> Làm theo đúng thứ tự các bước. Sau MỖI bước, build thử (`dotnet build`) trước khi sang bước sau.
> Nếu gặp mâu thuẫn giữa task này và code thực tế, ưu tiên `AGENTS.md` và DỪNG hỏi developer.

---

## 0. CHỐT QUYẾT ĐỊNH TRƯỚC KHI CODE (bắt buộc)

Task này dựa trên codebase thực tế. Có 3 điểm PHẢI xác nhận với developer; mặc định như sau
nếu không có phản hồi:

1. **Mô hình mật khẩu = HASH PHÍA SERVER** (xem AGENTS §6.5).
   → Client gửi mật khẩu **thô**; server `BCrypt.HashPassword` / `BCrypt.Verify`.
   → Phải thêm package `BCrypt.Net-Next`.
2. **Nhánh đăng nhập Admin = TẠM TẮT** cho tới khi có `AdminRepository`.
   → Login chỉ xử lý `User` qua `GetByIdDisplayAsync`. (Vì `IAdminRepository` chưa có impl — AGENTS §6.1.)
3. **Response bao bọc bằng `ApiResponse<T>`** — phải tạo generic trước (AGENTS §7.2).

> Nếu developer chọn khác (vd hash phía client, hoặc yêu cầu làm luôn AdminRepository),
> điều chỉnh các bước tương ứng và ghi chú trong PR.

---

## CONTEXT HIỆN TẠI (đã kiểm chứng)

- `AuthController.cs`: **toàn bộ bị comment** (không phải 501) → cần viết lại từ đầu.
- `LoginCommand`: hiện `record LoginCommand(string IdDisplay, string Password) : IRequest<string>` — trả `string`.
- `LoginCommandHandler`: **toàn bộ bị comment**; bản cũ so mật khẩu plaintext + gọi `GetByUsernameAsync` (không tồn tại) + inject `IAdminRepository` (chưa có impl).
- `RegisterCommand`: tồn tại nhưng **thiếu `: IRequest<>`** và **không có Handler**.
- Chưa có bảng `OtpLogs`, chưa có `IOtpLogRepository`/impl.
- Chưa có SendOtp/VerifyOtp/ResetPassword.
- `IJwtTokenGenerator` đã implement (`JwtTokenGenerator`) nhưng **chưa đăng ký DI**; namespace là `TuneVault.Application.Interfaces`.
- `IUserRepository` **thiếu**: `GetByEmailAsync`, `AddAsync/InsertAsync`, `UpdatePasswordAsync`.
- `ApiResponse` **non-generic**; chưa có `ApiResponse<T>`.
- JWT: lỗi mismatch config key + encoding; `UseAuthentication/UseAuthorization` đang comment (AGENTS §4.1, §10).

---

## BƯỚC 0 — CHUẨN BỊ NỀN

### 0.1 Thêm package BCrypt (nếu theo mặc định hash phía server)
```bash
dotnet add src/TuneVault.Application/TuneVault.Application.csproj package BCrypt.Net-Next
```

### 0.2 Tạo `ApiResponse<T>` (AGENTS §7.2)
Sửa `src/TuneVault.Application/Common/Responses/ApiResponse.cs`, thêm generic version (giữ
nguyên record non-generic cũ để không phá `PagedResponse<T>`):
```csharp
public record ApiResponse<T>(bool Success, T? Data, string? Message = null)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message);
    public static ApiResponse<T> Fail(string message) => new(false, default, message);
}
```

---

## BƯỚC 1 — SCHEMA SQL (Infrastructure)

Tạo `src/TuneVault.Infrastructure/Database/schemas/V3_AddOtpLogs.sql`:

```sql
USE TuneVault;
GO

CREATE TABLE OtpLogs (
    Id          varchar(10)  NOT NULL,
    Email       varchar(255) NOT NULL,
    OtpCode     varchar(10)  NOT NULL,
    Purpose     varchar(20)  NOT NULL,  -- 'register' | 'reset_password'
    CreatedAt   datetime2    NOT NULL CONSTRAINT DF_OtpLogs_CreatedAt DEFAULT GETUTCDATE(),
    ExpiresAt   datetime2    NOT NULL,  -- = CreatedAt + 5 phút
    IsActive    bit          NOT NULL CONSTRAINT DF_OtpLogs_IsActive DEFAULT 1,
    CONSTRAINT PK_OtpLogs PRIMARY KEY (Id)
);
GO

CREATE INDEX IX_OtpLogs_Email_Purpose_IsActive ON OtpLogs (Email, Purpose, IsActive);
GO
```
> Chạy file này trên SQL Server trước khi chạy backend. `Id` kiểu `varchar(10)` cho đồng bộ
> phong cách Id chuỗi của hệ thống (vd `OTP0000001`).

---

## BƯỚC 2 — DOMAIN INTERFACE (Domain)

Tạo `src/TuneVault.Domain/Interfaces/IOtpLogRepository.cs`:
```csharp
namespace TuneVault.Domain.Interfaces;

/// <summary>Kho dữ liệu cho bảng OtpLogs (mã OTP gửi qua email).</summary>
public interface IOtpLogRepository
{
    /// <summary>Sinh Id mới dạng chuỗi cho một OtpLog (vd OTP0000001).</summary>
    Task<string> GenerateNextIdAsync(CancellationToken ct);

    /// <summary>Thêm một bản ghi OTP mới.</summary>
    Task InsertAsync(string id, string email, string otpCode, string purpose, DateTime expiresAt, CancellationToken ct);

    /// <summary>
    /// Xác minh OTP còn hiệu lực (IsActive=1 và chưa hết hạn) cho đúng email+purpose.
    /// Nếu hợp lệ → set IsActive=0 (consume) và trả về true; ngược lại false.
    /// </summary>
    Task<bool> VerifyAndConsumeAsync(string email, string otpCode, string purpose, CancellationToken ct);
}
```
> Bỏ `ExpireOldOtpsAsync` so với spec gốc: việc kiểm tra hết hạn đã nằm trong điều kiện
> `ExpiresAt > GETUTCDATE()` lúc verify, không cần background job ở giai đoạn này.

---

## BƯỚC 3 — DTOs (đặt trong `Features/Auth/DTOs/` — AGENTS §2.1)

Tạo các file sau, **namespace `TuneVault.Application.Features.Auth.DTOs`**.
KHÔNG dùng các DTO cũ trong `Application/DTOs/Auth/` (sẽ xóa sau khi migrate — AGENTS §8).

`LoginRequestDto.cs`
```csharp
namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record LoginRequestDto(string IdDisplay, string Password); // Password: thô
```

`AuthResponseDto.cs`
```csharp
namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record AuthResponseDto(string AccessToken, string UserId, string IdDisplay, IEnumerable<string> Roles);
```
> Khác bản cũ `(AccessToken, UserId, UserName, Email)`. Dùng `Roles` (nhiều role) cho khớp
> `IJwtTokenGenerator.GenerateToken(..., IEnumerable<string> roles)`.

`SendOtpRequestDto.cs`
```csharp
namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record SendOtpRequestDto(string Email, string Purpose); // "register" | "reset_password"
```

`SendOtpResponseDto.cs`  (DEV MODE: trả OTP để test)
```csharp
namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record SendOtpResponseDto(string Email, string Purpose, string DevOtpCode, DateTime ExpiresAt);
```

`RegisterRequestDto.cs`
```csharp
namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record RegisterRequestDto(
    string Email,
    string OtpCode,
    string IdDisplay,
    string DisplayName,
    string Password);   // thô — server tự BCrypt hash (AGENTS §6.5)
```

`ResetPasswordRequestDto.cs`
```csharp
namespace TuneVault.Application.Features.Auth.DTOs;
public sealed record ResetPasswordRequestDto(string Email, string OtpCode, string NewPassword); // thô
```

---

## BƯỚC 4 — COMMANDS + HANDLERS (Application/Features/Auth/)

> Mọi Command BẮT BUỘC `: IRequest<TResponse>`. Mọi handler inject qua constructor.
> Dùng `using TuneVault.Application.Interfaces;` cho `IJwtTokenGenerator` (AGENTS §4).

### 4.1 Login  (`Commands/Login/`)

Cập nhật **`LoginCommand.cs`** → trả `AuthResponseDto`:
```csharp
using MediatR;
using TuneVault.Application.Features.Auth.DTOs;

namespace TuneVault.Application.Features.Auth.Commands.Login;
public sealed record LoginCommand(string IdDisplay, string Password) : IRequest<AuthResponseDto>;
```

Viết lại **`LoginCommandHandler.cs`** (uncomment + sửa). Inject `IUserRepository`, `IJwtTokenGenerator`.
**KHÔNG inject `IAdminRepository`** (chưa có impl — quyết định §0.2):
```
Flow:
  1. user = await _userRepository.GetByIdDisplayAsync(request.IdDisplay, ct)   // KHÔNG phải GetByUsernameAsync
  2. if (user is null || !user.IsActive) throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.")
  3. if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
         throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.")
  4. roles = [ user.IsArtist ? "Artist" : "Listener" ]
  5. token = _jwt.GenerateToken(user.Id, user.IdDisplay, roles)
  6. return new AuthResponseDto(token, user.Id, user.IdDisplay, roles)
```
> ⚠️ `UnauthorizedAccessException` hiện rơi vào nhánh 500 của global handler. Để trả 401 đúng:
> hoặc map nó tại controller (catch → 401 + `ApiResponse<T>.Fail`), hoặc bổ sung map trong
> global handler `Program.cs`. Chọn cách map ở global handler để nhất quán (xem BƯỚC 6).

### 4.2 SendOtp  (`Commands/SendOtp/`)
`SendOtpCommand.cs`: `record SendOtpCommand(SendOtpRequestDto Request) : IRequest<SendOtpResponseDto>;`

Handler inject `IOtpLogRepository`, `IUserRepository`:
```
1. purpose hợp lệ? (chỉ "register" | "reset_password") — sai → DomainException
2. Nếu purpose == "reset_password": email PHẢI tồn tại
      user = _userRepository.GetByEmailAsync(email, ct); null → DomainException("Email không tồn tại.")
   Nếu purpose == "register": email KHÔNG được trùng (null mới hợp lệ) → ngược lại DomainException
3. otpCode = 6 chữ số: dùng System.Security.Cryptography.RandomNumberGenerator (KHÔNG dùng Random thường)
      vd: RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6")
4. id = await _otpLogRepository.GenerateNextIdAsync(ct)
5. expiresAt = DateTime.UtcNow.AddMinutes(5)
6. await _otpLogRepository.InsertAsync(id, email, otpCode, purpose, expiresAt, ct)
7. [DEV MODE] return new SendOtpResponseDto(email, purpose, otpCode, expiresAt)  // trả OTP để test
```

### 4.3 Register  (`Commands/Register/`)
Sửa **`RegisterCommand.cs`** (thêm `IRequest`):
```csharp
using MediatR;
using TuneVault.Application.Features.Auth.DTOs;
namespace TuneVault.Application.Features.Auth.Commands.Register;
public sealed record RegisterCommand(RegisterRequestDto Request) : IRequest<AuthResponseDto>;
```
> Lưu ý: bản cũ `using TuneVault.Application.DTOs.Auth;` — đổi sang `Features.Auth.DTOs`.

Handler inject `IOtpLogRepository`, `IUserRepository`, `IJwtTokenGenerator`:
```
1. ok = await _otpLogRepository.VerifyAndConsumeAsync(email, otpCode, "register", ct)
      !ok → throw new DomainException("Mã OTP không hợp lệ hoặc đã hết hạn.")
2. Email chưa tồn tại? _userRepository.GetByEmailAsync(email, ct) != null → DomainException("Email đã được sử dụng.")
3. IdDisplay chưa tồn tại? _userRepository.GetByIdDisplayAsync(idDisplay, ct) != null → DomainException("IdDisplay đã tồn tại.")
4. id = await _userRepository.GenerateNextIdAsync(ct)   // dạng "U" + số (xem BƯỚC 5.2)
5. passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)   // ≥60 ký tự → thỏa ràng buộc entity
6. user = new User(id, idDisplay, displayName, email, passwordHash)  // entity tự validate
7. await _userRepository.AddAsync(user, ct)
8. roles = [ "Listener" ]   // user mới mặc định không phải Artist
9. token = _jwt.GenerateToken(user.Id, user.IdDisplay, roles)
10. return new AuthResponseDto(token, user.Id, user.IdDisplay, roles)
```

### 4.4 ResetPassword  (`Commands/ResetPassword/`)
`ResetPasswordCommand.cs`: `record ResetPasswordCommand(ResetPasswordRequestDto Request) : IRequest<bool>;`

Handler inject `IOtpLogRepository`, `IUserRepository`:
```
1. ok = VerifyAndConsumeAsync(email, otpCode, "reset_password", ct); !ok → DomainException("Mã OTP không hợp lệ hoặc đã hết hạn.")
2. user = GetByEmailAsync(email, ct); null → DomainException("Email không tồn tại.")
3. newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword)
4. user.ChangePassword(newHash)                       // entity tự validate ≥60 ký tự
5. await _userRepository.UpdatePasswordAsync(user.Id, user.PasswordHash, ct)
6. return true
```

---

## BƯỚC 5 — REPOSITORIES (Infrastructure)

### 5.1 `OtpLogRepository` (mới) — inject `IDbConnectionFactory` (AGENTS §3.1)
Tạo `src/TuneVault.Infrastructure/Repositories/OtpLogRepository.cs`:
```
- GenerateNextIdAsync:
    Lấy MAX(Id) hiện có → tách số → +1 → format "OTP" + số 7 chữ số (vd OTP0000001).
    Nếu bảng rỗng → OTP0000001.  (Dùng cùng kiểu logic Id chuỗi như phần còn lại của hệ thống.)
- InsertAsync:
    INSERT INTO OtpLogs (Id, Email, OtpCode, Purpose, CreatedAt, ExpiresAt, IsActive)
    VALUES (@Id, @Email, @OtpCode, @Purpose, GETUTCDATE(), @ExpiresAt, 1)
- VerifyAndConsumeAsync:
    SELECT TOP 1 Id FROM OtpLogs
    WHERE Email=@Email AND OtpCode=@OtpCode AND Purpose=@Purpose
      AND IsActive=1 AND ExpiresAt > GETUTCDATE()
    ORDER BY CreatedAt DESC;
    Nếu có → UPDATE OtpLogs SET IsActive=0 WHERE Id=@Id; return true.
    Nếu không → return false.
```
Mọi method: `using var conn = _db.CreateConnection();` + tham số `@Param` + `CommandDefinition(..., cancellationToken: ct)`.

### 5.2 Bổ sung vào `IUserRepository` + `UserRepository` các method còn thiếu
Thêm vào **interface** `IUserRepository` (Domain) và **impl** `UserRepository` (Infrastructure):
```
- Task<User?> GetByEmailAsync(string email, CancellationToken ct)
      SELECT * FROM [Users] WHERE Email = @Email   (so sánh lowercase: @Email = email.ToLowerInvariant())
- Task<string> GenerateNextIdAsync(CancellationToken ct)
      MAX(Id) → +1 → format "U" + số (giữ nhất quán với dữ liệu seed, vd U001/U042).
- Task AddAsync(User user, CancellationToken ct)
      INSERT đầy đủ cột: Id, IdDisplay, DisplayName, Email, PasswordHash, AvatarUrl, Bio,
      IsArtist, TotalFollowers, CreatedAt, IsActive.
- Task UpdatePasswordAsync(string userId, string newPasswordHash, CancellationToken ct)
      UPDATE [Users] SET PasswordHash=@PasswordHash WHERE Id=@Id
```
> `UserRepository` hiện inject `DapperContext` — GIỮ NGUYÊN cho các method mới trong CÙNG file
> này (để đồng nhất style file, AGENTS §0). Repository MỚI (`OtpLogRepository`) mới bắt buộc factory.

---

## BƯỚC 6 — WIRING DI + BẬT AUTH (API/Program.cs)

```csharp
// Repositories
builder.Services.AddScoped<TuneVault.Domain.Interfaces.IOtpLogRepository,
                           TuneVault.Infrastructure.Repositories.OtpLogRepository>();

// JWT generator (HIỆN CHƯA đăng ký — bắt buộc thêm, AGENTS §4)
builder.Services.AddScoped<TuneVault.Application.Interfaces.IJwtTokenGenerator,
                           TuneVault.Infrastructure.Authentication.JwtTokenGenerator>();
```

**Sửa lỗi cấu hình JWT (AGENTS §4.1 / §10 — bắt buộc để token validate được):**
- Dùng **cùng một** config key cho ký và validate (khuyến nghị `JwtSettings:SecretKey` cho cả hai,
  vì generator đang đọc key này).
- Dùng **cùng encoding** (generator dùng `Encoding.UTF8` → phần validate cũng `Encoding.UTF8`).

**Bật middleware (đang bị comment):**
```csharp
app.UseAuthentication();
app.UseAuthorization();
```
(Đặt SAU `UseCors`, TRƯỚC `MapControllers`.)

**(Khuyến nghị) map `UnauthorizedAccessException → 401` trong global exception handler** để Login trả đúng 401.

> KHÔNG xóa đăng ký `ITokenService → TokenService` trong bước này nếu chưa được developer
> đồng ý xóa file (AGENTS §8). Chỉ ghi chú trong PR.

---

## BƯỚC 7 — CONTROLLER (API)

Viết lại `src/TuneVault.API/Controllers/AuthController.cs` (đang comment toàn bộ).
Theo style controller đã implement (`UserController`/`MediaController`): `[ApiController]`,
`[Route("api/[controller]")]`, inject `IMediator`, mỗi action `async Task<IActionResult>` + `CancellationToken ct`.

```
[HttpPost("send-otp")]       Body: SendOtpRequestDto      → SendOtpCommand        → ApiResponse<SendOtpResponseDto>
[HttpPost("register")]       Body: RegisterRequestDto     → RegisterCommand       → ApiResponse<AuthResponseDto>
[HttpPost("login")]          Body: LoginRequestDto        → LoginCommand          → ApiResponse<AuthResponseDto> (401 nếu sai)
[HttpPost("reset-password")] Body: ResetPasswordRequestDto→ ResetPasswordCommand  → ApiResponse<bool>
```
- Map DTO → Command rồi `await _mediator.Send(cmd, ct)`.
- Bọc kết quả: `Ok(ApiResponse<T>.Ok(result))`.
- Guard input rỗng → `BadRequest(ApiResponse<T>.Fail("..."))`.
- Lỗi nghiệp vụ (`DomainException`) tự thành 400 qua global handler — không cần try/catch ở controller.

---

## CHECKLIST HOÀN THÀNH TASK

- [ ] (Quyết định §0 đã chốt: hash phía server / Admin tắt / `ApiResponse<T>`)
- [ ] `BCrypt.Net-Next` đã thêm; `ApiResponse<T>` đã tạo
- [ ] `V3_AddOtpLogs.sql` đã tạo và đã chạy trên DB
- [ ] `IOtpLogRepository` (Domain) + `OtpLogRepository` (Infrastructure, dùng `IDbConnectionFactory`)
- [ ] 6 DTO trong `Features/Auth/DTOs/` (đúng namespace), KHÔNG dùng bản cũ `DTOs/Auth/`
- [ ] `LoginCommand` trả `AuthResponseDto`; Handler dùng `GetByIdDisplayAsync` + `BCrypt.Verify`, KHÔNG inject `IAdminRepository`
- [ ] `RegisterCommand` đã có `: IRequest<AuthResponseDto>` + Handler
- [ ] `SendOtp` + `ResetPassword` Command/Handler hoàn chỉnh
- [ ] `IUserRepository`/`UserRepository` bổ sung: `GetByEmailAsync`, `GenerateNextIdAsync`, `AddAsync`, `UpdatePasswordAsync`
- [ ] DI: đăng ký `IOtpLogRepository` + `IJwtTokenGenerator`
- [ ] JWT: đồng bộ config key + encoding; bật `UseAuthentication()`/`UseAuthorization()`
- [ ] `AuthController` 4 endpoint, response bọc `ApiResponse<T>`
- [ ] `dotnet build` sạch (0 error); test Swagger: send-otp → register → login chạy được
- [ ] PR ghi chú các file đề xuất xóa (AGENTS §8) — KHÔNG tự xóa
