using System.ComponentModel;

namespace PlayPredictorWebAPI.Dtos.Faceit
{
    public class PlayerStatisticsRequestDto
    {
        public long? From { get; set; }
        public long? To { get; set; }
        public int Limit { get; set; } = 100;
        public int Offset { get; set; } = 0;

    }
}
