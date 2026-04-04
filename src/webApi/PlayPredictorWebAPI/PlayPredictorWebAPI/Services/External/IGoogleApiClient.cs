using g_map_compare_backend.Common.Results;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.GoogleCalendar;
using PlayPredictorWebAPI.Models;
using System.Text.Json.Serialization;
using System.Threading;

namespace PlayPredictorWebAPI.Services.External
{
    public interface IGoogleApiClient
    {
        Task<Result<GoogleTokenResponse>> ExchangeCodeForTokensAsync(string code, CancellationToken cancellationToken = default);

        Result<string> BuildOAuthUrl(string state);

        Task<Result<CalendarList>> GetUserCalendars(ExternalOAuthAccount acc, CancellationToken cancellationToken = default);

        Task<Result<Events>> GetCalendarEvents(ExternalOAuthAccount acc, CalendarEventsRequestDto reqDto, CancellationToken cancellationToken = default);
    }

    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("refresh_token_expires_in")]
        public int? RefreshTokenExpiresIn { get; set; }
        public string? IdToken { get; set; }
    }
}