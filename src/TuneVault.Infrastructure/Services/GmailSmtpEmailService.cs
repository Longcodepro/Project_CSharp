using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using TuneVault.Application.Abstractions;

namespace TuneVault.Infrastructure.Services;

public class GmailSmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailSmtpEmailService> _logger;
    private readonly bool _isDevelopment;

    public GmailSmtpEmailService(IConfiguration configuration, ILogger<GmailSmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _isDevelopment = configuration.GetValue<bool>("EmailSettings:DevMode");
    }

    public async Task SendOtpAsync(string toEmail, string otpCode, string purpose, CancellationToken ct = default)
    {
        if (_isDevelopment)
        {
            _logger.LogInformation("Development Mode: OTP for {Purpose} to {Email} is {OtpCode}", purpose, toEmail, otpCode);
            return;
        }

        var emailSettings = _configuration.GetSection("EmailSettings");
        var senderEmail = emailSettings["SenderEmail"];
        var senderPassword = emailSettings["SenderPassword"];
        var smtpHost = emailSettings["SmtpHost"];
        var smtpPort = emailSettings.GetValue<int>("SmtpPort");

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword)
            || string.IsNullOrEmpty(smtpHost) || smtpPort == 0)
        {
            _logger.LogError("Email settings are incomplete. Cannot send email.");
            throw new InvalidOperationException("Email service configuration is incomplete.");
        }

        var subject = purpose == "register"
            ? "TuneVault — Xác nhận đăng ký tài khoản"
            : "TuneVault — Yêu cầu đặt lại mật khẩu";

        var actionLabel = purpose == "register"
            ? "hoàn tất đăng ký tài khoản"
            : "đặt lại mật khẩu";

        var body = $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
</head>
<body style=""margin:0;padding:0;background-color:#0f0f0f;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#0f0f0f;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""520"" cellpadding=""0"" cellspacing=""0""
               style=""background-color:#1a1a1a;border-radius:16px;overflow:hidden;box-shadow:0 8px 32px rgba(0,0,0,0.5);"">

          <!-- HEADER -->
          <tr>
            <td style=""background:linear-gradient(135deg,#1db954,#158a3e);padding:36px 40px;text-align:center;"">
              <h1 style=""margin:0;color:#ffffff;font-size:28px;font-weight:700;letter-spacing:2px;"">
                🎵 TuneVault
              </h1>
              <p style=""margin:6px 0 0;color:rgba(255,255,255,0.8);font-size:13px;letter-spacing:1px;"">
                YOUR MUSIC. YOUR VAULT.
              </p>
            </td>
          </tr>

          <!-- BODY -->
          <tr>
            <td style=""padding:40px 40px 32px;"">
              <p style=""margin:0 0 8px;color:#aaaaaa;font-size:14px;"">Xin chào,</p>
              <p style=""margin:0 0 24px;color:#e0e0e0;font-size:15px;line-height:1.6;"">
                Chúng tôi nhận được yêu cầu <strong style=""color:#1db954;"">{actionLabel}</strong>
                cho tài khoản TuneVault gắn với địa chỉ email này.
              </p>

              <p style=""margin:0 0 12px;color:#aaaaaa;font-size:13px;text-align:center;"">
                MÃ XÁC NHẬN CỦA BẠN
              </p>

              <!-- OTP BOX -->
              <div style=""background-color:#111111;border:2px solid #1db954;border-radius:12px;
                           padding:24px;text-align:center;margin:0 0 28px;"">
                <span style=""font-size:42px;font-weight:700;letter-spacing:12px;
                              color:#1db954;font-family:'Courier New',monospace;"">
                  {otpCode}
                </span>
              </div>

              <p style=""margin:0 0 24px;color:#aaaaaa;font-size:13px;text-align:center;"">
                ⏱ Mã có hiệu lực trong <strong style=""color:#e0e0e0;"">5 phút</strong>
              </p>

              <div style=""background-color:#2a1a1a;border-left:3px solid #e74c3c;
                           border-radius:6px;padding:14px 16px;margin:0 0 24px;"">
                <p style=""margin:0;color:#e0a0a0;font-size:13px;line-height:1.5;"">
                  ⚠️ <strong>Lưu ý bảo mật:</strong> Không chia sẻ mã này với bất kỳ ai.
                  TuneVault sẽ không bao giờ hỏi mã OTP của bạn. Nếu bạn không thực hiện
                  yêu cầu này, hãy bỏ qua email này.
                </p>
              </div>

              <p style=""margin:0;color:#666666;font-size:12px;line-height:1.6;"">
                Nếu bạn gặp bất kỳ vấn đề nào, vui lòng liên hệ đội ngũ hỗ trợ của chúng tôi.
              </p>
            </td>
          </tr>

          <!-- FOOTER -->
          <tr>
            <td style=""background-color:#111111;padding:20px 40px;text-align:center;
                        border-top:1px solid #2a2a2a;"">
              <p style=""margin:0;color:#444444;font-size:11px;line-height:1.8;"">
                © 2026 TuneVault. All rights reserved.<br/>
                Email này được gửi tự động, vui lòng không trả lời.
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

        using var message = new MailMessage(senderEmail, toEmail)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation("OTP email sent successfully to {ToEmail} for purpose {Purpose}", toEmail, purpose);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {ToEmail}. SMTP Error: {SmtpErrorCode}", toEmail, ex.StatusCode);
            throw new InvalidOperationException("Failed to send OTP email. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending OTP email to {ToEmail}", toEmail);
            throw new InvalidOperationException("An unexpected error occurred while sending OTP email.", ex);
        }
    }
}