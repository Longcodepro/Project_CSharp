using System.Threading;
using System.Threading.Tasks;

namespace TuneVault.Application.Abstractions;

public interface IEmailService
{
    Task SendOtpAsync(string email, string otpCode, string purpose, CancellationToken cancellationToken = default);
}