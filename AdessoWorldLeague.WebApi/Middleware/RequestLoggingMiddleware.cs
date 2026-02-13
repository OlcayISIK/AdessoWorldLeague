using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AdessoWorldLeague.WebApi.Middleware;

public partial class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        var sw = Stopwatch.StartNew();

        var requestBody = await ReadRequestBodyAsync(context.Request);
        var sanitizedRequestBody = MaskSensitiveFields(requestBody);

        _logger.LogInformation(
            "Request started. CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, RequestBody: {RequestBody}",
            correlationId, context.Request.Method, context.Request.Path, sanitizedRequestBody);

        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context);

        sw.Stop();

        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        await responseBodyStream.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;

        var sanitizedResponseBody = MaskSensitiveFields(responseBody);

        _logger.LogInformation(
            "Request completed. CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, Duration: {Duration}ms, ResponseBody: {ResponseBody}",
            correlationId, context.Response.StatusCode, sw.ElapsedMilliseconds, sanitizedResponseBody);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
            return string.Empty;

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static string MaskSensitiveFields(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        return SensitiveFieldRegex().Replace(body, "$1\"***\"");
    }

    [GeneratedRegex("""("(?:password|token|secret)"\s*:\s*)("[^"]*")""", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveFieldRegex();
}
