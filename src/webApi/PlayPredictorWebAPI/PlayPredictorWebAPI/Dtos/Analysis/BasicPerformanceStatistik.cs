namespace PlayPredictorWebAPI.Dtos.Analysis
{
    public class BasicPerformanceStatistik
    {
        public double Kills { get; set; } = 0;
        public double Deaths { get; set; } = 0;
        public double Assists { get; set; } = 0;
        public double Damage { get; set; } = 0;
        public double KdRatio { get; set; } = 0;
        public double Adr { get; set; } = 0;
        public double KrRatio { get; set; } = 0;
        public double Headshots { get; set; } = 0;
        public double HeadshotPercentage { get; set; } = 0;

        public double DoubleKills { get; set; } = 0;
        public double TripleKills { get; set; } = 0;
        public double QuadroKills { get; set; } = 0;
        public double PentaKills { get; set; } = 0;
    }
}
