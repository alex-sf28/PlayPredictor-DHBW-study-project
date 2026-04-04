using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Data;
using g_map_compare_backend.Services;
using Google.Apis.Auth;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.Auth;
using PlayPredictorWebAPI.Dtos.Faceit;
using PlayPredictorWebAPI.Dtos.google_calendar;
using PlayPredictorWebAPI.Dtos.GoogleCalendar;
using PlayPredictorWebAPI.Dtos.OAuth;
using PlayPredictorWebAPI.Models;
using PlayPredictorWebAPI.Services.External;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;

namespace PlayPredictorWebAPI.Services
{
    public class OAuthService
    {

        private readonly UserService _userService;
        private readonly IGoogleApiClient _googleApiClient;
        private readonly OAuthStateService _stateService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly IFaceitApiClient _faceitApiClient;
        private readonly TokenService _tokenService;

        public OAuthService(
            UserService userService,
            IGoogleApiClient googleApiClient,
            OAuthStateService stateService,
            IConfiguration configuration,
            AppDbContext context,
            IFaceitApiClient faceitApiClient,
            TokenService tokenService)
        {
            _userService = userService;
            _googleApiClient = googleApiClient;
            _stateService = stateService;
            _configuration = configuration;
            _context = context;
            _faceitApiClient = faceitApiClient;
            _tokenService = tokenService;
        }

        public async Task<Result<ConnectGoogleCalendarResponseDto>> ConnectGoogleCalendar(string code, string state)
        {
            var statePayload = _stateService.Validate(state);
            if (statePayload == null)
                return Result<ConnectGoogleCalendarResponseDto>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Invalid state parameter." }, ErrorCode.ValidationError);

            var userRes = await _userService.GetUserByIdAsync(Int32.Parse(statePayload.UserId));

            if (!userRes.Success)
                return Result<ConnectGoogleCalendarResponseDto>.Fail(userRes.ErrorMessage, userRes.ErrorCode);

            var user = userRes.Data;

            var tokenResult = await _googleApiClient.ExchangeCodeForTokensAsync(code);
            if (!tokenResult.Success)
                return Result<ConnectGoogleCalendarResponseDto>.Fail(tokenResult.ErrorMessage, tokenResult.ErrorCode);

            var tokenDto = tokenResult.Data;

            // Versuche, falls vorhanden, Informationen aus id_token zu extrahieren
            string? providerUserId = null;
            string? email = null;

            if (!string.IsNullOrEmpty(tokenDto?.IdToken))
            {
                try
                {
                    var payload = await GoogleJsonWebSignature.ValidateAsync(tokenDto.IdToken, new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _configuration["Google:ClientId"] }
                    });

                    providerUserId = payload.Subject;
                    email = payload.Email;
                }
                catch
                {
                    // id_token validierung fehlgeschlagen → fallback: leave null
                }
            }

