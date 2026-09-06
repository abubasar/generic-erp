using Application.Core.Common;
using System.Net;

namespace Application.Core.Exceptions
{
    public class CustomException : Exception
    {
        public List<ErrorModel> ErrorMessages { get; } = new();

        public HttpStatusCode StatusCode { get; }

        public CustomException(string message, List<ErrorModel> errors, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            ErrorMessages = errors;
            StatusCode = statusCode;
        }
    }
}