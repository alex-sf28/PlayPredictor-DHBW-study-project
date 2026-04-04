

namespace PlayPredictorWebAPI.Models
{
    public class Analysis
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int SessionPerformanceId { get; set; }
        public ICollection<SessionPerformance> SessionPerformances { get; set; } = new List<SessionPerformance>();

        public ICollection<FeaturedMatchSession> FeaturedMatchSessions { get; set; } = new List<FeaturedMatchSession>();
    }
}
