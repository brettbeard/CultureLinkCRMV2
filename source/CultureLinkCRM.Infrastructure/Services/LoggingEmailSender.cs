using CultureLinkCRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CultureLinkCRM.Infrastructure.Services;

/// <summary>
/// Placeholder IEmailSender implementation: logs the reset link instead of sending mail.
/// Per SRS FR-12, the concrete transactional-provider-vs-SMTP-relay decision is deferred pending CultureLink's
/// choice of mail system; this keeps the password-reset flow functional (visible via logs) until that's wired up.
/// </summary>
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken ct = default)
    {
        logger.LogInformation("Password reset requested for {Email}. Reset link: {ResetLink}", toEmail, resetLink);
        return Task.CompletedTask;
    }
}
