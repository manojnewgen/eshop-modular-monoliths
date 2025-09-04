using System.Diagnostics;

namespace Shared.Behaviors
{
    /// <summary>
    /// MediatR Pipeline Behavior for managing request context
    /// Adds correlation IDs, user context, and distributed tracing
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class RequestContextBehavior<TRequest, TResponse>(
        ILogger<RequestContextBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor httpContextAccessor) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var correlationId = GetOrCreateCorrelationId();
            var userId = GetCurrentUserId();
            var userAgent = GetUserAgent();
            var ipAddress = GetClientIpAddress();

            // Set up logging scope with context information
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestName"] = requestName,
                ["UserId"] = userId ?? "Anonymous",
                ["UserAgent"] = userAgent ?? "Unknown",
                ["IpAddress"] = ipAddress ?? "Unknown"
            });

            // Add context to current activity for distributed tracing
            var activity = Activity.Current;
            if (activity != null)
            {
                activity.SetTag("correlation.id", correlationId);
                activity.SetTag("request.name", requestName);
                activity.SetTag("user.id", userId ?? "anonymous");
                activity.SetTag("user.agent", userAgent ?? "unknown");
                activity.SetTag("client.ip", ipAddress ?? "unknown");
            }

            // Log request context
            logger.LogDebug("?? Request context established for {RequestName} - CorrelationId: {CorrelationId}, User: {UserId}",
                requestName, correlationId, userId ?? "Anonymous");

            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "?? Request failed in context - CorrelationId: {CorrelationId}, User: {UserId}",
                    correlationId, userId ?? "Anonymous");
                throw;
            }
        }

        private string GetOrCreateCorrelationId()
        {
            var httpContext = httpContextAccessor.HttpContext;
            
            if (httpContext == null)
                return Guid.NewGuid().ToString("N")[..12]; // Short ID for non-HTTP contexts

            // Try to get from headers first
            if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue) ||
                httpContext.Request.Headers.TryGetValue("X-Request-ID", out headerValue))
            {
                var correlationId = headerValue.FirstOrDefault();
                if (!string.IsNullOrEmpty(correlationId))
                {
                    return correlationId;
                }
            }

            // Create new correlation ID
            var newCorrelationId = Guid.NewGuid().ToString("N")[..12];
            
            // Add to response headers so client can track
            httpContext.Response.Headers["X-Correlation-ID"] = newCorrelationId;
            
            return newCorrelationId;
        }

        private string? GetCurrentUserId()
        {
            var httpContext = httpContextAccessor.HttpContext;
            
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Try different claim types for user ID
                return httpContext.User.FindFirst("sub")?.Value ??
                       httpContext.User.FindFirst("userId")?.Value ??
                       httpContext.User.FindFirst("id")?.Value ??
                       httpContext.User.Identity.Name;
            }

            return null;
        }

        private string? GetUserAgent()
        {
            var httpContext = httpContextAccessor.HttpContext;
            
            if (httpContext?.Request?.Headers != null &&
                httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            {
                return userAgent.FirstOrDefault();
            }

            return null;
        }

        private string? GetClientIpAddress()
        {
            var httpContext = httpContextAccessor.HttpContext;
            
            if (httpContext == null)
                return null;

            // Check for forwarded headers (load balancer, proxy, etc.)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}