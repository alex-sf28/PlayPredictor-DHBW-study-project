using g_map_compare_backend.Common.Results;
using g_map_compare_backend.Dtos.Auth;
using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Dtos.Auth
{
    public record LoginServiceResponseDto(Result<LoginResponseDto> LoginResponse, RefreshToken? RefreshToken);
    
}
