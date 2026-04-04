using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Models
{
    public enum Origin
    {
        API,
        ICalendar
    }

    public class Calendar
    {
        public int Id { get; set; }
        public string CalendarId { get; set; } = "";
        public string? SyncToken { get; set; } = "";
        public DateTimeOffset? LastStartDateUtc { get; set; }
        public DateTimeOffset? LastEndDateUtc { get; set; }
        public string TimeZoneId { get; set; } = "";
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public Origin Origin { get; set; } = Origin.API;

        public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
    }
}
