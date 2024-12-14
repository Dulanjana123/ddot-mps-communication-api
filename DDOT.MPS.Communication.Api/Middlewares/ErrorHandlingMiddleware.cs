using DDOT.MPS.Communication.Model.Responses;
using System.Net;
using System.Text.Json;
using static DDOT.MPS.Communication.Core.Exceptions.UserDefinedException;

namespace DDOT.MPS.Communication.Api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                HttpResponse response = context.Response;
                response.ContentType = "application/json";

                switch (exception)
                {
                    case UDInvalidOperationException:
                        response.StatusCode = (int)HttpStatusCode.Accepted;
                        break;
                    case UDNotFoundException:
                        response.StatusCode = (int)HttpStatusCode.Accepted;
                        break;
                    case UDUnauthorizedAccessException:
                        response.StatusCode = (int)HttpStatusCode.Accepted;
                        break;
                    case UDArgumentException:
                        response.StatusCode = (int)HttpStatusCode.Accepted;
                        break;
                    case UDValiationException:
                        response.StatusCode = (int)HttpStatusCode.Accepted;
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                _logger.LogError("DDOT.MPS.Permit.Api.Middlewares.ErrorHandlingMiddleware | Exception: {exception}", exception.ToString());
                Console.WriteLine($"Error: {exception.Message} \n {exception?.InnerException?.StackTrace}");
                const string defaultErrorMessage = "SERVERSIDE_ERROR_OCCURED";
                BaseResponse<string> exceptionResponse = new BaseResponse<string> { Message = response.StatusCode == (int)HttpStatusCode.InternalServerError ? defaultErrorMessage : exception?.Message };
                JsonSerializerOptions serializeOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
                string result = JsonSerializer.Serialize(exceptionResponse, serializeOptions);
                await response.WriteAsync(result);
            }
        }
    }
}
