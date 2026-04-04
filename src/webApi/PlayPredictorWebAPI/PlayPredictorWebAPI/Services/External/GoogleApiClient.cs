using g_map_compare_backend.Common.Results;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.GoogleCalendar;
using PlayPredictorWebAPI.Dtos.OAuth;
using PlayPredictorWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PlayPredictorWebAPI.Services.External
{
    public class GoogleApiClient : IGoogleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;

        public GoogleApiClient(HttpClient httpClient, IConfiguration configuration, TokenService tokenService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        private async Task<Result<Google.Apis.Calendar.v3.CalendarService>> PrepareService(ExternalOAuthAccount acc, CancellationToken cancellationToken = default)
        {
            if (acc == null || acc.Token == null)
            {
                return Result<Google.Apis.Calendar.v3.CalendarService>.Fail(new ErrorMessage { Type = ErrorType.GOOGLE_CALENDAR_ACCESS_EXPIRED, Details = "No Google account connected." }, ErrorCode.NotFound);
            }

            var decryptedRes = _tokenService.DecryptOAuthToken(acc.Token);
            if (!decryptedRes.Success || decryptedRes.Data == null)
            {
                return Result<Google.Apis.Calendar.v3.CalendarService>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "No valid Google OAuth token available." }, ErrorCode.Unauthorized);
            }

            var decrypted = decryptedRes.Data;

            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:Secret"];
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return Result<Google.Apis.Calendar.v3.CalendarService>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Google client credentials not configured." }, ErrorCode.UnknownError);
            }

            // Baue TokenResponse basierend auf gespeicherten (entschlüsselten) Tokens
            var tokenResponse = new TokenResponse
            {
                AccessToken = decrypted.AccessToken,
                RefreshToken = decrypted.RefreshToken,
                ExpiresInSeconds = (long?)Math.Max(0, (int)(decrypted.AccessTokenExpiresAt - DateTime.UtcNow).TotalSeconds)
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                Scopes = new[] { "https://www.googleapis.com/auth/calendar.readonly" }
            });

            var userId = acc.Id.ToString();
            var credential = new UserCredential(flow, userId, tokenResponse);

            try
            {
                // Versuche, den Refresh-Token zu verwenden, falls nötig
                var refreshed = false;
                try
                {
                    refreshed = await credential.RefreshTokenAsync(cancellationToken);
                }
                catch
                {
                    return Result<Google.Apis.Calendar.v3.CalendarService>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Error while trying to use refresh token" });
                }

                // Wenn Token aktualisiert wurden, in DB persistieren
                if (refreshed || credential.Token.AccessToken != decrypted.AccessToken)
                {
                    var decryptedToken = new DecryptedOAuthToken
                    {
                        AccessToken = credential.Token.AccessToken,
                        RefreshToken = credential.Token.RefreshToken,
                        AccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 0)
                    };
                    var encryptedToken = _tokenService.EncryptOAuthToken(decryptedToken, acc).Data;

                    // acc.Token enthält die persistierbare Entität (verschlüsselt im echten System)
                    acc.Token.EncryptedAccessToken = encryptedToken.EncryptedAccessToken;
                    acc.Token.EncryptedRefreshToken = encryptedToken.EncryptedRefreshToken ?? acc.Token.EncryptedRefreshToken;
                    acc.Token.AccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(credential.Token.ExpiresInSeconds ?? 0);

                    var updateResult = await _tokenService.UpdateOAuthTokenAsync(acc.Token);
                    if (!updateResult.Success)
                    {
                        // Log ggf. hier — wir geben trotzdem den Service zurück, weil credential gültig ist.
                    }
                }

                var service = new Google.Apis.Calendar.v3.CalendarService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _configuration["Google:ApplicationName"] ?? "PlayPredictor"
                });

                return Result<Google.Apis.Calendar.v3.CalendarService>.Ok(service);
            }
            catch (Exception ex)
            {
                return Result<Google.Apis.Calendar.v3.CalendarService>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = $"Fehler beim Vorbereiten des CalendarService: {ex.Message}" }, ErrorCode.UnknownError);
            }
        }

        public async Task<Result<GoogleTokenResponse>> ExchangeCodeForTokensAsync(string code, CancellationToken cancellationToken = default)
        {
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:Secret"];
            var redirectUri = _configuration["Google:RedirectUri"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return Result<GoogleTokenResponse>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Google client credentials not configured." }, ErrorCode.UnknownError);

            var form = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri ?? "",
                ["grant_type"] = "authorization_code"
            };

            using var content = new FormUrlEncodedContent(form);
            using var resp = await _httpClient.PostAsync("token", content, cancellationToken);

            var body = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                return Result<GoogleTokenResponse>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = $"Google token exchange failed: {resp.StatusCode} - {body}" }, ErrorCode.Unauthorized);
            }
            try
            {
                var token = JsonSerializer.Deserialize<GoogleTokenResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (token == null)
                    return Result<GoogleTokenResponse>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Failed to parse Google token response." }, ErrorCode.UnknownError);

                return Result<GoogleTokenResponse>.Ok(token);
            }
            catch (JsonException ex)
            {
                return Result<GoogleTokenResponse>.Fail(new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = $"Failed to parse Google token response: {ex.Message}" }, ErrorCode.UnknownError);
            }
        }

        public Result<string> BuildOAuthUrl(string state)
        {
            var clientId = _configuration["Google:ClientId"];
            var redirectUri = _configuration["Google:RedirectUri"];

            // Scopes als Liste binden
            var scopesList = _configuration.GetSection("Google:Scopes").Get<List<string>>();

            // Google erwartet Scopes in der URL durch Leerzeichen getrennt
            var scopes = scopesList != null ? string.Join(" ", scopesList) : null;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri) || string.IsNullOrEmpty(scopes))
            {
                return Result<string>.Fail(new ErrorMessage
                {
                    Type = ErrorType.LOGIN_FAIL,
                    Details = "Google OAuth configuration is incomplete."
                }, ErrorCode.UnknownError);
            }

            var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?client_id={Uri.EscapeDataString(clientId)}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&response_type=code" +
                      $"&scope={Uri.EscapeDataString(scopes)}" +
                      $"&access_type=offline" +
                      $"&prompt=consent" +
                      $"&state={Uri.EscapeDataString(state)}";

            return Result<string>.Ok(url);
        }

        public async Task<Result<CalendarList>> GetUserCalendars(ExternalOAuthAccount acc, CancellationToken cancellationToken = default)
        {
            try
            {
                var svcRes = await PrepareService(acc);
                if (!svcRes.Success || svcRes.Data == null)
                {
                    return Result<CalendarList>.Fail(svcRes.ErrorMessage ?? new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Could not prepare Google service." }, svcRes.ErrorCode);
                }

                var service = svcRes.Data;
                var request = service.CalendarList.List();
                var response = await request.ExecuteAsync();

                return Result<CalendarList>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<CalendarList>.Fail(new ErrorMessage
                {
                    Type = ErrorType.LOGIN_FAIL,
                    Details = $"Fehler beim Abrufen der Kalenderliste: {ex.Message}"
                }, ErrorCode.UnknownError);
            }
        }

        public async Task<Result<Events>> GetCalendarEvents(ExternalOAuthAccount acc, CalendarEventsRequestDto reqDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var svcRes = await PrepareService(acc);
                if (!svcRes.Success || svcRes.Data == null)
                {
                    return Result<Events>.Fail(svcRes.ErrorMessage ?? new ErrorMessage { Type = ErrorType.LOGIN_FAIL, Details = "Could not prepare Google service." }, svcRes.ErrorCode);
                }

                var service = svcRes.Data;


                var request = service.Events.List(reqDto.CalendarId);

                // NextSyncToken only works if no time range is specified
                // If both are provided, prioritize the time range and ignore the sync token
                if (reqDto.StartDate != null && reqDto.EndDate != null)
                {
                    request.TimeMinDateTimeOffset = reqDto.StartDate;
                    request.TimeMaxDateTimeOffset = reqDto.EndDate;
                }else if(reqDto.SyncToken != null)
                {
                    request.SyncToken = reqDto.SyncToken;
                }
                request.MaxResults = reqDto.MaxResults;


                var response = await request.ExecuteAsync();
                var allItems = new List<Event>(response.Items);

                var nextPageToken = response.NextPageToken;

                // if there are more pages, retrieve them
                while (nextPageToken != null)
                {
                    var req = service.Events.List(reqDto.CalendarId);

                    req.PageToken = nextPageToken;

                    if (reqDto.StartDate != null && reqDto.EndDate != null)
                    {
                        req.TimeMinDateTimeOffset = reqDto.StartDate;
                        req.TimeMaxDateTimeOffset = reqDto.EndDate;
                    }
                    else if (reqDto.SyncToken != null)
                    {
                        req.SyncToken = reqDto.SyncToken;
                    }

                    var res = await req.ExecuteAsync();

                    allItems.AddRange(res.Items);
                    nextPageToken = res.NextPageToken;
                }
                response.Items = allItems;

                return Result<Events>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<Events>.Fail(new ErrorMessage
                {
                    Type = ErrorType.LOGIN_FAIL,
                    Details = $"Error while retreiving calendar events: {ex.Message}"
                }, ErrorCode.UnknownError);
            }
        }
    }
}