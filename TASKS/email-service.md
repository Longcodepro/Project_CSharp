# TASK: Implement Gmail SMTP Email Service — TuneVault

> ⚠️ Đọc `AGENTS.md` TRƯỚC khi làm bất cứ điều gì.  
> Làm **đúng thứ tự** từng bước. Không được skip hoặc gộp bước.  
> Đây là task **độc lập**, phải hoàn thành TRƯỚC task `auth-otp-feature.md`.

---

## MỤC TIÊU

Tạo một `IEmailService` hoàn chỉnh để backend tự động gửi email OTP qua Gmail SMTP.  
Các task sau (Auth, OTP) sẽ inject `IEmailService` và gọi trực tiếp — không cần sửa gì thêm.

---

## KIẾN TRÚC ĐẶT FILE

```
src/
├── TuneVault.Application/
│   └── Abstractions/
│       └── IEmailService.cs          ← BƯỚC 1: Interface (Application biết)
│
└── TuneVault.Infrastructure/
    ├── Services/
    │   └── GmailSmtpEmailService.cs  ← BƯỚC 2: Implementation (Infrastructure làm)
    └── TuneVault.Infrastructure.csproj ← BƯỚC 3: Thêm package MailKit
```

Đăng ký DI trong:
```
src/TuneVault.API/Program.cs          ← BƯỚC 4: Wire DI
```

Cấu hình trong:
```
src/TuneVault.API/appsettings.json          ← BƯỚC 5: Config production
src/TuneVault.API/appsettings.Development.json ← BƯỚC 5: Config dev (dev mode)
```

---

## BƯỚC 1 — TẠO INTERFACE (Application/Abstractions/)

Tạo file: `src/TuneVault.Application/Abstractions/IEmailService.cs`

```csharp
namespace TuneVault.Application.Abstractions;

/// <summary>
/// Định nghĩa contract để gửi email trong hệ thống TuneVault.
/// Implementation cụ thể nằm ở Infrastructure layer.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi mã OTP đến địa chỉ email của người dùng.
    /// </summary>
    /// <param name="toEmail">Địa chỉ email người nhận.</param>
    /// <param name="otpCode">Mã OTP 6 chữ số cần gửi.</param>
    /// <param name="purpose">Mục đích: "register" hoặc "reset_password".</param>
    Task SendOtpAsync(string toEmail, string otpCode, string purpose);
}
```

> **Lưu ý:** Interface chỉ có 1 method duy nhất. Không thêm method khác vào đây trừ khi có task mới yêu cầu.

---

## BƯỚC 2 — CÀI PACKAGE MAILKIT

`MailKit` là thư viện .NET chuẩn để gửi email qua SMTP, không dùng `System.Net.Mail` vì không hỗ trợ tốt OAuth2 và async.

Chạy lệnh sau trong terminal tại thư mục `src/TuneVault.Infrastructure/`:

```bash
dotnet add package MailKit --version 4.8.0
```

Sau khi cài, file `TuneVault.Infrastructure.csproj` sẽ tự thêm:
```xml
<PackageReference Include="MailKit" Version="4.8.0" />
```

> Kiểm tra: chạy `dotnet build` — nếu không lỗi thì tiếp tục bước 3.

---

## BƯỚC 3 — VIẾT IMPLEMENTATION (Infrastructure/Services/)

Tạo file: `src/TuneVault.Infrastructure/Services/GmailSmtpEmailService.cs`

