using Google.Apis.Calendar.v3.Data;
using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Dtos.GoogleCalendar
{
    public class CalendarEventsDto
    {
        public string CalendarId { get; set; } = "";
        public ICollection<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    }
}
