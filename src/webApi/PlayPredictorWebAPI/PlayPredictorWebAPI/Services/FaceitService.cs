using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Data;
using g_map_compare_backend.Services;
using Microsoft.EntityFrameworkCore;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.Faceit;
using PlayPredictorWebAPI.Models;
using PlayPredictorWebAPI.Services.External;
using System.Globalization;

namespace PlayPredictorWebAPI.Services
{
    public class FaceitService
    {
        private readonly IFaceitApiClient _faceitApiClient;
        private readonly AppDbContext _context;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public FaceitService(IFaceitApiClient faceitApiClient, AppDbContext context, UserService userService, IConfiguration configuration)
        {
            _faceitApiClient = faceitApiClient;
            _context = context;
            _userService = userService;
            _configuration = configuration;
        }


        public async Task<Result<ICollection<PlayerDto>>> SearchFaceitPlayersAsync(string nickname, int offset, int limit)
        {
            var res = await _faceitApiClient.SearchPlayersAsync(nickname, offset, limit);

            return res.Success
                ? Result<ICollection<PlayerDto>>.Ok(res.Data.Items)
                : Result<ICollection<PlayerDto>>.Fail(res.ErrorMessage, res.ErrorCode);
        }

        public async Task<Result> UpdateFaceitAccountAsync(string playerId)
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result.Fail(user.ErrorMessage, user.ErrorCode);

            var faceitAccount = await GetUserFaceitAccountAsync();
            if (!faceitAccount.Success)
                return Result.Fail(faceitAccount.ErrorMessage, faceitAccount.ErrorCode);

