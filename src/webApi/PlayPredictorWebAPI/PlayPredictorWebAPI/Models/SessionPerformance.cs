namespace PlayPredictorWebAPI.Models
{
    public enum PerformanceClass
    {
        good,
        average,
        bad
    }
    // Predicted performance of a session
    public class SessionPerformance
    {
        public int Id { get; set; }
        public double Kd { get; set; }

        public int AnalysisId { get; set; }
        public Analysis Analysis { get; set; } = null!;

        public double RelativeKd { get; set; }
        public double RelativeAdr { get; set; }
        public double PerformanceScore { get; set; }
        public PerformanceClass PerformanceClass { get; set; }
    }
}
