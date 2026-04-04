using PlayPredictorWebAPI.Common.Results;

namespace g_map_compare_backend.Common.Results
{
    public class Result<T> : Result
    {
        public T? Data { get; }

        private Result(bool success, T? data, ErrorMessage? errorMessage, ErrorCode errorCode)
            : base(success, errorMessage, errorCode)
        {
            Data = data;
        }

        public static Result<T> Ok(T data) =>
            new Result<T>(true, data, null, ErrorCode.None);

        public static Result<T> Fail(ErrorMessage errorMessage, ErrorCode errorCode = ErrorCode.UnknownError) =>
            new Result<T>(false, default, errorMessage, errorCode);
    }
}
