using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Dtos.GoogleCalendar
{
    public class UserCalendarResponseDto
    {
        public string Id { get; set; } = "";
        public string Description { get; set; } = "";
        public Origin Origin { get; set; } = Origin.API;
    }
}
