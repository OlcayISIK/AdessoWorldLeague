namespace AdessoWorldLeague.Core.Results;

public class OperationResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected OperationResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static OperationResult Success() => new(true, null);
    public static OperationResult Failure(string message) => new(false, message);
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; private set; }

    private OperationResult(bool isSuccess, T? data, string? errorMessage)
        : base(isSuccess, errorMessage)
    {
        Data = data;
    }

    public static OperationResult<T> Success(T data) => new(true, data, null);
    public new static OperationResult<T> Failure(string message) => new(false, default, message);
}
