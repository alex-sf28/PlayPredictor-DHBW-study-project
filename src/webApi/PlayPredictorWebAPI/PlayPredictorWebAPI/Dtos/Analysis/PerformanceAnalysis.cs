namespace PlayPredictorWebAPI.Dtos.Analysis
{
    public class PerformanceAnalysis
    {
        public double AverageKd { get; set; }
        public double AverageAdr { get; set; }
        public double Winrate { get; set; }
        public double AverageKillsPerMatch { get; set; }
        public double AverageDeathsPerMatch { get; set; }
        public Dictionary<DayOfWeek, BasicPerformanceStatistik> PerformanceByWeekdayDistribution { get; set; } = [];
        public Dictionary<int, BasicPerformanceStatistik> PerformanceByHourDistribution { get; internal set; } = [];
        public BasicPerformanceStatistik AveragePerfomance { get; internal set; }
    }
}
