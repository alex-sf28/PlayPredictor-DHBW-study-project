using System.Text.RegularExpressions;

namespace PlayPredictorWebAPI.Services
{
    public class ValidationService
    {
        public static bool IsValidEmail(string email)
        {
            string emailRegex = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$";

            return Regex.IsMatch(email, emailRegex);
        }

        public static bool IsValidPassword(string password) {
            return password != null && password.Length >= 6;
        }

        public static bool IsValidMatch(Models.Match match)
        {
            return match.GameMode == Models.GameMode.FiveVsFive;
        }
    }
}
