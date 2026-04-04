using System.ComponentModel;

namespace PlayPredictorWebAPI.Common.Results
{
    public enum ErrorType
    {
        [Description("Uknown Error")]
        UNKNOWN,

        [Description("Login failed")]
        LOGIN_FAIL,

        [Description("Registration failed")]
        REGISTER_FAIL,

        [Description("User not found")]
        USER_NOT_FOUND,

        [Description("Permission denied")]
        ACCESS_NOT_ALLOWED,

        [Description("Invalid password")]
        PASSWORD_INCORRECT,

        [Description("Googl Calendar access expired")]
        GOOGLE_CALENDAR_ACCESS_EXPIRED,
        [Description("Faceit API error")]
        FACEIT_ERROR,
        [Description("Validation error")]
        VALIDATION_ERROR,
        [Description("Internal server error")]
        INTERNAL,
        [Description("Invalid File type")]
        INVALID_TYPE
    }
}
