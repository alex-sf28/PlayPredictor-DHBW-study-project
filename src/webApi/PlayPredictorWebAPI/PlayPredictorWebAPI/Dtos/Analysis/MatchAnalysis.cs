namespace PlayPredictorWebAPI.Dtos.Analysis
{
    public class MatchAnalysis
    {
        public double AverageMatchesPerDay { get; set; }
        public Dictionary<int, double> MatchesPerDayDistribution { get; set; } = [];
        public double AverageTimeBetweenMatches { get; set; }
        public double AverageMatchDuration { get; set; }
        public Dictionary<int, double> ActivityByHourDistribution { get; set; } = [];
        public Dictionary<DayOfWeek, double> ActivityByWeekdayDistribution { get; set; } = [];
        public int TotalMatchCount { get; internal set; }
        public Dictionary<int, BasicPerformanceStatistik> PerformanceByMatchesPerDay { get; internal set; }
    }
}
