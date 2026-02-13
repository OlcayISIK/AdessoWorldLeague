namespace AdessoWorldLeague.Core.Results;

public class OperationResult
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }

    protected OperationResult(bool isSuccess, string? message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static OperationResult Success(string? message = null) => new(true, message);
    public static OperationResult Failure(string message) => new(false, message);
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; private set; }

    private OperationResult(bool isSuccess, T? data, string? message)
        : base(isSuccess, message)
    {
        Data = data;
    }

    public static OperationResult<T> Success(T data, string? message = null) => new(true, data, message);
    public new static OperationResult<T> Failure(string message) => new(false, default, message);
}
