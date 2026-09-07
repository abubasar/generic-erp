using System.Security.Cryptography;
using System.Text;

namespace Application.Services.Services.Platform
{
    /// <summary>
    /// HMACSHA512 + per-record salt — the same scheme the tenant <c>User</c> store
    /// uses, kept here so the two auth stores stay independent.
    /// </summary>
    public static class PlatformPasswordHasher
    {
        public static (byte[] hash, byte[] salt) Create(string password)
        {
            using var hmac = new HMACSHA512();
            return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
        }

        public static bool Verify(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }
    }
}
