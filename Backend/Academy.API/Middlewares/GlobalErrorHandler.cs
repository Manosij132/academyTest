using Academy.Core.Abstraction.Infrastructure;
using Academy.Shared.DTO;
using Academy.Shared.Exceptions;
using Academy.Shared.Response;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Net;

namespace Academy.API.Middlewares
{
    public class GlobalErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandler> _logger;
        private readonly ICollaboratorClient _collaboratorClient;
        private readonly IWebHostEnvironment _env;

        public GlobalErrorHandler(RequestDelegate next, ILogger<GlobalErrorHandler> logger,
            ICollaboratorClient collaboratorClient, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _collaboratorClient = collaboratorClient;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "JWT token expired.");
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, ex, DomainErrors.Authorization.AuthTokenExpired);
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed.");
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, ex, DomainErrors.Authorization.InvalidAuthToken);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument Exception");
                await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, ex, DomainErrors.Authorization.InvalidAuthToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                var errorMessage = string.IsNullOrEmpty(ex.Message) ? string.Empty : ex.Message;
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, ex, DomainErrors.Common.UnhandledException(errorMessage));
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, Exception ex, Error error)
        {
            string errorResponseId = Ulid.NewUlid().ToString();

            AcademyResponse<string> academyResponse = new()
            {
                Data = null,
                Status = statusCode,
                Error = error,
                ErrorResponseId = errorResponseId,
                StackTrace = ex.InnerException != null ? ex.InnerException.StackTrace : ex.StackTrace
            };

            _logger.LogError(errorResponseId, ex);

            if (_env.IsProduction())
            {
                await _collaboratorClient.SendMessageAsync(JsonConvert.SerializeObject(academyResponse));

                academyResponse.Error = error;
                academyResponse.StackTrace = string.Empty;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(academyResponse);
        }
    }
}
