using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace NavigationPlatform.Shared.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headerValue = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        var correlationId = string.IsNullOrWhiteSpace(headerValue)
            ? Guid.NewGuid().ToString()
            : headerValue;

        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}