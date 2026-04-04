namespace PlayPredictorWebAPI.Models
{
    public enum EventStatus
    {
        Confirmed,
        Tentative,
        Cancelled
    }
    public enum EventType
    {
        Default,
        Birthday
    }
    public class CalendarEvent
    {
        public int Id { get; set; }
        public string EventId { get; set; } = "";
        public string EncryptedSummary { get; set; } = "";

        public DateTimeOffset? Start { get; set; }
        public DateTimeOffset? End { get; set; }
        
        public EventStatus Status { get; set; } = EventStatus.Confirmed;

        public int CalendarId { get; set; }
        public Calendar Calendar { get; set; } = null!;

        public bool EndTimeUnspecified { get; set; } = false;

        public EventType Type { get; set; } = EventType.Default;

    }
}
