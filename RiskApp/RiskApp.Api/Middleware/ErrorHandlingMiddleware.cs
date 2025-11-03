using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RiskApp.Api.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                type = "https://httpstatuses.com/500",
                title = "An unexpected error occurred.",
                status = 500,
                traceId = traceId
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }

}