            var oauthToken = new DecryptedOAuthToken
            {
                AccessToken = tokenDto?.AccessToken,
                RefreshToken = tokenDto?.RefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenDto?.ExpiresIn ?? 0),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenDto?.RefreshTokenExpiresIn ?? 0)
            };

            return Result<ConnectGoogleCalendarResponseDto>.Ok(new ConnectGoogleCalendarResponseDto
            {
                ProviderUserId = providerUserId,
                Email = email,
                OAuthToken = oauthToken,
                User = user
            });
        }

        public async Task<Result> CreateGoogleAccount(ConnectGoogleCalendarResponseDto oAuthConnectDto)
        {

            var account = new ExternalOAuthAccount
            {
                Provider = OAuthProvider.Google,
                ProviderUserId = oAuthConnectDto.ProviderUserId,
                Email = oAuthConnectDto.Email,
                ConnectedAt = DateTime.UtcNow,
                User = oAuthConnectDto.User,
            };

            account.Token = _tokenService.EncryptOAuthToken(oAuthConnectDto.OAuthToken, account).Data;

            var externalAccounts = await GetExternalOAuthAccounts(oAuthConnectDto.User);
            var existing = externalAccounts.FirstOrDefault(a => a.Provider == OAuthProvider.Google);

            if (existing != null)
                return Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "A Google Account already exists" }, ErrorCode.ValidationError);
        
            _context.ExternalOAuthAccounts.Add(account);
            return await _context.SaveChangesAsync() > 0
                 ? Result.Ok()
                 : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Google Account could not be created" }, ErrorCode.UnknownError);
        }

        public async Task<Result> UpdateGoogleAccountAsync(string providerUserId, string email, DecryptedOAuthToken token, ExternalOAuthAccount acc)
        {
            acc.ProviderUserId = providerUserId;
            acc.Email = email;
            acc.ConnectedAt = DateTime.UtcNow;
            acc.RevokedAt = null;

            var encryptedToken = _tokenService.EncryptOAuthToken(token, acc).Data;

            if(acc.Token == null)
            {
                acc.Token = new OAuthToken
                {
                    EncryptedAccessToken = encryptedToken.EncryptedAccessToken,
                    EncryptedRefreshToken = encryptedToken.EncryptedRefreshToken,
                    AccessTokenExpiresAt = encryptedToken.AccessTokenExpiresAt,
                    RefreshTokenExpiresAt = encryptedToken.RefreshTokenExpiresAt,
                };
            }
            else
            {
                acc.Token.EncryptedAccessToken = encryptedToken.EncryptedAccessToken;
                acc.Token.EncryptedRefreshToken = encryptedToken.EncryptedRefreshToken;
                acc.Token.AccessTokenExpiresAt = encryptedToken.AccessTokenExpiresAt;
                acc.Token.RefreshTokenExpiresAt = encryptedToken.RefreshTokenExpiresAt;
            }

            _context.ExternalOAuthAccounts.Update(acc);
            return await _context.SaveChangesAsync() > 0
                 ? Result.Ok()
                 : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Google Account could not be updated" }, ErrorCode.UnknownError);
        }

        public async Task<Result<string>> StartGoogleOAuth(string? redirectUri)
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result<string>.Fail(user.ErrorMessage, user.ErrorCode);

            var state = _stateService.Create(user.Data.Id.ToString(), redirectUri);

            var url = _googleApiClient.BuildOAuthUrl(state).Data!;

            return Result<string>.Ok(url);
        }

        public async Task<IEnumerable<ExternalOAuthAccount>> GetExternalOAuthAccounts(User user)
        {
            var res = await _context.ExternalOAuthAccounts
                .Where(a => a.UserId == user.Id)
                .Include(a => a.Token)
                .ToListAsync();

            return res;
        }

        public async Task<Result> HasUserGoogleCalendar()
        {
            var user = await _userService.GetLoggedInUserAsync();

            if (!user.Success)
                return Result.Fail(user.ErrorMessage, user.ErrorCode);

            var externalAccounts = await GetExternalOAuthAccounts(user.Data);
            var hasGoogle = externalAccounts.Any(a => a.Provider == OAuthProvider.Google);

            return hasGoogle
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.GOOGLE_CALENDAR_ACCESS_EXPIRED, Details = "No Google Calendar connected." }, ErrorCode.NotFound);
        }

        public async Task<Result> DisconnectGoogleAccountAsync()
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "User not found." }, ErrorCode.NotFound);

            var googleAcc = await GetUserGoogleAccountAsync();
            if (!googleAcc.Success)
                return Result.Ok();

            _context.ExternalOAuthAccounts.Remove(googleAcc.Data);

            return await _context.SaveChangesAsync() > 0
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Google Account could not be removed" }, ErrorCode.UnknownError);
        }
        public async Task<Result<ExternalOAuthAccount>> GetUserGoogleAccountAsync()
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
            {
                return Result<ExternalOAuthAccount>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "User not found" }, ErrorCode.Unauthorized);
            }

            var externalAccoutns = await GetExternalOAuthAccounts(user.Data);
            var googleAccount = externalAccoutns.FirstOrDefault(a => a.Provider == OAuthProvider.Google);

            if (googleAccount == null)
            {
                return Result<ExternalOAuthAccount>.Fail(new ErrorMessage { Type = ErrorType.GOOGLE_CALENDAR_ACCESS_EXPIRED, Details = "No Google Calendar connected." }, ErrorCode.NotFound);
            }

            return Result<ExternalOAuthAccount>.Ok(googleAccount);
        }       
    }
}
