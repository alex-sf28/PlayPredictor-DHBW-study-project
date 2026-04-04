using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace PlayPredictorWebAPI.Services
{

    public class OAuthStateService
    {
        private readonly IDataProtector _protector;

        public OAuthStateService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("google-oauth-state");
        }

        public string Create(string userId, string? redirectUri)
        {
            var payload = new OAuthStatePayload
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                Nonce = Guid.NewGuid().ToString(),
                RedirectUri = redirectUri
            };

            var json = JsonSerializer.Serialize(payload);
            return _protector.Protect(json);
        }

        public OAuthStatePayload Validate(string protectedState)
        {
            var json = _protector.Unprotect(protectedState);
            var payload = JsonSerializer.Deserialize<OAuthStatePayload>(json);

            if (payload.ExpiresAt < DateTime.UtcNow)
                throw new Exception("OAuth state expired");

            return payload;
        }
    }

    public class OAuthStatePayload
    {
        public string UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Nonce { get; set; }

        public string? RedirectUri { get; set; }
    }

}
