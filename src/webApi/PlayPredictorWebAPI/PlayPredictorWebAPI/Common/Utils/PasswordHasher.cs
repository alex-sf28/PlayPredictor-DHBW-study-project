using PlayPredictorWebAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace PlayPredictorWebAPI.Common.Utils
{
    public class PasswordHasher
    {

        /// <summary>
        /// Checks if the provided password matches the password of the current User object.
        /// Call this when processing login attempts.
        /// </summary>
        public static bool IsCorrectPassword(string password, User user)
        {
            //get raw bytes of password string
            byte[] raw = Encoding.UTF8.GetBytes(password);

            //hash password using SHA256
            byte[] result = SHA256.HashData(raw);

            //construct string of hashed password for easier matching
            string providedPasswordHash = Convert.ToHexString(result).ToLower();

            //Note: Strings are primitive data types in C#, therefore the '==' operator will compare the
            //actual values in them against each other, instead of the references of the two string objects
            return providedPasswordHash == user.PasswordHash;
        }

        public static string HashPasswordForRegistration(string password)
        {
            byte[] rawPassword = Encoding.UTF8.GetBytes(password);

            byte[] hashBytes = SHA256.HashData(rawPassword);

            string passwordHash = Convert.ToHexString(hashBytes).ToLower();

            return passwordHash;
        }
    }
}
