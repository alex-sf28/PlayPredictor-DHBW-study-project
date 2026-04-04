using g_map_compare_backend.Common.Results;
using PlayPredictorWebAPI.Dtos.Faceit;

namespace PlayPredictorWebAPI.Services.External
{
    public interface IFaceitApiClient
    {
        //Task<Result<ICollection<GameDto>>> GetGamesAsync();

        Task<Result<FaceitListResponse<MatchDto>>> GetMatchesAsync(string gameId, string playerId, PlayerStatisticsRequestDto reqDto);

        Task<Result<FaceitListResponse<MatchHistoryDto>>> GetMatchesHistoryAsync(string gameId, string playerId, PlayerStatisticsRequestDto reqDto);

        Task<Result<FaceitListResponse<PlayerDto>>> SearchPlayersAsync(string nickname, int offset, int limit);

        Task<Result<PlayerDto>> GetPlayerAsync(string playerId);
    }

    public class FaceitListResponse<T>
    {
        public int Start { get; set; }
        public int End { get; set; }

        public ICollection<T> Items { get; set; } = new List<T>();
    }
}
