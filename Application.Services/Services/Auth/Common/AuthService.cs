using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Core.PermissionHelpers;
using Application.Core.Settings;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services.Services.Auth.Common
{



    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IConfiguration _configuration;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        public AuthService(IConfiguration configuration, IWorkContext workContext,
             IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
        {
            _configuration = configuration;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<Result<LoginViewModel>> Login(string username, string password)
        {
            User? user = await _unitOfWork.Repository<User>().TableUnfiltered().FirstOrDefaultAsync(x => x.Username.ToLower().Equals(username.ToLower()));
            if (user == null) throw new NotFoundResultException("User Not Found");
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) throw new BadRequestException("Wrong Password");
            else
            {
                var (accessToken, refreshToken) = await AccessTokenAsync(user);
                return await Result<LoginViewModel>.SuccessAsync(new LoginViewModel { AccessToken = accessToken, RefreshToken = refreshToken, Permissions = GetAllPermissionsByUserRoleId(user.RoleId) }, "Login Successful");
            }

        }

        public async Task<Result<List<RoleClaimModel>>> GetAllPermissionsAsync(Guid roleId)
        {
            Tenant? tenant = _unitOfWork.Repository<Tenant>().TableUnfiltered().FirstOrDefault(x => x.Id == _workContext.GetTenantId());
            if (tenant is null) throw new NotFoundResultException("User has no Tenant!");
            var allPermissions = new List<RoleClaimModel>();
            allPermissions.GetAllPermissions(tenant.BusinessType);
            var role = await _unitOfWork.Repository<Role>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == roleId);
            if (role != null)
            {

                var allRoleClaims = _unitOfWork.Repository<RoleClaim>().TableNoTracking();
                var roleClaims = allRoleClaims.Where(x => x.RoleId == role.Id);
                if (roleClaims != null)
                {
                    var allClaimValues = allPermissions.Select(a => a.Value).ToList();
                    var roleClaimValues = roleClaims.Select(a => a.Value).ToList();
                    var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
                    foreach (var permission in allPermissions)
                    {
                        permission.Id = allRoleClaims?.SingleOrDefault(x => x.RoleId == roleId && x.Type == permission.Type && x.Value == permission.Value)?.Id ?? new Guid();
                        permission.RoleId = roleId;
                        if (authorizedClaims.Any(a => a == permission.Value))
                        {
                            permission.Selected = true;
                            var roleClaim = roleClaims.SingleOrDefault(a => a.Type == permission.Type && a.Value == permission.Value);
                            if (roleClaim?.Description != null)
                            {
                                permission.Description = roleClaim.Description;
                            }

                            if (roleClaim?.Group != null)
                            {
                                permission.Group = roleClaim.Group;
                            }
                        }
                    }
                }
                else
                {
                    return await Result<List<RoleClaimModel>>.FailAsync("", "No Role Claims for this role");
                }
            }
            return await Result<List<RoleClaimModel>>.SuccessAsync(allPermissions, "Permission Found");
        }


        public async Task<Result<string>> UpdatePermissionsAsync(PermissionRequest request)
        {

            var role = await _unitOfWork.Repository<Role>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == request.RoleId);
            if (role == null) throw new NotFoundResultException("Role Not Found");
            IList<RoleClaim> claims = _unitOfWork.Repository<RoleClaim>().TableNoTracking().Where(x => x.RoleId == role.Id).ToList();
            if (claims.Any())
                await _unitOfWork.Repository<RoleClaim>().DeleteAsync(claims);

            var selectedClaims = request.RoleClaims.Where(a => a.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                var roleClaim = new RoleClaim
                {
                    Id = Guid.NewGuid(),
                    RoleId = claim.RoleId,
                    Type = claim.Type,
                    Value = claim.Value,
                    Group = claim.Group,
                    Description = claim.Description,
                };
                await _unitOfWork.Repository<RoleClaim>().AddAsync(roleClaim);
            }
            await _unitOfWork.SaveChangesAsync();
            return await Result<string>.SuccessAsync("", "Permissions Updated.");
        }



        public async Task<bool> UserExists(string username)
        {
            if (await _unitOfWork.Repository<User>().TableUnfiltered().AnyAsync(x => x.Username.ToLower() == username.ToLower()))
                return true;
            return false;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public IList<string> GetAllPermissionsByUserRoleId(Guid roleId)
        {
            var permissionClaims = new List<string>();
            var roleClaims = _unitOfWork.Repository<RoleClaim>().TableUnfiltered().Where(x => x.RoleId == roleId);
            foreach (var roleClaim in roleClaims)
                permissionClaims.Add(roleClaim.Value);
            return permissionClaims;
        }
        public IEnumerable<Claim> PrepareClaims(User user)
        {
            Role? role = _unitOfWork.Repository<Role>().TableUnfiltered().FirstOrDefault(x => x.Id == user.RoleId);
            if (role == null) throw new NotFoundResultException("Role not Found");
            Tenant? tenant = _unitOfWork.Repository<Tenant>().TableUnfiltered().FirstOrDefault(x => x.Id == user.TenantId);
            if (tenant is null) throw new NotFoundResultException("User has no Tenant!");
            FinancialYear? activeFinancialYear = _unitOfWork.Repository<FinancialYear>().TableUnfiltered().FirstOrDefault(x => x.IsActive && x.TenantId == tenant.Id);
            if (activeFinancialYear == null) throw new NotFoundResultException("Active Financial Year not Found");
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, role.Name),
                new Claim(JwtConst.Username, user.Username),
                new Claim(JwtConst.UserId, user.Id.ToString()),
                new Claim(JwtConst.RoleId, user.RoleId.ToString()),
                new Claim(JwtConst.TenantId, tenant.Id.ToString()),
                new Claim(JwtConst.TenantName, tenant.Name.ToString()),
                new Claim(JwtConst.BusinessType, tenant.BusinessType.ToString()),
                new Claim(JwtConst.FYId, activeFinancialYear.Id.ToString()),
                new Claim(JwtConst.FYName, activeFinancialYear.Name),
                new Claim(JwtConst.FYStartDate, activeFinancialYear.StartDate.ToString("yyyy-MM-dd")),
                new Claim(JwtConst.FYEndDate, activeFinancialYear.EndDate.ToString("yyyy-MM-dd")),
                new Claim(JwtConst.EmployeeId, user.EmployeeId.HasValue?user.EmployeeId.Value.ToString():"")
            };
        }
        public async Task<(string accessToken, string refreshToken)> AccessTokenAsync(User user)
        {
            var accessToken = GenerateAccessToken(PrepareClaims(user));
            var refreshToken = GenerateRefreshToken();
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenId = refreshToken,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedBy = user.Id.ToString(),
                UpdatedBy = user.Id.ToString(),
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                TenantId = user.TenantId

            };
            await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshToken);
            await _unitOfWork.RefreshTokenSaveChangesAsync();

            return (accessToken, refreshToken);
        }
        public async Task<(string accessToken, string refreshToken, IList<string> permissions)> RefreshTokensAsync(string refreshToken)
        {

            // Retrieve the refresh token from the database
            var storedRefreshToken = await _unitOfWork.Repository<RefreshToken>().TableUnfiltered().FirstOrDefaultAsync(X => X.TokenId == refreshToken);
            if (storedRefreshToken is null)
            {
                throw new UnauthorizationException("Refresh Token not Found!");
            }
            if (storedRefreshToken.ExpiresUtc < DateTime.UtcNow)
            {
                storedRefreshToken.Deleted = true;
                await _unitOfWork.Repository<RefreshToken>().UpdateAsync(storedRefreshToken);
                throw new UnauthorizationException("Refresh Token validity expires!");
            }
            var userId = storedRefreshToken.CreatedBy;
            var storedRefreshTokenId = storedRefreshToken.Id;
            // Delete the old refresh token
            storedRefreshToken.Deleted = true;
            await _unitOfWork.Repository<RefreshToken>().UpdateAsync(storedRefreshToken);

            User? user = await _unitOfWork.Repository<User>().TableUnfiltered().FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            if (user is null) throw new UnauthorizationException("User not Found");
            // Generate new access and refresh tokens
            var accessToken = GenerateAccessToken(PrepareClaims(user));
            var newRefreshToken = GenerateRefreshToken();

            // Store the new refresh token in the database
            var newRefreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenId = newRefreshToken,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedBy = user.Id.ToString(),
                UpdatedBy = user.Id.ToString(),
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                TenantId = user.TenantId
            };
            await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshTokenEntity);
            await _unitOfWork.RefreshTokenSaveChangesAsync();
            return (accessToken, newRefreshToken, GetAllPermissionsByUserRoleId(user.RoleId));
        }

        public virtual async Task<string> DeleteRefreshTokenAsync(string refreshToken)
        {
            var refreshTokenEntity = await _unitOfWork.Repository<RefreshToken>().TableUnfiltered().FirstOrDefaultAsync(x => x.TokenId == refreshToken);
            if (refreshTokenEntity is not null)
            {
                await _unitOfWork.Repository<RefreshToken>().DeleteAsync(refreshTokenEntity.Id);
                await _unitOfWork.RefreshTokenSaveChangesAsync();
            }
            return refreshToken;
        }
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)
            );

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

    }
}