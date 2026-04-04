namespace PlayPredictorWebAPI.Dtos.Analysis
{
    public class SessionAnalysis
    {
        public Dictionary<int, double> MatchesPerSessionDistribution { get; set; } = [];
        public double AverageMatchesPerSession { get; set; }
        public double AverageTimeBetweenMatchesInSession { get; set; }
        public double AverageTimeBetweenSessions { get; set; }
        public int BreakTime { get; internal set; }
        public Dictionary<int, BasicPerformanceStatistik> PerformanceByMatchIndexSession { get; internal set; } = [];
    }
}
