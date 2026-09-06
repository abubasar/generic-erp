using Application.Core.Common;
using System.Net;

namespace Application.Core.Exceptions
{
    public class CustomValidationException : CustomException
    {
        public CustomValidationException(List<ErrorModel> errors)
            : base("One or more validation failures have occurred", errors, HttpStatusCode.BadRequest)
        {
        }
    }
}