using g_map_compare_backend.Dtos.User;

namespace g_map_compare_backend.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; }
    }
}
