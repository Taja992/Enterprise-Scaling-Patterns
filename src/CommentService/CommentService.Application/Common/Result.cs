namespace CommentService.Application.Common;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public AppError? Error { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static Result<T> Failure(AppError error) => new() { IsSuccess = false, Error = error };
}

public record AppError(string Code, string Message)
{
    public static AppError NotFound(string message) => new("NOT_FOUND", message);

    public static AppError Validation(string message) => new("VALIDATION_ERROR", message);

    public static AppError InternalError(string message) => new("INTERNAL_ERROR", message);
}
