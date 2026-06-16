namespace Nexus.User.Api.Models;

public sealed class ApiResponse<T>
{
    public T? Result { get; init; }
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ApiResponse<T> Success(T? result, int statusCode = 200, string message = "") => new()
    {
        Result = result,
        IsSuccess = true,
        StatusCode = statusCode,
        Message = message
    };

    public static ApiResponse<T> Failure(string message, int statusCode = 400) => new()
    {
        Result = default,
        IsSuccess = false,
        StatusCode = statusCode,
        Message = message
    };
}
