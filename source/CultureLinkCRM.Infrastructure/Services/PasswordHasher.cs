namespace CultureLinkCRM.Infrastructure.Services;

/// <summary>Thin wrapper around BCrypt.Net-Next so no other Infrastructure code takes a direct dependency on the hashing library (Ref: FR-12, NFR 3.2).</summary>
public static class PasswordHasher
{
    public static string Hash(string plainTextPassword) => BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

    public static bool Verify(string plainTextPassword, string hash) => BCrypt.Net.BCrypt.Verify(plainTextPassword, hash);
}
