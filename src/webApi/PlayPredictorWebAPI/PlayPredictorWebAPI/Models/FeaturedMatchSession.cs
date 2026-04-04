namespace PlayPredictorWebAPI.Models
{
    public class FeaturedMatchSession
    {
        public int Id { get; set; }
        public int AnalysisId { get; set; }
        public Analysis Analysis { get; set; } = null!;
    }
}
