using System.Diagnostics;

namespace AdessoWorldLeague.WebApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "token", "secret"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "Request başladı. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}",
            correlationId, context.Request.Method, context.Request.Path);

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "Request tamamlandı. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, Duration: {Duration}ms",
            correlationId, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
