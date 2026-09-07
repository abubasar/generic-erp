using Application.Core.Common;
using Application.Core.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace Application.Api.Middlewares
{

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                if (exception is not CustomException && exception.InnerException != null)
                {
                    while (exception.InnerException != null)
                    {
                        exception = exception.InnerException;
                    }
                }

                var responseModel = await ErrorResult<ErrorModel>.ReturnErrorAsync(exception.Message);
                responseModel.Source = exception.Source;
                responseModel.Exception = exception.Message;
                responseModel.Message = exception.Message;
                Serilog.Log.Error(exception.Message, exception);
                switch (exception)
                {
                    case CustomException e:
                        responseModel.StatusCode = (int)e.StatusCode;
                        responseModel.ValidationErrors = e.ErrorMessages;
                        break;

                    case KeyNotFoundException:
                        responseModel.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case UnauthorizedAccessException:
                        responseModel.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    case NotImplementedException:
                        responseModel.StatusCode = (int)HttpStatusCode.NotImplemented;
                        break;
                    case UnauthorizationException:
                        responseModel.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    case ModuleNotEnabledException:
                        responseModel.StatusCode = (int)HttpStatusCode.Forbidden;
                        break;
                    case NotFoundResultException:
                        responseModel.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case BadRequestException:
                        responseModel.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    default:
                        responseModel.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonConvert.SerializeObject(responseModel, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });

                await response.WriteAsync(result);
            }
        }
    }

}
