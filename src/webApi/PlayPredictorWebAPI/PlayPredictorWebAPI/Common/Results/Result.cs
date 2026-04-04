using PlayPredictorWebAPI.Common.Results;

namespace g_map_compare_backend.Common.Results
{
    public class Result
    {
        public bool Success { get; }
        public ErrorMessage? ErrorMessage { get; }
        public ErrorCode ErrorCode { get; }

        protected Result(bool success, ErrorMessage? errorMessage, ErrorCode errorCode)
        {
            Success = success;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public static Result Ok() =>
            new Result(true, null, ErrorCode.None);

        public static Result Fail(ErrorMessage errorMessage, ErrorCode errorCode = ErrorCode.UnknownError) =>
            new Result(false, errorMessage, errorCode);
    }
}
