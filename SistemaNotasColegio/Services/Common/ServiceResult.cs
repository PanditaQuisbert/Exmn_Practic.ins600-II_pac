namespace SistemaNotasColegio.Services.Common;

public enum ServiceErrorType
{
    BadRequest,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

public sealed class ServiceResult<T>
{
    public bool Success { get; private init; }

    public T? Data { get; private init; }

    public string? Error { get; private init; }

    public ServiceErrorType ErrorType { get; private init; }

    public static ServiceResult<T> Ok(T data)
    {
        return new ServiceResult<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ServiceResult<T> Fail(
        string error,
        ServiceErrorType errorType = ServiceErrorType.BadRequest)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Error = error,
            ErrorType = errorType
        };
    }
}
