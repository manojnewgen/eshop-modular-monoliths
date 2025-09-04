using Shared.Exceptions;

namespace Catalog.Products.Features.TestLogging
{
    /// <summary>
    /// Test command to demonstrate centralized logging in action
    /// </summary>
    public record TestLoggingCommand(string Message, int DelayMs = 0) : ICommand<TestLoggingResult>;

    /// <summary>
    /// Response for test logging command
    /// </summary>
    public record TestLoggingResult(string Message, DateTime ProcessedAt, TimeSpan Duration);

    /// <summary>
    /// Handler for test logging command - demonstrates how all handlers get automatic logging
    /// </summary>
    public class TestLoggingHandler : ICommandHandler<TestLoggingCommand, TestLoggingResult>
    {
        private readonly ILogger<TestLoggingHandler> _logger;

        public TestLoggingHandler(ILogger<TestLoggingHandler> logger)
        {
            _logger = logger;
        }

        public async Task<TestLoggingResult> Handle(TestLoggingCommand command, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            
            // Log within the handler (this will be included in the correlation context)
            _logger.LogInformation("Processing test logging command with message: {Message}", command.Message);

            // Simulate some work
            if (command.DelayMs > 0)
            {
                _logger.LogDebug("Simulating delay of {DelayMs}ms", command.DelayMs);
                await Task.Delay(command.DelayMs, cancellationToken);
            }

            // Simulate different scenarios
            if (command.Message.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Test command contains 'error' - this will trigger error handling");
                throw new BadRequestException("Test error triggered by message content");
            }

            if (command.Message.Contains("slow", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Simulating slow operation");
                await Task.Delay(2000, cancellationToken); // 2 seconds to trigger slow request warning
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogInformation("Test logging command completed successfully");

            return new TestLoggingResult(
                $"Processed: {command.Message}",
                endTime,
                duration);
        }
    }
}