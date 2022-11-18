using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilio.Common.DataContracts.Base;
using Utilio.Common.DataContracts.Enumerations;
using Utilio.Common.Enumerations;
using Utilio.Common.Exceptions;
using Utilio.Common.Logger.Interfaces;
using Utilio.Common.Resources;
using Utilio.Common.Serialization;

namespace Utilio.Provider.OpcinaNovoSarajevo.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, ILoggerAdapter logger)
        {
            try
            {
                logger.LogDebug(await FormatRequest(context.Request));
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, logger);
            }
        }

        #region << Private Methods >>

        /// <summary>
        /// Returns exception message.
        /// </summary>
        /// <returns>Exception message</returns>
        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILoggerAdapter logger)
        {
            var exceptionType = ex.GetType();
            var severity = (ex as UtilioBaseException)?.Severity ?? Severity.Fatal;
            logger.LogException(ex, context.Request, severity);
            context.Response.ContentType = "application/json";

            switch (exceptionType)
            {
                case Type _ when exceptionType == typeof(UtilioValidationException):
                case Type _ when exceptionType == typeof(UtilioInvalidRequestException):
                case Type _ when exceptionType == typeof(UtilioArgumentNullException):
                case Type _ when exceptionType == typeof(UtilioArgumentException):
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(CreateResponseOnException(ex.Message, severity));
                    break;
                case Type _ when exceptionType == typeof(UtilioNotFoundException):
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync(CreateResponseOnException(ex.Message, severity));
                    break;
                case Type _ when exceptionType == typeof(UtilioNotAuthorizedException):
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync(CreateResponseOnException(ex.Message, severity));
                    break;
                case Type _ when exceptionType == typeof(UtilioProcessingException):
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                    await context.Response.WriteAsync(CreateResponseOnException(ex.Message, severity));
                    break;
                case Type _ when exceptionType == typeof(UtilioBaseException):
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(CreateResponseOnException(ex.Message, severity));
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(CreateResponseOnException("An error has occured. Please try again later or contact support.", severity));
                    break;
            }
        }

        /// <summary>
        ///  Creates appropriate response for exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="severity"></param>
        /// <returns>string</returns>

        private static string CreateResponseOnException(string message, Severity severity)
        {
            return JsonHelper.Serialize(new BaseResponse<string>
            {
                Success = false,
                Error = new Error
                {
                    Message = message,
                }
            });
        }

        /// <summary>
        /// Formats request as string
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>Request string</returns>
        private static async Task<string> FormatRequest(HttpRequest request)
        {
            var bodyStr = "";
            var req = request;

            // Allows using stream several times in ASP.Net Core
            req.EnableBuffering();

            using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = await reader.ReadToEndAsync();
            }

            req.Body.Position = 0;

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyStr}";
        }

        #endregion
    }
}
