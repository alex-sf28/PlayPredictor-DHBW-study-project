using System;
using System.Linq;
using System.Threading.Tasks;
using g_map_compare_backend.Common.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Query;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.Analysis;
using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Services
{
    public class AnalysisService
    {
        private readonly FaceitService _faceitService;
        private int _maxTimeBetweenMatchesInMinutes;
        private int _eventDurationIntervalInMinutes;
        private int _timeSinceLastEventIntervalInMinutes;
        private int _maxTimeSinceLastEventInMinutes;
        private int _firstMatchTimeOffsetInHr; 
        private readonly CalendarService _calendarService;
        private readonly IConfiguration _configuration;

        public AnalysisService(FaceitService faceitService, IConfiguration configuration, CalendarService calendarService)
        {
            _faceitService = faceitService;
            _configuration = configuration;
            _calendarService = calendarService;
            _maxTimeBetweenMatchesInMinutes = _configuration.GetValue<int>("Faceit:MaxTimeBetweenMatches", 15);
            _eventDurationIntervalInMinutes = _configuration.GetValue<int>("Faceit:EventDurationIntervalInMinutes", 60);
            _timeSinceLastEventIntervalInMinutes = _configuration.GetValue<int>("Faceit:TimeSinceLastEventIntervalInMinutes", 15);
            _maxTimeSinceLastEventInMinutes = _configuration.GetValue<int>("Faceit:MaxTimeSinceLastEventInMinutes", 240);
            _firstMatchTimeOffsetInHr = _configuration.GetValue<int>("Faceit:FirstMatchTimeOffsetInHr", 24);
        }

        public async Task<Result<MatchAnalysis>> AnalyzeMatchesAsync(DateTimeOffset start, DateTimeOffset end)
        {
            var matchesResult = await _faceitService.GetMatchesAsync(start, end);
            if (!matchesResult.Success)
                return Result<MatchAnalysis>.Fail(matchesResult.ErrorMessage);
            var matches = matchesResult.Data;

            var matchAnalysis = new MatchAnalysis
            {
                AverageMatchesPerDay = GetAverageMatchesPerDay(matches),
                MatchesPerDayDistribution = GetMatchesPerDayDistribution(matches),
                AverageTimeBetweenMatches = GetAverageTimeBetweenMatches(matches),
                AverageMatchDuration = GetAverageMatchDuration(matches),
                ActivityByHourDistribution = GetActivityByHourDistribution(matches),
                ActivityByWeekdayDistribution = GetActivityByWeekdayDistribution(matches),
                TotalMatchCount = matches.Count,
                PerformanceByMatchesPerDay = GetPerformanceByMatchesPerDay(matches)
            };
            return Result<MatchAnalysis>.Ok(matchAnalysis);
        }

        public async Task<Result<SessionAnalysis>> AnalyzeSessionsAsync(DateTimeOffset start, DateTimeOffset end)
        {
            var matchesResult = await _faceitService.GetMatchesAsync(start, end);
            if (!matchesResult.Success)
                return Result<SessionAnalysis>.Fail(matchesResult.ErrorMessage);
            var matches = matchesResult.Data;
            var sessionAnalysis = new SessionAnalysis
            {
                AverageMatchesPerSession = GetAverageMatchesInSession(matches),
                MatchesPerSessionDistribution = GetMatchesPerSessionDistribution(matches),
                AverageTimeBetweenMatchesInSession = GetAverageTimeBetweenMatchesInSession(matches),
                AverageTimeBetweenSessions = GetAverageTimeBetweenSessions(matches),
                BreakTime = _maxTimeBetweenMatchesInMinutes,
                PerformanceByMatchIndexSession = GetPerformanceByMatchIndexSession(matches)
            };

            return Result<SessionAnalysis>.Ok(sessionAnalysis);
        }

        public async Task<Result<PerformanceAnalysis>> AnalyzePerformanceAsync(DateTimeOffset start, DateTimeOffset end)
        {
            var matchesResult = await _faceitService.GetMatchesAsync(start, end);
            if (!matchesResult.Success)
                return Result<PerformanceAnalysis>.Fail(matchesResult.ErrorMessage);
            var matches = matchesResult.Data;
            var performanceAnalysis = new PerformanceAnalysis
            {
                AveragePerfomance = GetAveragePerformance(matches),
                AverageKd = GetAverageKd(matches),
                AverageKillsPerMatch = GetAverageKillsPerMatch(matches),
                AverageDeathsPerMatch = GetAverageDeathsPerMatch(matches),
                Winrate = GetWinrate(matches),
                AverageAdr = GetAverageAdr(matches),
                PerformanceByWeekdayDistribution = GetAveragePerformancePerWeekday(matches),
                PerformanceByHourDistribution = GetAveragePerformancePerHrDistribution(matches),
            };

            return Result<PerformanceAnalysis>.Ok(performanceAnalysis);
        }

        public async Task<Result<CalendarAnalysis>> AnalyzeCalendarAsync(DateTimeOffset start, DateTimeOffset end)
        {
            var matchesResult = await _faceitService.GetMatchesAsync(start, end);
            if (!matchesResult.Success)
                return Result<CalendarAnalysis>.Fail(matchesResult.ErrorMessage);

            var earliestDate = start;
            var latestDate = end;
            var matches = matchesResult.Data;

            if (matches != null && matches.Count > 0)
            {
                var matchStarts = matches
                    .Select(m => m.CreatedAt)
                    .OrderBy(d => d)
                    .ToList();

                earliestDate = matchStarts.First().Subtract(TimeSpan.FromHours(_firstMatchTimeOffsetInHr));
                latestDate = matchStarts.Last();
            }

            var eventsRes = await _calendarService.GetAllUserCalendarEventsInOneSplittedAsync(earliestDate, latestDate);
            if (!eventsRes.Success)
                return Result<CalendarAnalysis>.Fail(eventsRes.ErrorMessage);

            var events = eventsRes.Data;

            var analysis = new CalendarAnalysis
            {
                AverageEventsPerDay = GetAverageEventsPerDay(events),
                AverageEventLength = GetAverageEventLength(events),
                DaysWithoutEvents = GetDaysWithoutEvents(events),
                EventActivityByHourDistribution = GetEventActivityByHourDistribution(events),
                EventActivityByWeekdayDistribution = GetEventActivityByWeekdayDistribution(events),
                PerformanceByEventCount = GetPerformanceByEventCount(events, matches),
                PerformanceByEventDuration = GetPerformanceByEventDuration(events, matches),
                PerformanceByTimeSinceLastEvent = GetPerformanceByTimeSinceLastEvent(events, matches)
            };

            return Result<CalendarAnalysis>.Ok(analysis);
        }

        // Match Analysis

        public double GetAverageMatchesPerDay(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;

            // Verwende FinishedAt wenn vorhanden, sonst CreatedAt
            var dateTimes = matches
                .Select(m => m.FinishedAt != default ? m.FinishedAt : m.CreatedAt)
                .ToList();

            var minDate = dateTimes.Min();
            var maxDate = dateTimes.Max();

            // Tage inkl. beider Endpunkte (z. B. von 2026-03-01 bis 2026-03-01 = 1 Tag)
            var totalDays = (maxDate.Date - minDate.Date).TotalDays + 1;
            if (totalDays < 1)
                totalDays = 1;

            double averageMatchesPerDay = matches.Count / totalDays;

            return averageMatchesPerDay;
        }

        public Dictionary<int, double> GetMatchesPerDayDistribution(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return [];

            var matchesPerDay = matches
                .Select(m => (m.FinishedAt != default ? m.FinishedAt : m.CreatedAt).Date)
                .GroupBy(d => d)
                .Select(g => g.Count());

            var distributionCounts = matchesPerDay
                .GroupBy(count => count)
                .ToDictionary(g => g.Key, g => g.Count());

            int totalDays = distributionCounts.Values.Sum();

            var distribution = distributionCounts
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (double)kvp.Value / totalDays
                );

            return FillMissingIntegers(distribution);
        }

        public double GetAverageTimeBetweenMatches(ICollection<Match> matches)
        {
            if (matches == null || matches.Count < 2)
                return 0;
            var sortedMatches = matches.OrderBy(m => m.CreatedAt).ToList();
            List<double> timeDifferencesInMinutes = [];

            for (int i = 0; i < sortedMatches.Count - 1; i++)
            {
                var timeDifference = sortedMatches[i + 1].CreatedAt - sortedMatches[i].FinishedAt;
                timeDifferencesInMinutes.Add(timeDifference.TotalMinutes);
            }
            double averageTimeBetweenMatches = timeDifferencesInMinutes.Average();
            return averageTimeBetweenMatches;
        }

        public double GetAverageMatchDuration(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;

            var durations = matches
                .Select(m => (m.FinishedAt - m.CreatedAt).TotalMinutes)
                .Where(d => d >= 0).ToList();
            Console.WriteLine(durations);

            return durations.Count != 0 ? durations.Average() : 0;
        }

        public Dictionary<int, double> GetActivityByHourDistribution(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return [];

            // 0–23 Stunden vorbereiten
            var minutesPerHour = Enumerable.Range(0, 24)
                .ToDictionary(h => h, h => 0.0);

            foreach (var match in matches)
            {
                var start = match.CreatedAt;
                var end = match.FinishedAt != default ? match.FinishedAt : match.CreatedAt;

                if (end <= start)
                    continue;

                var current = start;

                while (current < end)
                {
                    var nextHour = new DateTime(
                        current.Year,
                        current.Month,
                        current.Day,
                        current.Hour,
                        0, 0).AddHours(1);

                    var segmentEnd = end < nextHour ? end : nextHour;

                    var minutes = (segmentEnd - current).TotalMinutes;

                    minutesPerHour[current.Hour] += minutes;

                    current = segmentEnd;
                }
            }

            // Gesamtzeit berechnen
            double totalMinutes = minutesPerHour.Values.Sum();

            if (totalMinutes == 0)
                return minutesPerHour.ToDictionary(kvp => kvp.Key, kvp => 0.0);

            // In Prozent umwandeln
            var distribution = minutesPerHour
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value / totalMinutes
                );

            return FillMissingIntegers(distribution);
        }

        public Dictionary<DayOfWeek, double> GetActivityByWeekdayDistribution(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return [];

            // Alle Wochentage initialisieren
            var minutesPerDay = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .ToDictionary(d => d, d => 0.0);

            foreach (var match in matches)
            {
                var start = match.CreatedAt;
                var end = match.FinishedAt != default ? match.FinishedAt : match.CreatedAt;

                if (end <= start)
                    continue;

                var current = start;

                while (current < end)
                {
                    var nextDay = current.Date.AddDays(1);

                    var segmentEnd = end < nextDay ? end : nextDay;

                    var minutes = (segmentEnd - current).TotalMinutes;

                    minutesPerDay[current.DayOfWeek] += minutes;

                    current = segmentEnd;
                }
            }

            double totalMinutes = minutesPerDay.Values.Sum();

            if (totalMinutes == 0)
                return minutesPerDay.ToDictionary(kvp => kvp.Key, kvp => 0.0);

            var distribution = minutesPerDay
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value / totalMinutes
                );

            return FillAndOrderWeekdays(distribution);
        }

        public Dictionary<int, BasicPerformanceStatistik> GetPerformanceByMatchesPerDay(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return new Dictionary<int, BasicPerformanceStatistik>();

            // Matches pro Tag sammeln (FinishedAt wenn vorhanden, sonst CreatedAt)
            var matchesByDay = matches
                .GroupBy(m => (m.FinishedAt != default ? m.FinishedAt : m.CreatedAt).Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (matchesByDay.Count == 0)
                return new Dictionary<int, BasicPerformanceStatistik>();

            // Tage nach Anzahl Matches gruppieren: key = matchesPerDay, value = alle Matches an diesen Tagen (flach)
            var groupedByCount = matchesByDay
                .GroupBy(kv => kv.Value.Count)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(kv => kv.Value).ToList()
                );

            int maxCount = groupedByCount.Keys.Max();

            var result = new Dictionary<int, BasicPerformanceStatistik>();

            // Für jede Anzahl (1..maxCount) Mittelwerte berechnen (falls keine Tage mit dieser Anzahl, Werte mit 0 belegen)
            for (int count = 1; count <= maxCount; count++)
            {
                if (!groupedByCount.TryGetValue(count, out var matchesForCount) || matchesForCount.Count == 0)
                {
                    result[count] = new BasicPerformanceStatistik();
                    continue;
                }

                // gleiche Felder wie in GetAveragePerformancePerHrDistribution verwenden
                result[count] = new BasicPerformanceStatistik
                {
                    Kills = matchesForCount.Average(m => m.Kills),
                    Deaths = matchesForCount.Average(m => m.Deaths),
                    Assists = matchesForCount.Average(m => m.Assists),
                    Damage = matchesForCount.Average(m => m.Damage),
                    KdRatio = matchesForCount.Average(m => m.KdRatio),
                    Adr = matchesForCount.Average(m => m.Adr),
                    KrRatio = matchesForCount.Average(m => m.KrRatio),
                    Headshots = matchesForCount.Average(m => m.Headshots),
                    HeadshotPercentage = matchesForCount.Average(m => m.HeadshotPercentage),
                    DoubleKills = matchesForCount.Average(m => m.DoubleKills),
                    TripleKills = matchesForCount.Average(m => m.TripleKills),
                    QuadroKills = matchesForCount.Average(m => m.QuadroKills),
                    PentaKills = matchesForCount.Average(m => m.PentaKills)
                };
            }

            return result;
        }

        // Session Analysis

        public Dictionary<int, double> GetMatchesPerSessionDistribution(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return [];

            var sortedMatches = matches
                .OrderBy(m => m.CreatedAt)
                .ToList();

            List<int> sessionLengths = new List<int>();
            int currentSessionCount = 1;

            for (int i = 1; i < sortedMatches.Count; i++)
            {
                var prev = sortedMatches[i - 1];
                var current = sortedMatches[i];

                var prevEnd = prev.FinishedAt != default ? prev.FinishedAt : prev.CreatedAt;
                var currentStart = current.CreatedAt;

                var timeDifference = currentStart - prevEnd;

                if (timeDifference.TotalMinutes <= _maxTimeBetweenMatchesInMinutes)
                {
                    currentSessionCount++;
                }
                else
                {
                    sessionLengths.Add(currentSessionCount);
                    currentSessionCount = 1;
                }
            }

            // letzte Session hinzufügen
            sessionLengths.Add(currentSessionCount);

            // Verteilung berechnen
            var distributionCounts = sessionLengths
                .GroupBy(count => count)
                .ToDictionary(g => g.Key, g => g.Count());

            int totalSessions = distributionCounts.Values.Sum();

            var distribution = distributionCounts
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => (double)kvp.Value / totalSessions
                );

            return FillMissingIntegers(distribution);
        }

        public double GetAverageMatchesInSession(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;

            if (matches.Count == 1)
                return 1;

            var sortedMatches = matches.OrderBy(m => m.CreatedAt).ToList();
            var sessionLengths = new List<int>();
            int currentSessionCount = 1;

            for (int i = 0; i < sortedMatches.Count - 1; i++)
            {
                var prev = sortedMatches[i];
                var next = sortedMatches[i + 1];

                var prevEnd = prev.FinishedAt != default ? prev.FinishedAt : prev.CreatedAt;
                var nextStart = next.CreatedAt;

                var timeDifference = nextStart - prevEnd;

                if (timeDifference.TotalMinutes <= _maxTimeBetweenMatchesInMinutes)
                {
                    currentSessionCount++;
                }
                else
                {
                    sessionLengths.Add(currentSessionCount);
                    currentSessionCount = 1;
                }
            }

            sessionLengths.Add(currentSessionCount);

            return sessionLengths.Count > 0 ? sessionLengths.Average() : 0;
        }

        public double GetAverageTimeBetweenMatchesInSession(ICollection<Match> matches)
        {
            if (matches == null || matches.Count < 2)
                return 0;

            var sortedMatches = matches.OrderBy(m => m.CreatedAt).ToList();
            var timeDifferencesInMinutes = new List<double>();

            for (int i = 0; i < sortedMatches.Count - 1; i++)
            {
                var prev = sortedMatches[i];
                var next = sortedMatches[i + 1];

                // Ende des vorherigen Matches (Fallback auf CreatedAt)
                var prevEnd = prev.FinishedAt != default ? prev.FinishedAt : prev.CreatedAt;
                var nextStart = next.CreatedAt;

                // Pause zwischen Ende des vorherigen und Start des nächsten Matches
                var timeDifference = nextStart - prevEnd;

                // Nur Pausen innerhalb einer Session zählen
                if (timeDifference.TotalMinutes >= 0 && timeDifference.TotalMinutes <= _maxTimeBetweenMatchesInMinutes)
                {
                    timeDifferencesInMinutes.Add(timeDifference.TotalMinutes);
                }
            }

            return timeDifferencesInMinutes.Count > 0 ? timeDifferencesInMinutes.Average() : 0;
        }

        public double GetAverageTimeBetweenSessions(ICollection<Match> matches)
        {
            if (matches == null || matches.Count < 2)
                return 0;
            var sortedMatches = matches.OrderBy(m => m.CreatedAt).ToList();
            List<double> sessionTimeDifferencesInMinutes = [];
            DateTimeOffset? lastSessionEndTime = null;
            for (int i = 0; i < sortedMatches.Count - 1; i++)
            {
                var timeDifference = sortedMatches[i + 1].CreatedAt - sortedMatches[i].FinishedAt;
                if (timeDifference.TotalMinutes > _maxTimeBetweenMatchesInMinutes)
                {
                    if (lastSessionEndTime.HasValue)
                    {
                        var sessionTimeDifference = sortedMatches[i + 1].CreatedAt - lastSessionEndTime.Value;
                        sessionTimeDifferencesInMinutes.Add(sessionTimeDifference.TotalMinutes);
                    }
                    lastSessionEndTime = sortedMatches[i].FinishedAt;
                }
            }
            double averageTimeBetweenSessions = sessionTimeDifferencesInMinutes.Average();
            return averageTimeBetweenSessions;
        }

        public Dictionary<int, BasicPerformanceStatistik> GetPerformanceByMatchIndexSession(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return new Dictionary<int, BasicPerformanceStatistik>();

            var sorted = matches.OrderBy(m => m.CreatedAt).ToList();

            // Sessions sammeln (Liste von Match-Listen)
            var sessions = new List<List<Match>>();
            var currentSession = new List<Match> { sorted[0] };

            for (int i = 1; i < sorted.Count; i++)
            {
                var prev = sorted[i - 1];
                var next = sorted[i];

                var prevEnd = prev.FinishedAt != default ? prev.FinishedAt : prev.CreatedAt;
                var timeDifference = next.CreatedAt - prevEnd;

                if (timeDifference.TotalMinutes <= _maxTimeBetweenMatchesInMinutes)
                {
                    currentSession.Add(next);
                }
                else
                {
                    sessions.Add(currentSession);
                    currentSession = new List<Match> { next };
                }
            }

            // letzte Session hinzufügen
            sessions.Add(currentSession);

            if (sessions.Count == 0)
                return new Dictionary<int, BasicPerformanceStatistik>();

            // Bestimme längste Session-Länge (Anzahl Matches in längster Session)
            int maxIndex = sessions.Max(s => s.Count);

            var result = new Dictionary<int, BasicPerformanceStatistik>();

            // Für jede Match-Position innerhalb einer Session (1-basierter Index) Mittelwerte berechnen
            for (int idx = 1; idx <= maxIndex; idx++)
            {
                var sessionsWithIndex = sessions.Where(s => s.Count >= idx).ToList();
                if (sessionsWithIndex.Count == 0)
                {
                    result[idx] = new BasicPerformanceStatistik();
                    continue;
                }

                var avgKills = sessionsWithIndex.Average(s => s[idx - 1].Kills);
                var avgDeaths = sessionsWithIndex.Average(s => s[idx - 1].Deaths);
                var avgAssists = sessionsWithIndex.Average(s => s[idx - 1].Assists);
                var avgDamage = sessionsWithIndex.Average(s => s[idx - 1].Damage);
                var avgKd = sessionsWithIndex.Average(s => s[idx - 1].KdRatio);
                var avgAdr = sessionsWithIndex.Average(s => s[idx - 1].Adr);
                var avgKr = sessionsWithIndex.Average(s => s[idx - 1].KrRatio);
                var avgHeadshots = sessionsWithIndex.Average(s => s[idx - 1].Headshots);
                var avgHeadshotPct = sessionsWithIndex.Average(s => s[idx - 1].HeadshotPercentage);
                var avgDouble = sessionsWithIndex.Average(s => s[idx - 1].DoubleKills);
                var avgTriple = sessionsWithIndex.Average(s => s[idx - 1].TripleKills);
                var avgQuadro = sessionsWithIndex.Average(s => s[idx - 1].QuadroKills);
                var avgPenta = sessionsWithIndex.Average(s => s[idx - 1].PentaKills);

                result[idx] = new BasicPerformanceStatistik
                {
                    Kills = avgKills,
                    Deaths = avgDeaths,
                    Assists = avgAssists,
                    Damage = avgDamage,
                    KdRatio = avgKd,
                    Adr = avgAdr,
                    KrRatio = avgKr,
                    Headshots = avgHeadshots,
                    HeadshotPercentage = avgHeadshotPct,
                    DoubleKills = avgDouble,
                    TripleKills = avgTriple,
                    QuadroKills = avgQuadro,
                    PentaKills = avgPenta
                };
            }

            return result;
        }

        // Performance Analysis

        public BasicPerformanceStatistik GetAveragePerformance(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return new BasicPerformanceStatistik();

            return new BasicPerformanceStatistik
            {
                Kills = matches.Average(m => (double)m.Kills),
                Deaths = matches.Average(m => (double)m.Deaths),
                Assists = matches.Average(m => (double)m.Assists),
                Damage = matches.Average(m => (double)m.Damage),
                KdRatio = matches.Average(m => m.KdRatio),
                Adr = matches.Average(m => m.Adr),
                KrRatio = matches.Average(m => m.KrRatio),
                Headshots = matches.Average(m => m.Headshots),
                HeadshotPercentage = matches.Average(m => m.HeadshotPercentage),
                DoubleKills = matches.Average(m => m.DoubleKills),
                TripleKills = matches.Average(m => m.TripleKills),
                QuadroKills = matches.Average(m => m.QuadroKills),
                PentaKills = matches.Average(m => m.PentaKills)
            };
        }
        public double GetAverageKd(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;

            List<double> kds = matches
                .Select(m => m.KdRatio)
                .ToList();

            double averageKd = kds.Average();
            return averageKd;
        }

        public double GetAverageKillsPerMatch(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;
            List<int> killsPerMatch = matches
                .Select(m => m.Kills)
                .ToList();
            return killsPerMatch.Average();
        }

        public double GetAverageDeathsPerMatch(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;
            List<int> deathsPerMatch = matches
                .Select(m => m.Deaths)
                .ToList();
            return deathsPerMatch.Average();
        }

        public double GetWinrate(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;

            int totalMatches = matches.Count;
            int wins = matches.Count(m => m.Won);

            double winrate = (double)wins / totalMatches;
            return winrate;
        }

        public double GetAverageAdr(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return 0;
            List<double> adrs = matches
                .Select(m => m.Adr)
                .ToList();
            return adrs.Average();
        }

        public Dictionary<DayOfWeek, double> GetAverageKdPerWeekday(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return [];

            var result = matches
                .GroupBy(m => m.CreatedAt.DayOfWeek)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(m => m.KdRatio)
                );

            return FillAndOrderWeekdays(result);
        }

        public Dictionary<int, double> GetAverageKdPerHrDistribution(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return Enumerable.Range(0, 24)
                    .ToDictionary(h => h, h => 0.0);

            // Verwende FinishedAt wenn vorhanden, sonst CreatedAt als Bezugsstunde
            var kdByHour = matches
                .Select(m => new
                {
                    Hour = (m.FinishedAt != default ? m.FinishedAt : m.CreatedAt).Hour,
                    Kd = m.KdRatio
                })
                .GroupBy(x => x.Hour)
                .ToDictionary(g => g.Key, g => g.Average(x => x.Kd));

            // Vollständige Stunden 0..23 sicherstellen (fehlende Stunden = 0.0)
            var distribution = Enumerable.Range(0, 24)
                .ToDictionary(h => h, h => kdByHour.TryGetValue(h, out var avg) ? avg : 0.0);

            return distribution;
        }

        public Dictionary<int, BasicPerformanceStatistik> GetAveragePerformancePerHrDistribution(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return Enumerable.Range(0, 24)
                    .ToDictionary(h => h, h => new BasicPerformanceStatistik());

            var statsByHour = matches
                .Select(m => new
                {
                    Hour = (m.FinishedAt != default ? m.FinishedAt : m.CreatedAt).Hour,
                    m.Kills,
                    m.Deaths,
                    m.Assists,
                    m.Damage,

                    m.KdRatio,
                    m.Adr,
                    m.Headshots,
                    m.HeadshotPercentage,
                    m.KrRatio,
                    m.DoubleKills,
                    m.TripleKills,
                    m.QuadroKills,
                    m.PentaKills
                })
                .GroupBy(x => x.Hour)
                .ToDictionary(
                    g => g.Key,
                    g => new BasicPerformanceStatistik
                    {
                        Kills = g.Average(x => x.Kills),
                        Deaths = g.Average(x => x.Deaths),
                        Assists = g.Average(x => x.Assists),
                        Damage = g.Average(x => x.Damage),
                        KdRatio = g.Average(x => x.KdRatio),
                        Adr = g.Average(x => x.Adr),
                        KrRatio = g.Average(x => x.KrRatio),
                        Headshots = g.Average(x => x.Headshots),
                        HeadshotPercentage = g.Average(x => x.HeadshotPercentage),
                        DoubleKills = g.Average(x => x.DoubleKills),
                        TripleKills = g.Average(x => x.TripleKills),
                        QuadroKills = g.Average(x => x.QuadroKills),
                        PentaKills = g.Average(x => x.PentaKills),

                    });

            // Vollständige Stunden 0..23 sicherstellen
            var result = Enumerable.Range(0, 24)
                .ToDictionary(h => h, h => statsByHour.TryGetValue(h, out var stat) ? stat : new BasicPerformanceStatistik ());

            return result;
        }

        public Dictionary<DayOfWeek, BasicPerformanceStatistik> GetAveragePerformancePerWeekday(ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0)
                return [];

            var result = matches
                .GroupBy(m => m.CreatedAt.DayOfWeek)
                .ToDictionary(
                    g => g.Key,
                    g => new BasicPerformanceStatistik
                    {
                        Kills = g.Average(x => x.Kills),
                        Deaths = g.Average(x => x.Deaths),
                        Assists = g.Average(x => x.Assists),
                        Damage = g.Average(x => x.Damage),
                        KdRatio = g.Average(x => x.KdRatio),
                        Adr = g.Average(x => x.Adr),
                        KrRatio = g.Average(x => x.KrRatio),
                        Headshots = g.Average(x => x.Headshots),
                        HeadshotPercentage = g.Average(x => x.HeadshotPercentage),
                        DoubleKills = g.Average(x => x.DoubleKills),
                        TripleKills = g.Average(x => x.TripleKills),
                        QuadroKills = g.Average(x => x.QuadroKills),
                        PentaKills = g.Average(x => x.PentaKills),

                });
            return FillAndOrderWeekdays(result);
        }

        // Calendar Analysis
        public double GetAverageEventsPerDay(ICollection<CalendarEvent> events)
        {
            if (events == null || events.Count == 0)
                return 0;

            // Nutze den Startzeitpunkt (sollte bei den gesplitteten Events gesetzt sein).
            var startDates = events
                .Where(e => e.Start.HasValue)
                .Select(e => e.Start!.Value.Date)
                .ToList();

            if (!startDates.Any())
                return 0;

            var minDate = startDates.Min();
            var maxDate = startDates.Max();

            // Tage inkl. beider Endpunkte
            var totalDays = (maxDate - minDate).TotalDays + 1;
            if (totalDays < 1)
                totalDays = 1;

            // Da Events bereits für jeden Tag gesplittet sind, reicht Gesamtanzahl durch Tage
            double averageEventsPerDay = events.Count / totalDays;

            return averageEventsPerDay;
        }

        public double GetAverageEventLength(ICollection<CalendarEvent> events)
        {
            if (events == null || events.Count == 0)
                return 0;

            // Berechne Dauer in Minuten. Berücksichtige fehlende Endwerte und EndTimeUnspecified.
            var durations = events
                .Where(e => e.Start.HasValue)
                .Select(e =>
                {
                    var start = e.Start!.Value;
                    var end = e.End ?? start;

                    double minutes = (end - start).TotalMinutes;

                    // Falls EndTimeUnspecified (All-Day) oder negative/zero Dauer vorliegt,
                    // versuche vernünftigen Fallback: ganze Tagesdauer.
                    if (e.EndTimeUnspecified && minutes <= 0)
                        minutes = 24 * 60;

                    // Vermeide negative Werte
                    return minutes > 0 ? minutes : 0.0;
                })
                .Where(d => d > 0)
                .ToList();

            return durations.Count > 0 ? durations.Average() : 0;
        }

        public Dictionary<int, double> GetEventActivityByHourDistribution(ICollection<CalendarEvent> events)
        {
            if (events == null || events.Count == 0)
                return new Dictionary<int, double>();

            // 0–23 Stunden vorbereiten
            var minutesPerHour = Enumerable.Range(0, 24)
                .ToDictionary(h => h, h => 0.0);

            foreach (var ev in events)
            {
                if (ev.Start == null)
                    continue;

                var start = ev.Start.Value;
                // Fallback: falls End fehlt und EndTimeUnspecified gesetzt ist -> ganzer Tag
                var end = ev.End ?? (ev.EndTimeUnspecified
                    ? new DateTimeOffset(start.Date.AddDays(1), start.Offset)
                    : start);

                if (end <= start)
                    continue;

                var current = start;

                while (current < end)
                {
                    var nextHour = new DateTime(
                        current.Year,
                        current.Month,
                        current.Day,
                        current.Hour,
                        0, 0).AddHours(1);

                    // nextHour als DateTime vergleichen wie in vorhandener Match-Logik
                    var segmentEnd = end < nextHour ? end : nextHour;

                    var minutes = (segmentEnd - current).TotalMinutes;

                    minutesPerHour[current.Hour] += minutes;

                    current = segmentEnd;
                }
            }

            // Gesamtzeit berechnen
            double totalMinutes = minutesPerHour.Values.Sum();

            if (totalMinutes == 0)
                return minutesPerHour.ToDictionary(kvp => kvp.Key, kvp => 0.0);

            // In Prozent umwandeln
            var distribution = minutesPerHour
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value / totalMinutes
                );

            return FillMissingIntegers(distribution);
        }

        public int GetDaysWithoutEvents(ICollection<CalendarEvent> events)
        {
            if (events == null || events.Count == 0)
                return 0;

            var eventDates = events
                .Where(e => e.Start.HasValue)
                .Select(e => e.Start!.Value.Date)
                .Distinct()
                .ToList();

            if (!eventDates.Any())
                return 0;

            var minDate = eventDates.Min();
            var maxDate = eventDates.Max();

            var totalDays = (int)((maxDate - minDate).TotalDays) + 1;
            if (totalDays < 1)
                return 0;

            var daysWithEvents = eventDates.Count;
            var daysWithoutEvents = totalDays - daysWithEvents;

            return daysWithoutEvents > 0 ? daysWithoutEvents : 0;
        }

        public Dictionary<DayOfWeek, double> GetEventActivityByWeekdayDistribution(ICollection<CalendarEvent> events)
        {
            if (events == null || events.Count == 0)
                return new Dictionary<DayOfWeek, double>();

            // Alle Wochentage initialisieren
            var minutesPerDay = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .ToDictionary(d => d, d => 0.0);

            foreach (var ev in events)
            {
                if (ev.Start == null)
                    continue;

                var start = ev.Start.Value;
                var end = ev.End ?? (ev.EndTimeUnspecified
                    ? new DateTimeOffset(start.Date.AddDays(1), start.Offset)
                    : start);

                if (end <= start)
                    continue;

                var current = start;

                while (current < end)
                {
                    var nextDay = current.Date.AddDays(1);

                    var segmentEnd = end < nextDay ? end : nextDay;

                    var minutes = (segmentEnd - current).TotalMinutes;

                    minutesPerDay[current.DayOfWeek] += minutes;

                    current = segmentEnd;
                }
            }

            double totalMinutes = minutesPerDay.Values.Sum();

            if (totalMinutes == 0)
                return minutesPerDay.ToDictionary(kvp => kvp.Key, kvp => 0.0);

            var distribution = minutesPerDay
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value / totalMinutes
                );

            return FillAndOrderWeekdays(distribution);
        }

        public Dictionary<int, BasicPerformanceStatistik> GetPerformanceByEventCount(ICollection<CalendarEvent> events, ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0 || events == null || events.Count == 0)
                return new Dictionary<int, BasicPerformanceStatistik>();

            // Gruppiere Events pro Kalendertag
            var eventsByDay = events
                .Where(e => e.Start.HasValue)
                .GroupBy(e => e.Start!.Value.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (!eventsByDay.Any())
                return new Dictionary<int, BasicPerformanceStatistik>();

            var minDate = eventsByDay.Keys.Min();
            var maxDate = eventsByDay.Keys.Max();

            // Matches nach Tag gruppieren (FinishedAt wenn vorhanden, sonst CreatedAt)
            var matchesByDay = matches
                .GroupBy(m => (m.FinishedAt != default ? m.FinishedAt : m.CreatedAt).Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Baue Mapping: für jede Tagesanzahl (Anzahl Events an diesem Tag) -> alle Matches an diesen Tagen
            var daysGroupedByEventCount = Enumerable.Range(0, (int)(maxDate - minDate).TotalDays + 1)
                .Select(offset => minDate.AddDays(offset))
                .GroupBy(day =>
                {
                    eventsByDay.TryGetValue(day, out var evList);
                    return evList?.Count ?? 0;
                })
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        // Für alle Tage mit dieser Event-Anzahl die Matches sammeln (falls vorhanden)
                        var days = g.ToList();
                        var allMatches = days
                            .SelectMany(d => matchesByDay.TryGetValue(d, out var mlist) ? mlist : new List<Match>())
                            .ToList();
                        return allMatches;
                    });

            if (!daysGroupedByEventCount.Any())
                return new Dictionary<int, BasicPerformanceStatistik>();

            int maxCount = daysGroupedByEventCount.Keys.Max();

            var result = new Dictionary<int, BasicPerformanceStatistik>();

            for (int count = 0; count <= maxCount; count++)
            {
                if (!daysGroupedByEventCount.TryGetValue(count, out var matchesForCount) || matchesForCount.Count == 0)
                {
                    result[count] = new BasicPerformanceStatistik();
                    continue;
                }

                // Im Einklang mit GetAveragePerformancePerHrDistribution: dieselben Felder mitteln
                result[count] = new BasicPerformanceStatistik
                {
                    Kills = matchesForCount.Average(m => m.Kills),
                    Deaths = matchesForCount.Average(m => m.Deaths),
                    Assists = matchesForCount.Average(m => m.Assists),
                    Damage = matchesForCount.Average(m => m.Damage),
                    KdRatio = matchesForCount.Average(m => m.KdRatio),
                    Adr = matchesForCount.Average(m => m.Adr),
                    KrRatio = matchesForCount.Average(m => m.KrRatio),
                    Headshots = matchesForCount.Average(m => m.Headshots),
                    HeadshotPercentage = matchesForCount.Average(m => m.HeadshotPercentage),
                    DoubleKills = matchesForCount.Average(m => m.DoubleKills),
                    TripleKills = matchesForCount.Average(m => m.TripleKills),
                    QuadroKills = matchesForCount.Average(m => m.QuadroKills),
                    PentaKills = matchesForCount.Average(m => m.PentaKills)
                };
            }

            return result;
        }

        public Dictionary<int, BasicPerformanceStatistik> GetPerformanceByEventDuration(ICollection<CalendarEvent> events, ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0 || events == null || events.Count == 0)
                return new Dictionary<int, BasicPerformanceStatistik>();

            // Events pro Tag summieren (Dauer in Minuten)
            var eventMinutesByDay = events
                .Where(e => e.Start.HasValue)
                .GroupBy(e => e.Start!.Value.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(e =>
                    {
                        var start = e.Start!.Value;
                        var end = e.End ?? start;
                        double minutes = (end - start).TotalMinutes;
                        if (e.EndTimeUnspecified && minutes <= 0)
                            minutes = 24 * 60;
                        return minutes > 0 ? minutes : 0.0;
                    })
                );

            if (!eventMinutesByDay.Any())
                return new Dictionary<int, BasicPerformanceStatistik>();

            var minDate = eventMinutesByDay.Keys.Min();
            var maxDate = eventMinutesByDay.Keys.Max();

            // Matches pro Tag vorbereiten (FinishedAt falls vorhanden, sonst CreatedAt)
            var matchesByDay = (matches ?? Enumerable.Empty<Match>())
                .GroupBy(m => (m.FinishedAt != default ? m.FinishedAt : m.CreatedAt).Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Tage im Bereich durchgehen und nach Intervall bucketen
            var days = Enumerable.Range(0, (int)(maxDate - minDate).TotalDays + 1)
                .Select(offset => minDate.AddDays(offset))
                .ToList();

            var bucketedMatches = new Dictionary<int, List<Match>>();

            foreach (var day in days)
            {
                eventMinutesByDay.TryGetValue(day, out var totalMinutes);
                int bucketIndex = (int)Math.Floor(totalMinutes / _eventDurationIntervalInMinutes);
                int bucketLower = bucketIndex * _eventDurationIntervalInMinutes; // z.B. 60 => 60-120

                if (!bucketedMatches.TryGetValue(bucketLower, out var list))
                {
                    list = new List<Match>();
                    bucketedMatches[bucketLower] = list;
                }

                if (matchesByDay.TryGetValue(day, out var dayMatches))
                    list.AddRange(dayMatches);
            }

            if (!bucketedMatches.Any())
                return new Dictionary<int, BasicPerformanceStatistik>();

            var result = new Dictionary<int, BasicPerformanceStatistik>();

            foreach (var kvp in bucketedMatches.OrderBy(k => k.Key))
            {
                var bucket = kvp.Key;
                var list = kvp.Value;

                if (list == null || list.Count == 0)
                {
                    result[bucket] = new BasicPerformanceStatistik();
                    continue;
                }

                // Durchschnittswerte wie bei anderen Performance-Methoden
                result[bucket] = new BasicPerformanceStatistik
                {
                    Kills = list.Average(m => m.Kills),
                    Deaths = list.Average(m => m.Deaths),
                    Assists = list.Average(m => m.Assists),
                    Damage = list.Average(m => m.Damage),
                    KdRatio = list.Average(m => m.KdRatio),
                    Adr = list.Average(m => m.Adr),
                    KrRatio = list.Average(m => m.KrRatio),
                    Headshots = list.Average(m => m.Headshots),
                    HeadshotPercentage = list.Average(m => m.HeadshotPercentage),
                    DoubleKills = list.Average(m => m.DoubleKills),
                    TripleKills = list.Average(m => m.TripleKills),
                    QuadroKills = list.Average(m => m.QuadroKills),
                    PentaKills = list.Average(m => m.PentaKills)
                };
            }

            return result;
        }

        public Dictionary<int, BasicPerformanceStatistik> GetPerformanceByTimeSinceLastEvent(ICollection<CalendarEvent> events, ICollection<Match> matches)
        {
            if (matches == null || matches.Count == 0 || events == null || events.Count == 0)
                return new Dictionary<int, BasicPerformanceStatistik>();

            // Normaleisiere Event-Endzeiten (Fallback auf Start wenn End fehlt)
            var evtList = events
                .Where(e => e.Start.HasValue)
                .Select(e => new
                {
                    Start = e.Start!.Value,
                    End = e.End ?? e.Start!.Value,
                })
                .OrderBy(e => e.End)
                .ToList();

            if (!evtList.Any())
                return new Dictionary<int, BasicPerformanceStatistik>();

            // Matches nach Datum gruppiert wird später nicht benötigt — wir berechnen für jedes Match individuell
            var bucketed = new Dictionary<int, List<Match>>();

            foreach (var match in matches)
            {
                var matchStart = match.CreatedAt;
                var matchEnd = match.FinishedAt != default ? match.FinishedAt : match.CreatedAt;

                // Finde das letzte Event, das vollständig vor dem MatchStart endet
                var lastEventBeforeMatch = evtList
                    .Where(e => e.End <= matchStart)
                    .LastOrDefault();

                // Falls kein Event vor Match gefunden wurde, prüfen wir, ob ein Event mit dem Match überlappt.
                if (lastEventBeforeMatch == null)
                {
                    var overlappingEvent = evtList
                        .FirstOrDefault(e => e.Start < matchEnd && e.End > matchStart);

                    if (overlappingEvent != null)
                    {
                        // Wenn es ein überlappendes Event gibt, versuche das vorherige Event vor diesem Event zu nehmen
                        lastEventBeforeMatch = evtList
                            .Where(e => e.End <= overlappingEvent.Start)
                            .LastOrDefault();
                    }
                }

                double minutesSinceLastEvent;

                if (lastEventBeforeMatch == null)
                {
                    // Kein vorheriges Event gefunden -> behandeln wie "weiter entfernt als max"
                    minutesSinceLastEvent = _maxTimeSinceLastEventInMinutes;
                }
                else
                {
                    minutesSinceLastEvent = (matchStart - lastEventBeforeMatch.End).TotalMinutes;
                    if (minutesSinceLastEvent < 0)
                        minutesSinceLastEvent = 0;
                    if (minutesSinceLastEvent > _maxTimeSinceLastEventInMinutes)
                        minutesSinceLastEvent = _maxTimeSinceLastEventInMinutes;
                }

                // Bucket berechnen: lower bound (0, interval, 2*interval, ...)
                var bucketIndex = (int)Math.Floor(minutesSinceLastEvent / _timeSinceLastEventIntervalInMinutes);
                var bucketLower = bucketIndex * _timeSinceLastEventIntervalInMinutes;

                if (!bucketed.TryGetValue(bucketLower, out var list))
                {
                    list = new List<Match>();
                    bucketed[bucketLower] = list;
                }

                // Falls Match mit einem Event überlappt und wir keinen "lastEventBeforeMatch" fanden, überspringen wir das Match
                // (Anforderung: bei Überlappung vorheriges Event nehmen; wenn nicht vorhanden behandeln wir als max — daher hier nicht überspringen)
                list.Add(match);
            }

            if (!bucketed.Any())
                return new Dictionary<int, BasicPerformanceStatistik>();

            var result = new Dictionary<int, BasicPerformanceStatistik>();

            foreach (var kvp in bucketed.OrderBy(k => k.Key))
            {
                var bucket = kvp.Key;
                var list = kvp.Value;

                if (list == null || list.Count == 0)
                {
                    result[bucket] = new BasicPerformanceStatistik();
                    continue;
                }

                result[bucket] = new BasicPerformanceStatistik
                {
                    Kills = list.Average(m => m.Kills),
                    Deaths = list.Average(m => m.Deaths),
                    Assists = list.Average(m => m.Assists),
                    Damage = list.Average(m => m.Damage),
                    KdRatio = list.Average(m => m.KdRatio),
                    Adr = list.Average(m => m.Adr),
                    KrRatio = list.Average(m => m.KrRatio),
                    Headshots = list.Average(m => m.Headshots),
                    HeadshotPercentage = list.Average(m => m.HeadshotPercentage),
                    DoubleKills = list.Average(m => m.DoubleKills),
                    TripleKills = list.Average(m => m.TripleKills),
                    QuadroKills = list.Average(m => m.QuadroKills),
                    PentaKills = list.Average(m => m.PentaKills)
                };
            }

            return result;
        }
        // Helpers
        public static Dictionary<DayOfWeek, double> FillAndOrderWeekdays(Dictionary<DayOfWeek, double> input)
        {
            return Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .OrderBy(day => ((int)day + 6) % 7) // Montag zuerst
                .ToDictionary(day => day, day => input.ContainsKey(day) ? input[day] : 0.0);
        }

        public static Dictionary<DayOfWeek, BasicPerformanceStatistik> FillAndOrderWeekdays(Dictionary<DayOfWeek, BasicPerformanceStatistik> input)
        {
            return Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .OrderBy(day => ((int)day + 6) % 7) // Montag zuerst
                .ToDictionary(
                    day => day,
                    day =>
                    {
                        if (input != null && input.TryGetValue(day, out var value))
                        {
                            return value;
                        }
                        return new BasicPerformanceStatistik();
                    }
                );
        }

        public static Dictionary<int, double> FillMissingIntegers(Dictionary<int, double> input)
        {
            if (input == null || input.Count == 0)
                return new Dictionary<int, double>();

            int min = input.Keys.Min();
            int max = input.Keys.Max();

            var result = Enumerable.Range(min, max - min + 1)
                .ToDictionary(
                    key => key,
                    key => input.ContainsKey(key) ? input[key] : 0.0
                );

            return result;
        }

    }
}
