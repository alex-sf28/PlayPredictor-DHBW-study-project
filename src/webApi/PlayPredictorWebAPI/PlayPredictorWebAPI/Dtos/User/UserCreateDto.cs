using PlayPredictorWebAPI.Models;

namespace PlayPredictorWebAPI.Dtos.User
{
    public class UserCreateDto
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";

        public string? Password { get; set; }

        public string? GoogleID { get; set; }

        public AuthProvider AuthProvider { get; set; }


    }
}
