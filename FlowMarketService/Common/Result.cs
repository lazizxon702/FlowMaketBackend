namespace FlowMarketService.Common;

public readonly struct Result<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static Result<T> Ok(T value, int statusCode = 200) =>
        new() { Success = true, Value = value, StatusCode = statusCode };

    public static Result<T> Fail(string error, int statusCode = 400) =>
        new() { Success = false, Error = error, StatusCode = statusCode };
}
