using g_map_compare_backend.Common.Results;
using Microsoft.AspNetCore.WebUtilities;
using PlayPredictorWebAPI.Common.Results;
using PlayPredictorWebAPI.Dtos.Faceit;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PlayPredictorWebAPI.Services.External
{

    public class FaceitApiClient : IFaceitApiClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FaceitApiClient(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        private async Task<HttpResponseMessage> HttpGetRequest(string path, Dictionary<string, string>? queryParams = null)
        {
            var apiKey = _configuration["Faceit:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Faceit API key not configured (Faceit:ApiKey).");
            }

            var url = path;

            if (queryParams != null && queryParams.Count > 0)
            {
                url = QueryHelpers.AddQueryString(path, queryParams);
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            return await _httpClient.SendAsync(request).ConfigureAwait(false);
        }

        private async Task<Result<T>> Get<T>(string path, Dictionary<string, string>? queryParams = null)
        {
            var response = await HttpGetRequest(path, queryParams);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return Result<T>.Fail(new ErrorMessage { Type = ErrorType.FACEIT_ERROR, Details = errorMsg }, ErrorCode.UnknownError);
            }

            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var content = await JsonSerializer.DeserializeAsync<T>(stream, options).ConfigureAwait(false);
            return Result<T>.Ok(content);
        }

        async Task<Result<FaceitListResponse<MatchDto>>> IFaceitApiClient.GetMatchesAsync(string gameId, string playerId, PlayerStatisticsRequestDto reqDto)
        {
            var path = $"players/{playerId}/games/{gameId}/stats";

            var qParams = new Dictionary<string, string>
            {
                { "limit", reqDto.Limit.ToString() },
                { "offset", reqDto.Offset.ToString() }
            };

            if (reqDto.From.HasValue)
                qParams["from"] = reqDto.From.Value.ToString();

            if (reqDto.To.HasValue)
                qParams["to"] = reqDto.To.Value.ToString();


            var result = await Get<FaceitListResponse<StatsDto>>(path, qParams);
            var history = await GetMatchesHistoryAsync(gameId, playerId, reqDto);

            if (!history.Success)
                return Result<FaceitListResponse<MatchDto>>.Fail(history.ErrorMessage, history.ErrorCode);

            if (!result.Success)
                return Result<FaceitListResponse<MatchDto>>.Fail(result.ErrorMessage, result.ErrorCode);

            var faceitList = new FaceitListResponse<MatchDto>();
            foreach(var stats in result.Data.Items)
            {
                
                var match = history.Data.Items.FirstOrDefault(m => m.MatchId == stats.Stats.MatchId);
                if(match == null)
                {
                    Console.WriteLine($"Match with ID {stats.Stats.MatchId} not found in history for player {playerId} and game {gameId}");
                    continue;
                }
                if (match.Status != "finished" || match.FinishedAt == default)
                {
                    continue;
                }
                    

                stats.Stats.CreatedAt = match != null
                    ? DateTimeOffset.FromUnixTimeSeconds(match.StartedAt).UtcDateTime
                    : DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);

                faceitList.Items.Add(stats.Stats);
            }

            return Result<FaceitListResponse<MatchDto>>.Ok(faceitList);
        }

        async Task<Result<PlayerDto>> IFaceitApiClient.GetPlayerAsync(string playerId)
        {
            var path = $"players/{playerId}";
            var result = await Get<PlayerDto>(path);

            if (result.Data == null)
                return Result<PlayerDto>.Fail(new ErrorMessage { Type = ErrorType.USER_NOT_FOUND, Details = $"The faceit account could not be found" }, ErrorCode.NotFound);

            return result;            
        }

        async Task<Result<FaceitListResponse<PlayerDto>>> IFaceitApiClient.SearchPlayersAsync(string nickname, int offset, int limit)
        {
            var path = "search/players";
            var qParams = new Dictionary<string, string>
            {
                { "nickname", nickname },
                { "offset", offset.ToString() },
                {"limit", limit.ToString() }
            };

            var result = await Get<FaceitListResponse<PlayerDto>>(path, qParams);

            return result;             
        }

        public async Task<Result<FaceitListResponse<MatchHistoryDto>>> GetMatchesHistoryAsync(string gameId, string playerId, PlayerStatisticsRequestDto reqDto)
        {
            var path = $"players/{playerId}/history";
            var qParams = new Dictionary<string, string>
            {
                { "game", gameId },
                { "limit", _configuration.GetValue<int>("Faceit:MatchHistoryEndpointLimit", 100).ToString() },
                { "offset", reqDto.Offset.ToString() }
            };
            if (reqDto.From.HasValue)
                qParams.Add("from", (reqDto.From.Value / 1000).ToString());

            if (reqDto.To.HasValue)
                qParams.Add("to", ((reqDto.To.Value) / 1000).ToString());
        
            var result = await Get<FaceitListResponse<MatchHistoryDto>> (path, qParams);
            return result;
        }
    }
}
