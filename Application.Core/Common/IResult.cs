namespace Application.Core.Common
{

    public interface IResult
    {
        int StatusCode { get; set; }
        bool Succeeded { get; set; }
        string? Message { get; set; }
        string? VersionNumber { get; set; }

    }

    public interface IResult<out T> : IResult
    {
        T? Data { get; }
        string? Error { get; set; }
    }
}