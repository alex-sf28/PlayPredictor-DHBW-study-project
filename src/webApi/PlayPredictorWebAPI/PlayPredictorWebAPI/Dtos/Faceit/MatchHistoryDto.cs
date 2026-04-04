using System.Text.Json.Serialization;

namespace PlayPredictorWebAPI.Dtos.Faceit
{
    public class MatchHistoryDto
    {
        [JsonPropertyName("match_id")]
        public string MatchId { get; set; } = "";
        public string Status { get; set; } = "";
        [JsonPropertyName("started_at")]
        public int StartedAt { get; set; }
        [JsonPropertyName("finished_at")]
        public int FinishedAt { get; set; }

    }
}
