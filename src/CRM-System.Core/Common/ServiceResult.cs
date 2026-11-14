namespace CRM_System.Core.Common;

public class ServiceResult
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public ServiceErrorType ErrorType { get; init; } = ServiceErrorType.None;

    public static ServiceResult Success(string? message = null) => new()
    {
        Succeeded = true,
        Message = message,
        ErrorType = ServiceErrorType.None
    };

    public static ServiceResult Failure(string error, ServiceErrorType errorType = ServiceErrorType.Validation) => new()
    {
        Succeeded = false,
        Message = error,
        Errors = [error],
        ErrorType = errorType
    };

    public static ServiceResult Failure(IEnumerable<string> errors, ServiceErrorType errorType = ServiceErrorType.Validation)
    {
        var list = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        return new ServiceResult
        {
            Succeeded = false,
            Message = list.FirstOrDefault() ?? "Operation failed.",
            Errors = list,
            ErrorType = errorType
        };
    }
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Success(T data, string? message = null) => new()
    {
        Succeeded = true,
        Data = data,
        Message = message,
        ErrorType = ServiceErrorType.None
    };

    public new static ServiceResult<T> Failure(string error, ServiceErrorType errorType = ServiceErrorType.Validation) => new()
    {
        Succeeded = false,
        Message = error,
        Errors = [error],
        ErrorType = errorType
    };

    public new static ServiceResult<T> Failure(IEnumerable<string> errors, ServiceErrorType errorType = ServiceErrorType.Validation)
    {
        var list = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToList();
        return new ServiceResult<T>
        {
            Succeeded = false,
            Message = list.FirstOrDefault() ?? "Operation failed.",
            Errors = list,
            ErrorType = errorType
        };
    }
}

public enum ServiceErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Unauthorized = 3,
    Forbidden = 4,
    Conflict = 5,
    Unexpected = 6
}
