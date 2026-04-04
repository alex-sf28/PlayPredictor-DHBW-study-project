using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Dtos.GoogleCalendar
{
    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string EventId { get; set; } = "";
        public string Summary { get; set; } = "";

        public string Description { get; set; } = "";
        public DateTimeOffset? Start { get; set; }
        public DateTimeOffset? End { get; set; }

        public EventStatus Status { get; set; } = EventStatus.Confirmed;

        public int CalendarId { get; set; }
        
        public bool EndTimeUnspecified { get; set; } = false;

        public EventType Type { get; set; } = EventType.Default;
    }
}
