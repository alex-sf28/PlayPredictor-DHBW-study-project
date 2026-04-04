using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Dtos.OAuth
{
    public class ConnectGoogleCalendarResponseDto
    {
        public string ProviderUserId { get; set; } = "";
        public string Email { get; set; } = "";
        public DecryptedOAuthToken OAuthToken { get; set; } = null!;
        public Models.User User { get; set; } = null!;
    }
}
