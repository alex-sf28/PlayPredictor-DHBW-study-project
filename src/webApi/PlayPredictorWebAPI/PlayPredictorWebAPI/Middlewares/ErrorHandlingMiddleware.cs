using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using PlayPredictorWebAPI.Common.Results;
using System.Net;

namespace g_map_compare_backend.Middlewares
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new ProblemDetails { Title = ErrorMessage.GetDescription(ErrorType.INTERNAL), Detail = "An internal server error occured: " + ex.Message });
            }

        }
    }
}
