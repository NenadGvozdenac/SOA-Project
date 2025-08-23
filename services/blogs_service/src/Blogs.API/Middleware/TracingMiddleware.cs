using System.Diagnostics;

namespace blogs_service.src.Blogs.API.Middleware;

public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TracingMiddleware> _logger;

    public TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create trace ID
        var traceId = context.TraceIdentifier;
        
        // Add trace ID to response headers
        context.Response.Headers.TryAdd("X-Trace-ID", traceId);
        
        // Extract parent trace ID from incoming headers
        var parentTraceId = context.Request.Headers["X-Trace-ID"].FirstOrDefault();
        
        // Start activity for distributed tracing
        using var activity = Activity.Current?.Source.StartActivity("HTTP Request");
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.url", context.Request.Path);
        activity?.SetTag("service.name", "blogs-service");
        
        if (!string.IsNullOrEmpty(parentTraceId))
        {
            activity?.SetTag("parent.trace_id", parentTraceId);
        }

        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("[TRACE] {TraceId} - Request started: {Method} {Path}", 
            traceId, context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            activity?.SetTag("http.status_code", context.Response.StatusCode);
            activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation("[TRACE] {TraceId} - Request completed: {StatusCode} in {Duration}ms", 
                traceId, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
