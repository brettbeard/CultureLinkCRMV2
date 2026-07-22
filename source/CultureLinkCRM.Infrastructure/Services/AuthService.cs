using System.Security.Cryptography;
using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Core.Enums;
using CultureLinkCRM.Core.Interfaces;
using CultureLinkCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CultureLinkCRM.Infrastructure.Services;

public class AuthService(CultureLinkCrmDbContext db, IEmailSender emailSender) : IAuthService
{
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    public async Task<User?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }
        return user;
    }

    public async Task<ServiceResult> RequestPasswordResetAsync(string email, string resetLinkBaseUrl, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
        {
            // Do not reveal whether the email exists.
            return ServiceResult.Success();
        }

        user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(ResetTokenLifetime);
        await db.SaveChangesAsync(ct);

        var resetLink = $"{resetLinkBaseUrl.TrimEnd('/')}?token={user.PasswordResetToken}";
        await emailSender.SendPasswordResetEmailAsync(user.Email, resetLink, ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token, ct);
        if (user is null || user.PasswordResetTokenExpiresAt is null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult.Failure("This password reset link is invalid or has expired.");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            return ServiceResult.Failure("Password must be at least 8 characters.");
        }

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}

public class UserService(CultureLinkCrmDbContext db) : IUserService
{
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await db.Users.OrderBy(u => u.Email).ToListAsync(ct);

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<ServiceResult<User>> CreateAsync(string email, string password, UserRole role, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return ServiceResult<User>.Failure("A valid email is required.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return ServiceResult<User>.Failure("Password must be at least 8 characters.");
        }

        if (await db.Users.AnyAsync(u => u.Email == email, ct))
        {
            return ServiceResult<User>.Failure("A user with this email already exists.");
        }

        var user = new User { Email = email, PasswordHash = PasswordHasher.Hash(password), Role = role };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<User>> UpdateRoleAsync(int id, UserRole role, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return ServiceResult<User>.Failure("User not found.");
        }

        user.Role = role;
        await db.SaveChangesAsync(ct);
        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        if (user.Role == UserRole.Admin && await db.Users.CountAsync(u => u.Role == UserRole.Admin, ct) <= 1)
        {
            return ServiceResult.Failure("Cannot delete the last remaining Admin user.");
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}

public class SettingsService(CultureLinkCrmDbContext db) : ISettingsService
{
    public async Task<string?> GetAsync(string key, CancellationToken ct = default) =>
        (await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct))?.Value;

    public async Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            db.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
        await db.SaveChangesAsync(ct);
    }
}
