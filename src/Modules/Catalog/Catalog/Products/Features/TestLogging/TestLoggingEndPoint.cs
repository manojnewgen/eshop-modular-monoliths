namespace Catalog.Products.Features.TestLogging
{
    /// <summary>
    /// Test endpoint to demonstrate centralized logging capabilities
    /// Shows how logging, performance tracking, and error handling work together
    /// </summary>
    public class TestLoggingEndPoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/test/logging", async (TestLoggingRequest request, ISender sender) =>
            {
                var command = new TestLoggingCommand(request.Message, request.DelayMs);
                var result = await sender.Send(command);
                
                return Results.Ok(new TestLoggingResponse(
                    result.Message,
                    result.ProcessedAt,
                    result.Duration.TotalMilliseconds));
            })
            .WithName("TestLogging")
            .Produces<TestLoggingResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Test centralized logging")
            .WithDescription("Endpoint to test centralized logging, performance tracking, and error handling")
            .WithTags("Testing", "Logging");

            // Additional test endpoints for different scenarios
            app.MapGet("/test/logging/slow", async (ISender sender) =>
            {
                var command = new TestLoggingCommand("This is a slow operation", 0);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("TestSlowLogging")
            .WithSummary("Test slow request logging");

            app.MapGet("/test/logging/error", async (ISender sender) =>
            {
                var command = new TestLoggingCommand("This will trigger an error", 0);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("TestErrorLogging")
            .WithSummary("Test error logging");
        }
    }

    /// <summary>
    /// Request model for test logging
    /// </summary>
    public record TestLoggingRequest(string Message, int DelayMs = 0);

    /// <summary>
    /// Response model for test logging
    /// </summary>
    public record TestLoggingResponse(string Message, DateTime ProcessedAt, double DurationMs);
}