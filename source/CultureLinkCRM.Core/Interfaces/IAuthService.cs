using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;

namespace CultureLinkCRM.Core.Interfaces;

public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
    Task<ServiceResult> RequestPasswordResetAsync(string email, string resetLinkBaseUrl, CancellationToken ct = default);
    Task<ServiceResult> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);
}

public interface IUserService
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<User>> CreateAsync(string email, string password, CultureLinkCRM.Core.Enums.UserRole role, CancellationToken ct = default);
    Task<ServiceResult<User>> UpdateRoleAsync(int id, CultureLinkCRM.Core.Enums.UserRole role, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>Deferred concrete implementation per SRS FR-12 pending CultureLink's choice of mail system; interface unblocks the reset flow now.</summary>
public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken ct = default);
}

public interface ISettingsService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, CancellationToken ct = default);
}