            var player = await _faceitApiClient.GetPlayerAsync(playerId);
            if (!player.Success)
                return Result.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "The given faceit account does not exist." }, ErrorCode.NotFound);

            if (faceitAccount.Data == null)
            {
                _context.FaceitAccounts.Add(new FaceitAccount
                {
                    PlayerId = playerId,
                    User = user.Data
                });
            }
            else
            {
                faceitAccount.Data.PlayerId = playerId;
                await DeletePlayerMatches(faceitAccount.Data.Id);
            }

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result<FaceitAccount>> GetUserFaceitAccountAsync()
        {
            var user = await _userService.GetLoggedInUserAsync();
            if (!user.Success)
                return Result<FaceitAccount>.Fail(user.ErrorMessage, user.ErrorCode);

            var faceitAccount = await _context.FaceitAccounts.FirstOrDefaultAsync(fa => fa.User == user.Data);

            return Result<FaceitAccount>.Ok(faceitAccount);
        }

        public async Task<Result> DisconnectFaceitAccountAsync()
        {
            var faceitAccount = await GetUserFaceitAccountAsync();
            if (!faceitAccount.Success)
                return Result.Fail(faceitAccount.ErrorMessage, faceitAccount.ErrorCode);

            if (faceitAccount == null)
                return Result.Ok();

            _context.FaceitAccounts.Remove(faceitAccount.Data);

            return await _context.SaveChangesAsync() > 0
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Faceitaccount could not be disconnected." }, ErrorCode.UnknownError);
        }

        public async Task<Result<PlayerDto>> GetUserPlayerAccount()
        {
            var account = await GetUserFaceitAccountAsync();
            if (!account.Success)
                return Result<PlayerDto>.Fail(account.ErrorMessage, account.ErrorCode);

            if (account.Data == null)
                return Result<PlayerDto>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "User has no faceit account" }, ErrorCode.NotFound);

            var player = await _faceitApiClient.GetPlayerAsync(account.Data.PlayerId);
            return player;
        }


        public async Task<Result<ICollection<MatchDto>>> GetMatchesFromApiAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var player = await GetUserFaceitAccountAsync();
            if (!player.Success)
                return Result<ICollection<MatchDto>>.Fail(player.ErrorMessage, player.ErrorCode);

            long start = startDate.ToUnixTimeMilliseconds();
            long end = endDate.ToUnixTimeMilliseconds();
            var limit = _configuration.GetValue<int>("Faceit:StatsEnpointLimit",80);

            var matches = new List<MatchDto>();
            var matchIds = new HashSet<string>(); 
            var reqResult = new List<MatchDto>();

            do
            {
                var matchRes = await _faceitApiClient.GetMatchesAsync(
                    "cs2",
                    player.Data.PlayerId,
                    new PlayerStatisticsRequestDto
                    {
                        From = start,
                        To = end,
                        Limit = limit,
                    }
                );

                if (!matchRes.Success)
                    return Result<ICollection<MatchDto>>.Fail(matchRes.ErrorMessage, matchRes.ErrorCode);

                reqResult = [.. matchRes.Data.Items.OrderByDescending(match => match.FinishedAt)];

                if (reqResult.Count > 0)
                {
                    foreach (var match in reqResult)
                    {
                        if (string.IsNullOrWhiteSpace(match.MatchId))
                            continue;

                       
                        if (matchIds.Add(match.MatchId))
                        {
                            matches.Add(match);
                        }
                    }

                    end = reqResult.Last().FinishedAt - 1;
                }

            } while (reqResult.Count >= limit);

            return Result<ICollection<MatchDto>>.Ok(matches);
        }

        public async Task<Result<Match>> GetLatestSavedMatchAsync()
        {
            var faceitAccountRes = await GetUserFaceitAccountAsync();
            if (!faceitAccountRes.Success)
                return Result<Match>.Fail(faceitAccountRes.ErrorMessage, faceitAccountRes.ErrorCode);

            if (faceitAccountRes.Data == null)
                return Result<Match>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "No Faceitaccount found" });

            var lastMatch = await _context.Matches
                .AsNoTracking()
                .Where(m => m.FaceitAccountId == faceitAccountRes.Data.Id)
                .OrderByDescending(m => m.FinishedAt)
                .FirstOrDefaultAsync();

            return Result<Match>.Ok(lastMatch);
        }

        public async Task<Result<Match>> GetEarliestSavedMatchAsync()
        {
            var faceitAccountRes = await GetUserFaceitAccountAsync();
            if (!faceitAccountRes.Success)
                return Result<Match>.Fail(faceitAccountRes.ErrorMessage, faceitAccountRes.ErrorCode);

            if (faceitAccountRes.Data == null)
                return Result<Match>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "No Faceitaccount found" });

            var firstMatch = await _context.Matches.Where(m => m.FaceitAccountId == faceitAccountRes.Data.Id)
                .OrderBy(m => m.FinishedAt)
                .FirstOrDefaultAsync();

            return Result<Match>.Ok(firstMatch);
        }

        public async Task<Result<ICollection<Match>>> SavePlayerMatchesAsync(ICollection<MatchDto> dto)
        {
            var faceitAccountRes = await GetUserFaceitAccountAsync();
            if (!faceitAccountRes.Success)
                return Result<ICollection<Match>>.Fail(faceitAccountRes.ErrorMessage, faceitAccountRes.ErrorCode);

            var faceitAccount = faceitAccountRes.Data;
            if (faceitAccount == null)
                return Result<ICollection<Match>>.Fail(
                    new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "Faceit account not found for user." },
                    ErrorCode.NotFound);

            if (dto == null || dto.Count == 0)
                return Result<ICollection<Match>>.Ok(new List<Match>());

            var existingMatchIds = (await _context.Matches
                .Where(m => m.FaceitAccountId == faceitAccount.Id)
                .Select(m => m.MatchId)
                .ToListAsync())
                .ToHashSet();

            var matchesToAdd = new List<Match>();

            foreach (var matchDto in dto)
            {
                if (string.IsNullOrWhiteSpace(matchDto.MatchId))
                    continue;

                if (existingMatchIds.Contains(matchDto.MatchId))
                    continue;

                var match = new Match
                {
                    MatchId = matchDto.MatchId,
                    FaceitAccountId = faceitAccount.Id,
                    GameMode = matchDto.GameMode == "5v5" ? GameMode.FiveVsFive : GameMode.Unknown,
                    CreatedAt = matchDto.CreatedAt,
                    FinishedAt = DateTimeOffset.FromUnixTimeMilliseconds(matchDto.FinishedAt).UtcDateTime,
                    Map = matchDto.Map,
                    Kills = ParseInt(matchDto.Kills),
                    Deaths = ParseInt(matchDto.Deaths),
                    Assists = ParseInt(matchDto.Assists),
                    KdRatio = ParseDouble(matchDto.KdRatio),
                    KrRatio = ParseDouble(matchDto.KrRatio),
                    Adr = ParseDouble(matchDto.Adr),
                    Damage = ParseInt(matchDto.Damage),
                    Headshots = ParseInt(matchDto.Headshots),
                    HeadshotPercentage = ParseDouble(matchDto.HeadshotPercentage),
                    Mvps = ParseInt(matchDto.Mvps),
                    DoubleKills = ParseInt(matchDto.DoubleKills),
                    TripleKills = ParseInt(matchDto.TripleKills),
                    QuadroKills = ParseInt(matchDto.QuadroKills),
                    PentaKills = ParseInt(matchDto.PentaKills),
                    Score = matchDto.Score,
                    FinalScore = ParseInt(matchDto.FinalScore),
                    FirstHalfScore = ParseInt(matchDto.FirstHalfScore),
                    SecondHalfScore = ParseInt(matchDto.SecondHalfScore),
                    OvertimeScore = ParseInt(matchDto.OvertimeScore),
                    Rounds = ParseInt(matchDto.Rounds),
                    Won = ParseInt(matchDto.Result) == 1
                };

                if (!ValidationService.IsValidMatch(match))
                    continue;

                matchesToAdd.Add(match);
            }

            if (!matchesToAdd.Any())
                return Result<ICollection<Match>>.Ok(new List<Match>());

            try
            {
                await _context.Matches.AddRangeAsync(matchesToAdd);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving matches: {ex.Message}");
            }
           

            return Result<ICollection<Match>>.Ok(matchesToAdd);
        }

        //public async Task<Result> UpdateCalendarAsync(Models.Calendar calendar)
        //{
        //    var res = await _context.Calendars.Update(calendar);
        //}


        public async Task<Result<ICollection<CalendarEvent>>> GetPersistentCalendarEventsAsync(int calendarId)
        {
            var events = await _context.CalendarEvents.Where(ce => ce.CalendarId == calendarId).ToListAsync();
            return Result<ICollection<CalendarEvent>>.Ok(events);
        }

        public async Task<Result<ICollection<Match>>> GetMatchesAsync(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var lastMatch = await GetLatestSavedMatchAsync();
            if (!lastMatch.Success)
                return Result<ICollection<Match>>.Fail(lastMatch.ErrorMessage!, lastMatch.ErrorCode);

            var firstMatch = await GetEarliestSavedMatchAsync();
            if (!firstMatch.Success)
                return Result<ICollection<Match>>.Fail(firstMatch.ErrorMessage!, lastMatch.ErrorCode);

            var faceitAccout = await GetUserFaceitAccountAsync();
            if(!faceitAccout.Success | faceitAccout.Data == null)
                return Result<ICollection<Match>>.Fail(new ErrorMessage { Type= ErrorType.USER_NOT_FOUND, Details = "No FACEIT Account found."});

            if (lastMatch.Data != null)
            {
                if (lastMatch.Data.FinishedAt < endDate)
                {
                    var newMatches = await GetMatchesFromApiAsync(lastMatch.Data.FinishedAt, endDate);
                    if (!newMatches.Success)
                        return Result<ICollection<Match>>.Fail(newMatches.ErrorMessage!, newMatches.ErrorCode);

                    var saved = await SavePlayerMatchesAsync(newMatches.Data!);
                    if (!saved.Success)
                        return Result<ICollection<Match>>.Fail(saved.ErrorMessage!, saved.ErrorCode);
                }
                if (firstMatch.Data.FinishedAt > startDate)
                {
                    var newMatches = await GetMatchesFromApiAsync(startDate, firstMatch.Data.FinishedAt);
                    if (!newMatches.Success)
                        return Result<ICollection<Match>>.Fail(newMatches.ErrorMessage!, newMatches.ErrorCode);

                    var saved = await SavePlayerMatchesAsync(newMatches.Data!);
                    if (!saved.Success)
                        return Result<ICollection<Match>>.Fail(saved.ErrorMessage!, saved.ErrorCode);
                }
            }
            else
            {
                var matchDtos = await GetMatchesFromApiAsync(startDate, endDate);
                var saved = await SavePlayerMatchesAsync(matchDtos.Data);
                if (!saved.Success)
                    return Result<ICollection<Match>>.Fail(saved.ErrorMessage!, saved.ErrorCode);
            }
            var matches = await _context.Matches
                 .Where(m => m.FaceitAccountId == faceitAccout.Data.Id &&
                             m.CreatedAt <= endDate &&
                             m.FinishedAt >= startDate)
                 .ToListAsync();

            return Result<ICollection<Match>>.Ok(matches);
        }

        public async Task<Result> DeletePlayerMatches(int faceitAccountId)
        {
            var matches = await _context.Matches.Where(m => m.FaceitAccountId == faceitAccountId).ToListAsync();
            if(matches == null || matches.Count == 0)
                return Result.Ok();

            _context.Matches.RemoveRange(matches);

            return await _context.SaveChangesAsync() > 0
                ? Result.Ok()
                : Result.Fail(new ErrorMessage { Type = ErrorType.UNKNOWN, Details = "Matches could not be deleted." }, ErrorCode.UnknownError);
        }

        public async Task<Result<bool>> HasUserSavedMatches()
        {
            var faceitAccout = await GetUserFaceitAccountAsync();
            if (!faceitAccout.Success | faceitAccout.Data == null)
                return Result<bool>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = "No FACEIT Account found." });

            return Result<bool>.Ok(_context.Matches.Any(m => m.FaceitAccount == faceitAccout.Data));
        }

        public double ParseDouble(string input)
        {
            return input.Length > 0
                ? double.Parse(input, CultureInfo.InvariantCulture)
                : 0;
        }
        public int ParseInt(string input)
        {
            return input.Length > 0 ? Int32.Parse(input) : 0;
        }
    }
}
