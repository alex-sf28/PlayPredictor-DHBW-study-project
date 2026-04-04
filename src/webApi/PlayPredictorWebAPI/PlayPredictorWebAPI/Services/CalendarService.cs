using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Data;
using g_map_compare_backend.Services;
using Google.Apis.Calendar.v3.Data;
using Ical.Net;
using Microsoft.EntityFrameworkCore;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.google_calendar;
using PlayPredictorWebAPI.Dtos.GoogleCalendar;
using PlayPredictorWebAPI.Models;
using PlayPredictorWebAPI.Services.External;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PlayPredictorWebAPI.Services
{
    public class CalendarService
    {
        private readonly OAuthService _oAuthService;
        private readonly IGoogleApiClient _googleApiClient;
        private readonly UserService _userService;
        private readonly AppDbContext _context;

        public CalendarService(OAuthService oAuthService, IGoogleApiClient googleApiClient, UserService userService, AppDbContext context)
        {
            _oAuthService = oAuthService;
            _googleApiClient = googleApiClient;
            _userService = userService;
            _context = context;
        }
        public async Task<Result<ICollection<UserCalendarResponseDto>>> GetUserCalendars()
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result<ICollection<UserCalendarResponseDto>>.Fail(user.ErrorMessage);

            var calendars = new List<UserCalendarResponseDto>();

            var acc = await _oAuthService.GetUserGoogleAccountAsync();
            if (acc.Success)
            {
                var res = await _googleApiClient.GetUserCalendars(acc.Data);
                if (!res.Success)
                    return Result<ICollection<UserCalendarResponseDto>>.Fail(res.ErrorMessage);

                foreach(var cal in res.Data.Items)
                {
                    calendars.Add(new UserCalendarResponseDto
                    {
                        Id = cal.Id,
                        Description = cal.Description,
                    });
                }
            }

            var lokalCalendars = _context.Calendars.Where(c => c.UserId == user.Data.Id && c.Origin == Origin.ICalendar).ToList();
            foreach (var calendar in lokalCalendars)
            {
                calendars.Add(new UserCalendarResponseDto
                {
                    Id = calendar.CalendarId,
                    Origin = Origin.ICalendar
                });
            }

            return Result <ICollection<UserCalendarResponseDto>>.Ok(calendars);
        }

        public async Task<Result> SetActiveCalendars(ICollection<ActiveCalendarDto> dtos)
        {
            if (dtos == null || dtos.Count <= 0)
            {
                return Result.Fail(
                    new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "No calendar IDs provided." },
                    ErrorCode.ValidationError);
            }

            var userRes = await _userService.GetLoggedInUserAsync();
            if (!userRes.Success)
            {
                return Result.Fail(userRes.ErrorMessage, userRes.ErrorCode);
            }

            var user = userRes.Data;

            try
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                var existing = await _context.Calendars
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();

                // Neue IDs vorbereiten
                var newIds = dtos
                    .Select(d => d.CalendarId?.Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToHashSet();

                var existingIds = existing
                    .Select(c => c.CalendarId)
                    .ToHashSet();

                // 1. Löschen: alles was NICHT mehr in dtos ist
                var toRemove = existing
                    .Where(c => !newIds.Contains(c.CalendarId))
                    .ToList();

                if (toRemove.Any())
                {
                    _context.Calendars.RemoveRange(toRemove);
                }

                // 2. Hinzufügen: alles was noch nicht existiert
                var toAdd = newIds
                    .Where(id => !existingIds.Contains(id))
                    .Select(id => new Models.Calendar
                    {
                        CalendarId = id,
                        UserId = user.Id
                    })
                    .ToList();

                if (toAdd.Any())
                {
                    await _context.Calendars.AddRangeAsync(toAdd);
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(
                    new ErrorMessage { Type = ErrorType.UNKNOWN, Details = $"Failed to set active calendars: {ex.Message}" },
                    ErrorCode.UnknownError);
            }
        }


        public async Task<Result<ICollection<Models.Calendar>>> GetActiveCalendar()
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result<ICollection<Models.Calendar>>.Fail(user.ErrorMessage, user.ErrorCode);

            var calendars = await _context.Calendars.Where(c => c.UserId == user.Data.Id)
                .ToListAsync();

            return Result<ICollection<Models.Calendar>>.Ok(calendars);
        }

        public async Task<Result<ICollection<Models.Calendar>>> GetActiveCalendar(User user)
        {

            if (user == null)
                return Result<ICollection<Models.Calendar>>.Fail(new ErrorMessage { Details = "No user found", Type = ErrorType.USER_NOT_FOUND }, ErrorCode.NotFound);

            var calendars = await _context.Calendars.Where(c => c.UserId == user.Id)
                .ToListAsync();

            return Result<ICollection<Models.Calendar>>.Ok(calendars);
        }

        public async Task<Result> DeleteUserCalendars()
        {
            var user = await _userService.GetLoggedInUserAsync();

            var calendarRes = await GetActiveCalendar();
            if (!calendarRes.Success)
                return Result.Fail(calendarRes.ErrorMessage, calendarRes.ErrorCode);

            if (calendarRes.Data.Count > 0)
            {
                foreach (Models.Calendar calendar in calendarRes.Data)
                {
                    _context.Calendars.RemoveRange(calendar);
                }
                return await _context.SaveChangesAsync() > 0
                    ? Result.Ok()
                    : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Calendars could not be deleted" }, ErrorCode.UnknownError);
            }

            return Result.Ok();
        }

        public async Task<Result> DeleteUserCalendars(User user)
        {
            var calendarRes = await GetActiveCalendar(user);
            if (!calendarRes.Success)
                return Result.Fail(calendarRes.ErrorMessage, calendarRes.ErrorCode);

            if (calendarRes.Data.Count > 0)
            {
                foreach (Models.Calendar calendar in calendarRes.Data)
                {
                    _context.Calendars.RemoveRange(calendar);
                }
                return await _context.SaveChangesAsync() > 0
                    ? Result.Ok()
                    : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Calendars could not be deleted" }, ErrorCode.UnknownError);
            }

            return Result.Ok();
        }

        public async Task<Result> DeleteUserAPICalendars(User user)
        {
            var calendarRes = await GetActiveCalendar(user);
            if (!calendarRes.Success)
                return Result.Fail(calendarRes.ErrorMessage, calendarRes.ErrorCode);

            if (calendarRes.Data.Count > 0)
            {
                foreach (Models.Calendar calendar in calendarRes.Data)
                {
                    if(calendar.Origin == Origin.API)
                        _context.Calendars.RemoveRange(calendar);
                }
                await _context.SaveChangesAsync();
            }

            return Result.Ok();
        }

        public async Task<Result<ICollection<CalendarEvent>>> GetCalendarEvents(Models.Calendar calendar, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result<ICollection<CalendarEvent>>.Fail(user.ErrorMessage, user.ErrorCode);


            if (calendar.Origin == Origin.API)
            {
                var googleAcc = await _oAuthService.GetUserGoogleAccountAsync();
                if (!googleAcc.Success)
                    return Result<ICollection<CalendarEvent>>.Fail(googleAcc.ErrorMessage, googleAcc.ErrorCode);

                var eventReq = new CalendarEventsRequestDto
                {
                    CalendarId = calendar.CalendarId,
                    SyncToken = calendar.SyncToken
                };

                if (startDate != calendar.LastStartDateUtc || endDate != calendar.LastEndDateUtc)
                {
                    eventReq.StartDate = startDate;
                    eventReq.EndDate = endDate;
                }

                var res = await _googleApiClient.GetCalendarEvents(googleAcc.Data, eventReq);
                if (!res.Success)
                    return Result<ICollection<CalendarEvent>>.Fail(res.ErrorMessage, res.ErrorCode);

                calendar.SyncToken = res.Data.NextSyncToken;
                calendar.LastStartDateUtc = startDate.ToUniversalTime();
                calendar.LastEndDateUtc = endDate.ToUniversalTime();
                calendar.TimeZoneId = res.Data.TimeZone;

                var savedCalendar = await _context.SaveChangesAsync();

                var savedEvents = await UpdateCalendarEventsAsync(calendar, res.Data);
                if(!savedEvents.Success)
                    Result<ICollection<CalendarEvent>>.Fail(savedEvents.ErrorMessage, savedEvents.ErrorCode);
            }
            var resEvents = await _context.CalendarEvents
                .Where(ce => ce.CalendarId == calendar.Id && ce.Start >= startDate && ce.Start <= endDate)
                .ToListAsync();

            return Result<ICollection<CalendarEvent>>.Ok(resEvents);               
        }

        public async Task<Result<ICollection<CalendarEventsDto>>> GetAllUserCallendarEventsAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result<ICollection<CalendarEventsDto>>.Fail(user.ErrorMessage, user.ErrorCode);

            var calendarIds = await GetActiveCalendar();
            if (!calendarIds.Success)
                return Result<ICollection<CalendarEventsDto>>.Fail(calendarIds.ErrorMessage, calendarIds.ErrorCode);

            var allEvents = new List<CalendarEventsDto>();
            foreach (var calendar in calendarIds.Data)
            {
                var res = await GetCalendarEvents(calendar, startDate, endDate);
                if (!res.Success)
                    return Result<ICollection<CalendarEventsDto>>.Fail(res.ErrorMessage, res.ErrorCode);

                allEvents.Add(new CalendarEventsDto
                {
                    CalendarId = calendar.CalendarId,
                    Events = res.Data ?? new List<CalendarEvent>()
                });

            }

            return Result<ICollection<CalendarEventsDto>>.Ok(allEvents);
        }

        public async Task<Result<ICollection<CalendarEvent>>> GetAllUserCalendarEventsInOneSplittedAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var eventList = new List<CalendarEvent>();
            var userCalendars = await GetAllUserCallendarEventsAsync(startDate, endDate);

            if (!userCalendars.Success)
                return Result<ICollection<CalendarEvent>>.Fail(userCalendars.ErrorMessage, userCalendars.ErrorCode);

            if (userCalendars.Data == null || userCalendars.Data.Count == 0)
                return Result<ICollection<CalendarEvent>>.Ok(eventList);

            foreach (var calendar in userCalendars.Data)
            {
                eventList.AddRange(calendar.Events);
            }

            return Result<ICollection<CalendarEvent>>.Ok(SplitMultiDayEvents(eventList));
        }

        public async Task<Result> UpdateCalendarEventsAsync(Models.Calendar calendar, Events events)
        {
            if (events?.Items == null || events.Items.Count == 0)
                return Result.Ok();

            var eventIds = events.Items.Select(e => e.Id).ToList();

            var existingEvents = await _context.CalendarEvents
                .Where(e => e.CalendarId == calendar.Id && eventIds.Contains(e.EventId))
                .ToDictionaryAsync(e => e.EventId);

            foreach (var evnt in events.Items)
            {
                var start = (evnt.Start?.DateTimeDateTimeOffset
                            ?? DateTimeOffset.Parse(evnt.Start.Date))
                            .ToUniversalTime();

                var end = (evnt.End?.DateTimeDateTimeOffset
                            ?? DateTimeOffset.Parse(evnt.End.Date))
                            .ToUniversalTime();

                if (existingEvents.TryGetValue(evnt.Id, out var existingEvent))
                {
                    MapEvent(existingEvent, evnt, start, end);
                }
                else
                {
                    var newEvent = new CalendarEvent
                    {
                        EventId = evnt.Id,
                        CalendarId = calendar.Id
                    };

                    MapEvent(newEvent, evnt, start, end);

                    await _context.CalendarEvents.AddAsync(newEvent);
                }
            }

            await _context.SaveChangesAsync();

            return Result.Ok();

        }

        private void MapEvent(CalendarEvent entity, Event evnt, DateTimeOffset start, DateTimeOffset end)
        {
            entity.EncryptedSummary = EncryptSensitiveData(evnt.Summary);
            entity.Start = start;
            entity.End = end;
            entity.Status = evnt.Status == "cancelled"
                ? Models.EventStatus.Cancelled
                : Models.EventStatus.Confirmed;

            entity.EndTimeUnspecified = evnt.EndTimeUnspecified ?? false;

            entity.Type = evnt.EventType == "birthday"
                ? EventType.Birthday
                : EventType.Default;
        }

        public ICollection<CalendarEvent> SplitMultiDayEvents(ICollection<CalendarEvent> events)
        {
            if (events == null || events.Count == 0)
                return new List<CalendarEvent>();

            var result = new List<CalendarEvent>();

            foreach (var ev in events)
            {
                if (ev.Start == null || ev.End == null)
                {
                    result.Add(ev);
                    continue;
                }

                var start = ev.Start.Value;
                var end = ev.End.Value;

                // Kein gültiger Zeitraum oder nur ein Tag -> original belassen
                if (end <= start || start.Date == end.Date)
                {
                    result.Add(ev);
                    continue;
                }

                var currentStart = start;

                // Segmentiere an Mitternacht (im Offset von currentStart) bis zum Endzeitpunkt
                while (currentStart < end)
                {
                    var nextMidnight = new DateTimeOffset(currentStart.Date.AddDays(1), currentStart.Offset);
                    var segmentEnd = nextMidnight < end ? nextMidnight : end;

                    var segment = new CalendarEvent
                    {
                        // Id bewusst nicht übernommen (neue Instanz), EventId bleibt gleich
                        EventId = ev.EventId,
                        EncryptedSummary = ev.EncryptedSummary,
                        Start = currentStart,
                        End = segmentEnd,
                        Status = ev.Status,
                        CalendarId = ev.CalendarId,
                        EndTimeUnspecified = ev.EndTimeUnspecified,
                        Type = ev.Type
                    };

                    result.Add(segment);

                    currentStart = segmentEnd;
                }
            }

            return result;
        }

        public async Task<Result> AddCalendarsFromFile(List<IFormFile> files)
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return user;

            var validationResult = await CheckIfFilesAreICalendar(files);
            if (!validationResult.Success)
                return validationResult;

            try
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var file in files)
                {
                    // Lese Dateiinhalt
                    string content;
                    using (var stream = file.OpenReadStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        content = await reader.ReadToEndAsync();
                    }

                    var ical = Ical.Net.Calendar.Load(content);

                    // Bestimme eine CalendarId: falls der ICS-Name vorhanden ist, nutze ihn, ansonsten Fallback auf Dateiname + Hash
                    var icalName = ical.Properties.FirstOrDefault(p => p.Name?.Equals("X-WR-CALNAME", StringComparison.OrdinalIgnoreCase) == true)?.Value
                                   ?? ical.Properties.FirstOrDefault(p => p.Name?.Equals("NAME", StringComparison.OrdinalIgnoreCase) == true)?.Value
                                   ?? file.FileName;

                    // Erzeuge einen stabilen, kürzeren Identifier (Hash) um Duplikate zu vermeiden
                    var calendarIdentifier = icalName + "_FILE";

                    // Prüfe ob Kalender bereits für den User existiert
                    var dbCalendar = await _context.Calendars
                        .FirstOrDefaultAsync(c => c.UserId == user.Data.Id && c.CalendarId == calendarIdentifier);

                    if (dbCalendar == null)
                    {
                        dbCalendar = new Models.Calendar
                        {
                            CalendarId = calendarIdentifier,
                            UserId = user.Data.Id,
                            TimeZoneId = ical.TimeZones.FirstOrDefault()?.Name ?? "",
                            Origin = Origin.ICalendar                        
                        };

                        await _context.Calendars.AddAsync(dbCalendar);
                        await _context.SaveChangesAsync(); // Sicherstellen, dass wir die Id bekommen
                    }

                    // Hilfsfunktion zur Konvertierung von DateTime? -> DateTimeOffset?
                    static DateTimeOffset? ToOffset(DateTime? dt)
                    {
                        if (!dt.HasValue)
                            return null;

                        var d = dt.Value;
                        if (d.Kind == DateTimeKind.Unspecified)
                            d = DateTime.SpecifyKind(d, DateTimeKind.Utc);

                        return new DateTimeOffset(d).ToUniversalTime();
                    }

                    // Mappe Events und speichere, falls noch nicht vorhanden
                    foreach (var icalEvent in ical.Events)
                    {
                        var eventId = !string.IsNullOrWhiteSpace(icalEvent.Uid) ? icalEvent.Uid : Guid.NewGuid().ToString();

                        var exists = await _context.CalendarEvents
                            .AnyAsync(e => e.EventId == eventId && e.CalendarId == dbCalendar.Id);

                        if (exists)
                            continue;

                        var start = ToOffset(icalEvent.DtStart?.Value);
                        var end = ToOffset(icalEvent.DtEnd?.Value);

                        // Falls kein End vorhanden, versuche Duration, ansonsten End = Start
                        if (!end.HasValue && icalEvent.Duration != null && start.HasValue)
                        {
                            end = start.Value.AddMinutes(icalEvent.Duration.Value.Minutes ?? 0);
                        }
                        if (!end.HasValue && start.HasValue)
                        {
                            end = start;
                        }

                        var statusString = icalEvent.Status?.ToString();
                        var status = (!string.IsNullOrWhiteSpace(statusString) && statusString.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase))
                            ? Models.EventStatus.Cancelled
                            : Models.EventStatus.Confirmed;

                        var isAllDay = icalEvent.IsAllDay;

                        var newEvent = new CalendarEvent
                        {
                            EventId = eventId,
                            EncryptedSummary = EncryptSensitiveData(icalEvent.Summary ?? string.Empty),
                            Start = start,
                            End = end,
                            Status = status,
                            EndTimeUnspecified = isAllDay,
                            Type = EventType.Default,
                            CalendarId = dbCalendar.Id
                        };

                        await _context.CalendarEvents.AddAsync(newEvent);
                    }

                    // Speichere Events dieses Kalenders
                    await _context.SaveChangesAsync();
                }

                await tx.CommitAsync();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = $"Failed to import calendars from files: {ex.Message}" }, ErrorCode.UnknownError);
            }
        }

        public async Task<Result> CheckIfFilesAreICalendar(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                // 1. Extension prüfen
                var extension = Path.GetExtension(file.FileName);
                if (!string.Equals(extension, ".ics", StringComparison.OrdinalIgnoreCase))
                    return Result.Fail(new ErrorMessage { Type = ErrorType.INVALID_TYPE, Details = "Only .ics files are allowed" });

                // 2. MIME-Type prüfen (optional)
                if (file.ContentType != "text/calendar" &&
                    file.ContentType != "application/octet-stream")
                    return Result.Fail(new ErrorMessage { Type = ErrorType.INVALID_TYPE, Details = "Invalid MIME-Type" } );

                // 3. Inhalt prüfen
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var firstLine = await reader.ReadLineAsync();

                    if (firstLine == null || !firstLine.Contains("BEGIN:VCALENDAR"))
                        return Result.Fail(new ErrorMessage { Type = ErrorType.INVALID_TYPE, Details = "Invalid ICS-File" });
                }
            }

            return Result.Ok();
        }

        public string EncryptSensitiveData(string data)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(data.ToLower().Trim());

            // Einzeiler für den Hash
            byte[] hashBytes = SHA256.HashData(inputBytes);

            // In Hex-String umwandeln
            string hashString = Convert.ToHexString(hashBytes);

            return hashString;
        }
    }
}