```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

/// <summary>
/// Gửi email qua Gmail SMTP sử dụng MailKit.
/// Đọc cấu hình từ section "EmailSettings" trong appsettings.json.
/// </summary>
public sealed class GmailSmtpEmailService : IEmailService
{
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _appPassword;
    private readonly bool _isDevMode;

    public GmailSmtpEmailService(IConfiguration configuration)
    {
        _fromEmail   = configuration["EmailSettings:FromEmail"]
                       ?? throw new InvalidOperationException("Thiếu EmailSettings:FromEmail trong appsettings.json");
        _fromName    = configuration["EmailSettings:FromName"] ?? "TuneVault";
        _appPassword = configuration["EmailSettings:AppPassword"]
                       ?? throw new InvalidOperationException("Thiếu EmailSettings:AppPassword trong appsettings.json");
        _isDevMode   = bool.TryParse(configuration["EmailSettings:DevMode"], out var dev) && dev;
    }

    /// <inheritdoc />
    public async Task SendOtpAsync(string toEmail, string otpCode, string purpose)
    {
        // ── DEV MODE: Không gửi email thật, chỉ log ra console ──────────────
        if (_isDevMode)
        {
            Console.WriteLine("========================================");
            Console.WriteLine($"[DEV EMAIL] To      : {toEmail}");
            Console.WriteLine($"[DEV EMAIL] Purpose : {purpose}");
            Console.WriteLine($"[DEV EMAIL] OTP Code: {otpCode}");
            Console.WriteLine("========================================");
            return;
        }

        // ── PRODUCTION MODE: Gửi email thật qua Gmail SMTP ──────────────────
        var subject = purpose == "register"
            ? "TuneVault — Mã xác thực đăng ký tài khoản"
            : "TuneVault — Mã xác thực đặt lại mật khẩu";

        var bodyHtml = BuildHtmlBody(otpCode, purpose);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = bodyHtml };

        using var client = new SmtpClient();

        // Gmail SMTP: smtp.gmail.com, port 587, STARTTLS
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_fromEmail, _appPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    // ── Helper: Tạo nội dung HTML cho email ─────────────────────────────────
    private static string BuildHtmlBody(string otpCode, string purpose)
    {
        var purposeText = purpose == "register"
            ? "hoàn tất đăng ký tài khoản"
            : "đặt lại mật khẩu";

        return $"""
            <!DOCTYPE html>
            <html lang="vi">
            <head><meta charset="UTF-8"></head>
            <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;">
              <div style="max-width: 480px; margin: auto; background: white;
                          border-radius: 8px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
                
                <h2 style="color: #1a1a1a; margin-bottom: 8px;">TuneVault</h2>
                <p style="color: #555;">Mã OTP để {purposeText} của bạn:</p>
                
                <div style="background: #f0f0f0; border-radius: 6px; padding: 20px;
                            text-align: center; margin: 24px 0;">
                  <span style="font-size: 36px; font-weight: bold;
                               letter-spacing: 8px; color: #1a1a1a;">
                    {otpCode}
                  </span>
                </div>
                
                <p style="color: #888; font-size: 13px;">
                  ⏱ Mã có hiệu lực trong <strong>5 phút</strong>.
                </p>
                <p style="color: #888; font-size: 13px;">
                  Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.
                </p>
                
                <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;">
                <p style="color: #bbb; font-size: 11px; text-align: center;">
                  © 2025 TuneVault. Đây là email tự động, vui lòng không reply.
                </p>
              </div>
            </body>
            </html>
            """;
    }
}
```

---

## BƯỚC 4 — ĐĂNG KÝ DI (Program.cs)

Mở file `src/TuneVault.API/Program.cs`.

Tìm đến section `// 5. ĐĂNG KÝ CÁC DỊCH VỤ BỔ SUNG` và **thêm dòng** sau vào cuối section đó:

```csharp
// Email Service — Gmail SMTP
builder.Services.AddScoped<TuneVault.Application.Abstractions.IEmailService,
                            TuneVault.Infrastructure.Services.GmailSmtpEmailService>();
```

Đặt ngay sau dòng đăng ký `ICurrentUserService`, trước section CORS:

```csharp
// =========================================================================
// 5. ĐĂNG KÝ CÁC DỊCH VỤ BỔ SUNG
// =========================================================================
builder.Services.AddScoped<TuneVault.Application.Abstractions.ITokenService,
                            TuneVault.Infrastructure.Services.TokenService>();
builder.Services.AddScoped<TuneVault.Application.Abstractions.ICurrentUserService,
                            TuneVault.Infrastructure.Services.CurrentUserService>();

// ✅ THÊM DÒNG NÀY:
builder.Services.AddScoped<TuneVault.Application.Abstractions.IEmailService,
                            TuneVault.Infrastructure.Services.GmailSmtpEmailService>();
```

> **Không xóa** hay sửa các dòng đang có. Chỉ thêm dòng mới.

---

## BƯỚC 5 — CẤU HÌNH appsettings.json

### 5A — appsettings.Development.json (dùng khi dev — không gửi email thật)

Thêm section sau vào file `appsettings.Development.json`:

