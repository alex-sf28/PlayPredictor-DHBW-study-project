using System.Text.Json.Serialization;

namespace PlayPredictorWebAPI.Dtos.Faceit
{
    public class PlayerDto
    {
        [JsonPropertyName("player_id")]
        public required string Id { get; set; }

        [JsonPropertyName("nickname")]
        public required string Nickname { get; set; }

        public string Avatar { get; set; } = string.Empty;

        [JsonPropertyName("steam_nickname")]
        public string SteamNickname { get; set; } = string.Empty;
    }
}
