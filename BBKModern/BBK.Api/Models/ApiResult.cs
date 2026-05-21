namespace BBK.Api.Models;

public sealed record ApiResult<T>(bool Success, string Message, T? Data)
{
    public static ApiResult<T> Ok(T data, string message = "") => new(true, message, data);

    public static ApiResult<T> Fail(string message) => new(false, message, default);
}
