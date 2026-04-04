namespace PlayPredictorWebAPI.Models
{
    public enum OAuthProvider
    {
        Google,
        Faceit
    }
    public class ExternalOAuthAccount
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public OAuthProvider Provider { get; set; } 

        // Provider User Info
        public string? ProviderUserId { get; set; } // Google "sub"
        public string? Email { get; set; }

        public OAuthToken? Token { get; set; }
        public DateTime ConnectedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }

}
