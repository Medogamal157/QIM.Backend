namespace QIM.Shared.Models;

/// <summary>
/// Uniform API response wrapper (non-generic).
/// </summary>
public class Result
{
    public bool IsSuccess { get; set; }
    public bool IsForbidden { get; set; }
    public bool IsNotFound { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];

    public static Result Success(string? message = null) =>
        new() { IsSuccess = true, Message = message };

    public static Result Failure(string error) =>
        new() { IsSuccess = false, Errors = [error] };

    public static Result Failure(List<string> errors) =>
        new() { IsSuccess = false, Errors = errors };

    public static Result Forbidden(string error) =>
        new() { IsSuccess = false, IsForbidden = true, Errors = [error] };

    public static Result NotFound(string error) =>
        new() { IsSuccess = false, IsNotFound = true, Errors = [error] };
}

/// <summary>
/// Uniform API response wrapper (generic — carries data payload).
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Success(T data, string? message = null) =>
        new() { IsSuccess = true, Data = data, Message = message };

    public new static Result<T> Failure(string error) =>
        new() { IsSuccess = false, Errors = [error] };

    public new static Result<T> Failure(List<string> errors) =>
        new() { IsSuccess = false, Errors = errors };

    public new static Result<T> Forbidden(string error) =>
        new() { IsSuccess = false, IsForbidden = true, Errors = [error] };

    public new static Result<T> NotFound(string error) =>
        new() { IsSuccess = false, IsNotFound = true, Errors = [error] };
}