```json
{
  "EmailSettings": {
    "FromEmail": "your-gmail@gmail.com",
    "FromName": "TuneVault Dev",
    "AppPassword": "placeholder-not-used-in-dev-mode",
    "DevMode": true
  }
}
```

Khi `DevMode: true` → email **không được gửi**, OTP chỉ in ra terminal (console.log màu xanh).  
Dùng trong quá trình code và test qua Swagger.

---

### 5B — appsettings.json (production — gửi email thật)

Thêm section sau vào file `appsettings.json`:

```json
{
  "EmailSettings": {
    "FromEmail": "your-gmail@gmail.com",
    "FromName": "TuneVault",
    "AppPassword": "xxxx xxxx xxxx xxxx",
    "DevMode": false
  }
}
```

---

### 5C — Cách lấy App Password Gmail (làm 1 lần duy nhất)

> ⚠️ Đây là bước **người dùng (developer) làm tay**, không phải AI làm.  
> Làm 1 lần, sau đó dán vào appsettings.json là xong mãi mãi.

**Bước 1:** Truy cập [https://myaccount.google.com/security](https://myaccount.google.com/security)

**Bước 2:** Bật **"2-Step Verification"** (Xác minh 2 bước) nếu chưa bật — bắt buộc phải bật mới dùng được App Password.

**Bước 3:** Tìm mục **"App passwords"** (Mật khẩu ứng dụng):
- Gõ "App passwords" vào ô tìm kiếm trong trang Google Account
- Hoặc vào thẳng: [https://myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)

**Bước 4:** Tạo App Password mới:
- Ô "App name" → gõ: `TuneVault`
- Bấm **"Create"**
- Google sinh ra 16 ký tự dạng: `abcd efgh ijkl mnop`
- **Copy ngay** vì Google chỉ hiển thị 1 lần

**Bước 5:** Dán vào `appsettings.json`:
```json
"AppPassword": "abcd efgh ijkl mnop"
```

> ✅ Gmail App Password khác với mật khẩu Gmail thường — an toàn, có thể thu hồi bất kỳ lúc nào.

---

## BƯỚC 6 — THÊM appsettings.json VÀO .gitignore

> ⚠️ **QUAN TRỌNG — Bảo mật App Password**

Mở file `.gitignore` tại thư mục gốc project, thêm dòng:

```
# Sensitive config — chứa Gmail App Password
src/TuneVault.API/appsettings.json
```

Giữ `appsettings.Development.json` trong git vì không chứa password thật (DevMode = true).

---

## BƯỚC 7 — KIỂM TRA BUILD

Sau khi hoàn thành tất cả bước trên, chạy:

```bash
cd src
dotnet build
```

Kết quả mong đợi:
```
Build succeeded.
0 Warning(s)
0 Error(s)
```

Nếu lỗi → đọc kỹ thông báo lỗi, kiểm tra lại:
- Package MailKit đã được thêm vào `.csproj` chưa?
- Namespace `using MailKit.Net.Smtp;` có đúng không?
- DI trong `Program.cs` có đúng tên class không?

---

## CHECKLIST HOÀN THÀNH TASK

Tự kiểm tra trước khi báo xong:

- [ ] `IEmailService.cs` đã tạo tại `Application/Abstractions/`
- [ ] Package `MailKit 4.8.0` đã thêm vào `Infrastructure.csproj`
- [ ] `GmailSmtpEmailService.cs` đã tạo tại `Infrastructure/Services/`
- [ ] DI đã đăng ký trong `Program.cs` section 5
- [ ] `appsettings.Development.json` có section `EmailSettings` với `DevMode: true`
- [ ] `appsettings.json` có section `EmailSettings` với `DevMode: false`
- [ ] `appsettings.json` đã thêm vào `.gitignore`
- [ ] `dotnet build` không có lỗi

---

## SAU KHI HOÀN THÀNH TASK NÀY

Task tiếp theo cần làm: **`TASKS/auth-otp-feature.md`**

Trong task đó, `SendOtpCommandHandler` sẽ inject `IEmailService` và gọi:

```csharp
// Trong SendOtpCommandHandler:
private readonly IEmailService _emailService;

// Trong Handle():
await _emailService.SendOtpAsync(request.Email, otpCode, request.Purpose);
// Dev mode  → in ra console, không gửi email
// Prod mode → email tự động bay vào hòm thư user
```

Không cần sửa gì trong Email Service nữa sau khi task này xong.
