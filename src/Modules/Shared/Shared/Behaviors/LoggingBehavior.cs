using System.Diagnostics;
using System.Text.Json;

namespace Shared.Behaviors
{
    /// <summary>
    /// MediatR Pipeline Behavior for centralized logging of all requests and responses
    /// Logs request details, execution time, and response information
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid();
            var stopwatch = Stopwatch.StartNew();

            // Log request start with structured data
            logger.LogInformation("?? Starting request {RequestName} [{RequestId}] at {Timestamp}",
                requestName, requestId, DateTime.UtcNow);

            // Log detailed request information (only in Development/Staging)
            LogRequestDetails(request, requestId);

            TResponse response;
            Exception? exception = null;

            try
            {
                // Execute the request
                response = await next();
                
                stopwatch.Stop();

                // Log successful completion
                logger.LogInformation("? Completed request {RequestName} [{RequestId}] in {ElapsedMs}ms",
                    requestName, requestId, stopwatch.ElapsedMilliseconds);

                // Log response details (only in Development/Staging)
                LogResponseDetails(response, requestId);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                exception = ex;

                // Log failure with exception details
                logger.LogError(ex, "? Failed request {RequestName} [{RequestId}] in {ElapsedMs}ms - {ErrorMessage}",
                    requestName, requestId, stopwatch.ElapsedMilliseconds, ex.Message);

                throw;
            }
            finally
            {
                // Log performance metrics
                LogPerformanceMetrics(requestName, requestId, stopwatch.ElapsedMilliseconds, exception);
            }
        }

        private void LogRequestDetails(TRequest request, Guid requestId)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            try
            {
                var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                logger.LogDebug("?? Request details [{RequestId}]: {RequestData}",
                    requestId, requestJson);
            }
            catch (Exception ex)
            {
                logger.LogWarning("?? Could not serialize request [{RequestId}]: {Error}",
                    requestId, ex.Message);
            }
        }

        private void LogResponseDetails(TResponse response, Guid requestId)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            try
            {
                // Don't log large response objects or sensitive data
                var responseType = typeof(TResponse);
                
                if (IsLoggableResponse(responseType))
                {
                    var responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    });

                    logger.LogDebug("?? Response details [{RequestId}]: {ResponseData}",
                        requestId, responseJson);
                }
                else
                {
                    logger.LogDebug("?? Response [{RequestId}]: {ResponseType}",
                        requestId, responseType.Name);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("?? Could not serialize response [{RequestId}]: {Error}",
                    requestId, ex.Message);
            }
        }

        private void LogPerformanceMetrics(string requestName, Guid requestId, long elapsedMs, Exception? exception)
        {
            // Log performance warnings for slow requests
            if (elapsedMs > 5000) // > 5 seconds
            {
                logger.LogWarning("?? Slow request detected: {RequestName} [{RequestId}] took {ElapsedMs}ms",
                    requestName, requestId, elapsedMs);
            }
            else if (elapsedMs > 1000) // > 1 second
            {
                logger.LogInformation("?? Request {RequestName} [{RequestId}] took {ElapsedMs}ms",
                    requestName, requestId, elapsedMs);
            }

            // Log to structured logging/metrics system (e.g., Application Insights, Prometheus)
            using var activity = Activity.Current;
            activity?.SetTag("request.name", requestName);
            activity?.SetTag("request.id", requestId.ToString());
            activity?.SetTag("request.duration_ms", elapsedMs);
            activity?.SetTag("request.success", exception == null);
            
            if (exception != null)
            {
                activity?.SetTag("request.error_type", exception.GetType().Name);
            }
        }

        private static bool IsLoggableResponse(Type responseType)
        {
            // Don't log large collections or file responses
            var nonLoggableTypes = new[]
            {
                "Stream", "FileResult", "IActionResult", "byte[]"
            };

            var typeName = responseType.Name;

            // Don't log if it's a non-loggable type
            if (nonLoggableTypes.Any(t => typeName.Contains(t)))
                return false;

            // Don't log large collections (over 100 items)
            if (responseType.IsGenericType)
            {
                var genericTypeDefinition = responseType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(IEnumerable<>))
                {
                    // Could check collection size here, but for simplicity, we'll log collections
                    return true;
                }
            }

            return true;
        }
    }
}