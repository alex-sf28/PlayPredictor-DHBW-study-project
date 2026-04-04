namespace PlayPredictorWebAPI.Dtos.GoogleCalendar
{
    public class CalendarEventsRequestDto
    {
        public string CalendarId { get; set; } = string.Empty;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string? SyncToken { get; set; }
        public int MaxResults { get; set; } = 250;
    }
}
