namespace PlayPredictorWebAPI.Models
{
    public class FaceitAccount
    {
        public int Id { get; set; }

        public required string PlayerId { get; set; }

        public required User User { get; set; }

        public int UserId { get; set; }

        public ICollection<Match> Matches { get; set; } = new List<Match>();

    }
}
