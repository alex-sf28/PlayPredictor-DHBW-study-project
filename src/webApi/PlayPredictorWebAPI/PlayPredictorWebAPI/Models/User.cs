
using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Models
{
    public enum  UserRole 
    {
        User,
        Admin
    }

    public enum AuthProvider
    {
        Google,
        Password
    }
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";

        public string? PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; internal set; }

        public UserRole Role { get; set; } = UserRole.User;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>(); 

        public AuthProvider AuthProvider { get; set; }

        public string? GoogleId { get; set; }

        public ICollection<ExternalOAuthAccount> ExternalAccounts { get; set; } = new List<ExternalOAuthAccount>();

        public ICollection<Calendar> Calendars { get; set; } = new List<Calendar>();

        public FaceitAccount FaceitAccount { get; set; } = null!;

        public ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();
    }
}
