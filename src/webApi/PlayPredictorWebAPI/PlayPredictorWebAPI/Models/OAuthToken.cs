namespace PlayPredictorWebAPI.Models
{
    public class OAuthToken
    {
        public int Id { get; set; }

        public int ExternalOAuthAccountId { get; set; }

        public ExternalOAuthAccount ExternalOAuthAccount { get; set; } = null!;
       
        public string? EncryptedAccessToken { get; set; }
        public string? EncryptedRefreshToken { get; set; }

        public DateTime AccessTokenExpiresAt { get; set; }

        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
