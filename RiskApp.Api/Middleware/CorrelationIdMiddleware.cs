using System.Diagnostics;

namespace RiskApp.Api.Middleware;

public class CorrelationIdMiddleware : IMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Use incoming header or existing Activity Id
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(incoming))
        {
            context.TraceIdentifier = incoming;
        }

        // Ensure response header present
        context.Response.OnStarting(() =>
        {
            var id = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            context.Response.Headers[HeaderName] = id;
            return Task.CompletedTask;
        });

        await next(context);
    }
}
