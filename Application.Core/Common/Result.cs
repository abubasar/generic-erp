namespace Application.Core.Common
{
    public class Result : IResult
    {
        public Result()
        {
        }
        public int StatusCode { get; set; }
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public string? VersionNumber { get; set; }

    }

    public class Result<T> : Result, IResult<T>
    {
        public Result()
        {
        }

        public T? Data { get; set; }
        public string? Error { get; set; }


        public static Result<T> Fail(string error, string message)
        {
            return new Result<T> { Succeeded = false, Error = error, Message = message };
        }

        public static Task<Result<T>> FailAsync(string error, string message)
        {
            return Task.FromResult(Fail(error, message));
        }

        public static Result<T> Success(T data, string message, string? versionNumber="N/A")
        {
            return new Result<T> { StatusCode = 200, Succeeded = true, Data = data, Message = message,VersionNumber=versionNumber };
        }

        public static Task<Result<T>> SuccessAsync(T data, string message,string? versionNumber="N/A")
        {
            return Task.FromResult(Success(data, message,versionNumber));
        }
        public static ErrorResult<T> ReturnError(string message)
        {
            return new() { Succeeded = false, Message = message, StatusCode = 500 };
        }
        public static Task<ErrorResult<T>> ReturnErrorAsync(string message)
        {
            return Task.FromResult(ReturnError(message));
        }
    }
    public class ErrorResult<T> : Result<T>
    {
        public List<T>? ValidationErrors { get; set; } = new List<T>();
        public string? Source { get; set; }

        public string? Exception { get; set; }
    }
}