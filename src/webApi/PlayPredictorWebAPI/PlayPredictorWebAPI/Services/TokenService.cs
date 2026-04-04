using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Data;
using g_map_compare_backend.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.OAuth;
using PlayPredictorWebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PlayPredictorWebAPI.Services
{
    public class TokenService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        private readonly IDataProtector _protector;

        public TokenService(AppDbContext context, IConfiguration configuration, UserService userService, IDataProtectionProvider provider)
        {
            _context = context;
            _configuration = configuration;
            _userService = userService;
            _protector = provider.CreateProtector("oAuthToken.v1");
        }

        public RefreshToken CreateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(30),
                Created = DateTime.UtcNow
            };
        }

        // generates the authorizatin token
        public string GenerateJwtToken(User user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };


            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result> InvalidateAllRefreshTokensAsync(User user)
        {
            var tokens = _context.RefreshTokens.Where(rt => rt.User == user);
            _context.RefreshTokens.RemoveRange(tokens);

            var saved = await _context.SaveChangesAsync();
            return saved > 0 ? Result.Ok() : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Could not invalidate refresh tokens." }, ErrorCode.UnknownError);
        }

        //public async Task<Result<DecryptedOAuthToken>> GetGoogleOAuthTokenAsync(ExternalOAuthAccount googleAccount)
        //{
        //    var token = new DecryptedOAuthToken
        //    {
        //        Id = googleAccount.Token.Id,
        //        AccessToken = _protector.Unprotect(googleAccount.Token.EncryptedAccessToken),
        //        RefreshToken = _protector.Unprotect(googleAccount.Token.EncryptedRefreshToken),
        //        AccessTokenExpiresAt = googleAccount.Token.AccessTokenExpiresAt,
        //        RefreshTokenExpiresAt = googleAccount.Token.RefreshTokenExpiresAt
        //    };

        //    return Result<DecryptedOAuthToken>.Ok(token);
        //}

        public async Task<Result> UpdateOAuthTokenAsync(OAuthToken token)
        {
            token.EncryptedRefreshToken = token.EncryptedRefreshToken;
            token.EncryptedAccessToken = token.EncryptedAccessToken;

            _context.OAuthTokens.Update(token);

            return await _context.SaveChangesAsync() > 0
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Could not update OAuth token." }, ErrorCode.UnknownError);
        }

        public Result<DecryptedOAuthToken> DecryptOAuthToken(OAuthToken token)
        {
            if (token == null)
                return Result<DecryptedOAuthToken>.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Token to decrypt is empty " });

            var decryptedToken = new DecryptedOAuthToken
            {
                Id = token.Id,
                AccessToken = _protector.Unprotect(token.EncryptedAccessToken),
                RefreshToken = _protector.Unprotect(token.EncryptedRefreshToken),
                AccessTokenExpiresAt = token.AccessTokenExpiresAt,
                RefreshTokenExpiresAt = token.RefreshTokenExpiresAt
            };

            return Result<DecryptedOAuthToken>.Ok(decryptedToken);
        }

        public Result<OAuthToken> EncryptOAuthToken(DecryptedOAuthToken token, ExternalOAuthAccount account)
        {
            if (token is null) throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrEmpty(token.AccessToken) || string.IsNullOrEmpty(token.RefreshToken))
                return Result<OAuthToken>.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "AccessToken or RefreshToken is missing." }, ErrorCode.ValidationError);

            var newToken = new OAuthToken
            {
               
                AccessTokenExpiresAt = token.AccessTokenExpiresAt,
                EncryptedAccessToken = _protector.Protect(token.AccessToken),
                EncryptedRefreshToken = _protector.Protect(token.RefreshToken),
                ExternalOAuthAccount = account,
                RefreshTokenExpiresAt = token.RefreshTokenExpiresAt
            };

            return Result<OAuthToken>.Ok(newToken);
        }
    }
}
