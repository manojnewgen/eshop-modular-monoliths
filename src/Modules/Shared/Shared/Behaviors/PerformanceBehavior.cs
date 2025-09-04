using System.Diagnostics;

namespace Shared.Behaviors
{
    /// <summary>
    /// MediatR Pipeline Behavior for performance monitoring and metrics collection
    /// Tracks execution time, memory usage, and identifies performance bottlenecks
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();
            
            // Memory tracking
            var initialMemory = GC.GetTotalMemory(false);
            
            try
            {
                var response = await next();
                
                stopwatch.Stop();
                var finalMemory = GC.GetTotalMemory(false);
                var memoryUsed = finalMemory - initialMemory;
                
                LogPerformanceMetrics(requestName, stopwatch.ElapsedMilliseconds, memoryUsed);
                
                return response;
            }
            catch (Exception)
            {
                stopwatch.Stop();
                var finalMemory = GC.GetTotalMemory(false);
                var memoryUsed = finalMemory - initialMemory;
                
                LogPerformanceMetrics(requestName, stopwatch.ElapsedMilliseconds, memoryUsed, true);
                throw;
            }
        }

        private void LogPerformanceMetrics(string requestName, long elapsedMs, long memoryUsed, bool hadError = false)
        {
            // Define performance thresholds
            const long SlowRequestThreshold = 1000; // 1 second
            const long VerySlowRequestThreshold = 5000; // 5 seconds
            const long HighMemoryUsage = 1024 * 1024; // 1 MB

            // Log based on performance characteristics
            if (elapsedMs > VerySlowRequestThreshold)
            {
                _logger.LogWarning("?? Very slow request: {RequestName} took {ElapsedMs}ms and used {MemoryUsed:N0} bytes",
                    requestName, elapsedMs, memoryUsed);
            }
            else if (elapsedMs > SlowRequestThreshold)
            {
                _logger.LogWarning("?? Slow request: {RequestName} took {ElapsedMs}ms and used {MemoryUsed:N0} bytes",
                    requestName, elapsedMs, memoryUsed);
            }
            else if (memoryUsed > HighMemoryUsage)
            {
                _logger.LogInformation("?? High memory usage: {RequestName} used {MemoryUsed:N0} bytes in {ElapsedMs}ms",
                    requestName, memoryUsed, elapsedMs);
            }
            else if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("?? Performance: {RequestName} completed in {ElapsedMs}ms using {MemoryUsed:N0} bytes",
                    requestName, elapsedMs, memoryUsed);
            }

            // Create metrics for monitoring systems (Application Insights, Prometheus, etc.)
            var tags = new Dictionary<string, object?>
            {
                ["request_name"] = requestName,
                ["duration_ms"] = elapsedMs,
                ["memory_used_bytes"] = memoryUsed,
                ["had_error"] = hadError
            };

            // If using Application Insights or similar, you can send custom metrics here
            // Example: _telemetryClient.GetMetric("request.performance").TrackValue(elapsedMs, tags);
        }
    }
}