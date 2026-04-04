namespace g_map_compare_backend.Common.Results
{
    public enum ErrorCode
    {
        None = 0,
        ValidationError = 1,
        NotFound = 2,
        Conflict = 3,
        Unauthorized = 4,
        Forbidden = 5,
        UnknownError = 99
    }
}
