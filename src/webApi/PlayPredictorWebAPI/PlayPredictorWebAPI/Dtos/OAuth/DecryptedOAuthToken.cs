using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Dtos.OAuth
{
    public class DecryptedOAuthToken
    {
        public int Id { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public DateTime AccessTokenExpiresAt { get; set; }

        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
