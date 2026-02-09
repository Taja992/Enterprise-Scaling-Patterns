namespace ArticleService.Application.Common;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public Error? Error { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static Result<T> Failure(Error error) => new() { IsSuccess = false, Error = error };
}

public record Error(string Code, string Message);
