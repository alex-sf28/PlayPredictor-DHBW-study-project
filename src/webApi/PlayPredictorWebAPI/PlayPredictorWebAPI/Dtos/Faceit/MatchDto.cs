using System.Text.Json.Serialization;

namespace PlayPredictorWebAPI.Dtos.Faceit
{
    public class MatchDto
    {
        [JsonPropertyName("Match Id")]
        public string MatchId { get; set; } = "";

        [JsonPropertyName("Player Id")]
        public string PlayerId { get; set; } = "";
        [JsonPropertyName("Game Mode")]
        public string GameMode { get; set; } = "";

        [JsonPropertyName("Created At")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("Match Finished At")]
        public long FinishedAt { get; set; }

        // Identifikation & Metadaten

        public string Map { get; set; } = "";

        // Spieler-Statistiken
        public string Kills { get; set; } = "";
        public string Deaths { get; set; } = "";
        public string Assists { get; set; } = "";

        [JsonPropertyName("K/D Ratio")]
        public string KdRatio { get; set; } = ""; // "0.5"
        [JsonPropertyName("K/R Ratio")]
        public string KrRatio { get; set; } = "";// "0.44"

        public string Adr { get; set; } = "";  // "42.9"
        public string Damage { get; set; } = "";
        public string Headshots { get; set; } = "";
        [JsonPropertyName("Headshots %")]
        public string HeadshotPercentage { get; set; } = ""; // "50"
        public string Mvps { get; set; } = "";

        // Multikills
        [JsonPropertyName("Double Kills")]
        public string DoubleKills { get; set; } = "";
        [JsonPropertyName("Triple Kills")]
        public string TripleKills { get; set; } = "";
        [JsonPropertyName("Quadro Kills")]
        public string QuadroKills { get; set; } = "";
        [JsonPropertyName("Penta Kills")]
        public string PentaKills { get; set; } = "";

        // Match-Details & Ergebnis
        public string Score { get; set; } = ""; // "5 / 13"
        [JsonPropertyName("Final Score")]
        public string FinalScore { get; set; } = "";
        [JsonPropertyName("First Half Score")]
        public string FirstHalfScore { get; set; } = "";
        [JsonPropertyName("Second Half Score")]
        public string SecondHalfScore { get; set; } = "";
        [JsonPropertyName("Overtime Score")]
        public string OvertimeScore { get; set; } = "";
        public string Rounds { get; set; } = "";
        public string Result { get; set; } = "";// 0 oder 1
    }
}
