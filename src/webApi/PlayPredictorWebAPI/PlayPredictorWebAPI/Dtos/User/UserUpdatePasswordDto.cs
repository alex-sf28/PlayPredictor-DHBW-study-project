namespace PlayPredictorWebAPI.Dtos.User
{
    public class UserUpdatePasswordDto
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
