namespace PlayPredictorWebAPI.Models
{
    public enum GameMode
    {
        FiveVsFive,
        Unknown
    }

    public class Match
    {
        // Vorhandene Felder
        public string MatchId { get; set; } = "";

        public int FaceitAccountId { get; set; }

        public FaceitAccount FaceitAccount { get; set; } = null!;
        public GameMode GameMode { get; set; } = GameMode.FiveVsFive;
        public DateTime CreatedAt { get; set; }
        public DateTime FinishedAt { get; set; }

        // Identifikation & Metadaten
       
        public string Map { get; set; } = "";

        // Spieler-Statistiken
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public double KdRatio { get; set; } // "0.5"
        public double KrRatio { get; set; } // "0.44"
        public double Adr { get; set; }     // "42.9"
        public int Damage { get; set; }
        public int Headshots { get; set; }
        public double HeadshotPercentage { get; set; } // "50"
        public int Mvps { get; set; }

        // Multikills
        public int DoubleKills { get; set; }
        public int TripleKills { get; set; }
        public int QuadroKills { get; set; }
        public int PentaKills { get; set; }

        // Match-Details & Ergebnis
        public string Score { get; set; } = ""; // "5 / 13"
        public int FinalScore { get; set; }
        public int FirstHalfScore { get; set; }
        public int SecondHalfScore { get; set; }
        public int OvertimeScore { get; set; }
        public int Rounds { get; set; }
        public bool Won { get; set; } // 0 oder 1

    }
}
