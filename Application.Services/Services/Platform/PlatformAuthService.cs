using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities.Platform;
using Application.Core.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Services.Platform
{
    public interface IPlatformAuthService
    {
        Task<PlatformLoginResult> LoginAsync(string email, string password);
        Task EnsureSeedAdminAsync();
    }

    public sealed record PlatformLoginResult(string AccessToken, DateTime ExpiresOn, PlatformAdminInfo Admin);
    public sealed record PlatformAdminInfo(Guid Id, string Email, string Name, string Role);

    public sealed class PlatformAuthService : IPlatformAuthService
    {
        private readonly DataContext _db;
        private readonly PlatformAuthSettings _settings;

        public PlatformAuthService(DataContext db, IOptions<PlatformAuthSettings> settings)
        {
            _db = db;
            _settings = settings.Value;
        }

        public async Task<PlatformLoginResult> LoginAsync(string email, string password)
        {
            var admin = await _db.PlatformAdmins.FirstOrDefaultAsync(a => a.Email == email.Trim().ToLower());
            if (admin is null || !admin.IsActive || !PlatformPasswordHasher.Verify(password, admin.PasswordHash, admin.PasswordSalt))
                throw new UnauthorizedAccessException("Invalid email or password.");

            admin.LastLoginOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var (token, expires) = GenerateToken(admin);
            return new PlatformLoginResult(token, expires,
                new PlatformAdminInfo(admin.Id, admin.Email, admin.Name, admin.Role));
        }

        private (string token, DateTime expires) GenerateToken(PlatformAdmin admin)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: new[]
                {
                    new Claim(PlatformClaims.Scope, PlatformClaims.ScopeValue),
                    new Claim(PlatformClaims.AdminId, admin.Id.ToString()),
                    new Claim(PlatformClaims.Email, admin.Email),
                    new Claim(PlatformClaims.Name, admin.Name),
                    new Claim(PlatformClaims.Role, admin.Role),
                },
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        /// <summary>First-run: create the seed admin from config when the table is empty.</summary>
        public async Task EnsureSeedAdminAsync()
        {
            if (string.IsNullOrWhiteSpace(_settings.SeedEmail) || string.IsNullOrWhiteSpace(_settings.SeedPassword))
                return;
            if (await _db.PlatformAdmins.AnyAsync())
                return;

            var (hash, salt) = PlatformPasswordHasher.Create(_settings.SeedPassword);
            _db.PlatformAdmins.Add(new PlatformAdmin
            {
                Id = Guid.NewGuid(),
                Email = _settings.SeedEmail.Trim().ToLower(),
                Name = string.IsNullOrWhiteSpace(_settings.SeedName) ? "Platform Owner" : _settings.SeedName,
                Role = PlatformRoles.Owner,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();
        }
    }
}
