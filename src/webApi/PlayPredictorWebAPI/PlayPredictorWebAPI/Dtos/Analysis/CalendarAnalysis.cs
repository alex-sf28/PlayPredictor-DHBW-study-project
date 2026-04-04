namespace PlayPredictorWebAPI.Dtos.Analysis
{
    public class CalendarAnalysis
    {
        public double AverageEventsPerDay { get; set; }
        public double AverageEventLength { get; set; }
        public int DaysWithoutEvents { get; set; }
        public Dictionary<int, double> EventActivityByHourDistribution { get; set; } = [];
        public Dictionary<DayOfWeek, double> EventActivityByWeekdayDistribution { get; set; } = [];
        public Dictionary<int, BasicPerformanceStatistik> PerformanceByEventCount { get; internal set; }
        public Dictionary<int, BasicPerformanceStatistik> PerformanceByEventDuration { get; internal set; }
        public Dictionary<int, BasicPerformanceStatistik> PerformanceByTimeSinceLastEvent { get; internal set; }
    }
}
