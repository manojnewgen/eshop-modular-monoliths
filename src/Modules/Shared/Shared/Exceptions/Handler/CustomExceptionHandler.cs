using Microsoft.AspNetCore.Diagnostics;
using FluentValidation;
using System.Text.Json;
using Shared.Exceptions;

namespace Shared.Exceptions.Handler
{
    public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(
                exception,
                "Exception occurred: {Message} at {Time}",
                exception.Message,
                DateTime.UtcNow);

            var errorResponse = exception switch
            {
                ValidationException validationException => CreateValidationErrorResponse(validationException),
                BadRequestException badRequestException => CreateErrorResponse(
                    "Bad Request",
                    badRequestException.Detail ?? badRequestException.Message,
                    StatusCodes.Status400BadRequest),
                NotFoundException notFoundException => CreateErrorResponse(
                    "Not Found",
                    notFoundException.Message,
                    StatusCodes.Status404NotFound),
                InternalServerException internalServerException => CreateErrorResponse(
                    "Internal Server Error",
                    internalServerException.Detail ?? internalServerException.Message,
                    StatusCodes.Status500InternalServerError),
                _ => CreateErrorResponse(
                    "Internal Server Error",
                    "An unexpected error occurred",
                    StatusCodes.Status500InternalServerError)
            };

            httpContext.Response.StatusCode = errorResponse.StatusCode;
            httpContext.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

            return true;
        }

        private static ErrorResponse CreateErrorResponse(string title, string detail, int statusCode)
        {
            return new ErrorResponse
            {
                Title = title,
                Detail = detail,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        private static ValidationErrorResponse CreateValidationErrorResponse(ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            return new ValidationErrorResponse
            {
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred",
                StatusCode = StatusCodes.Status400BadRequest,
                Timestamp = DateTime.UtcNow,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Standard error response structure
    /// </summary>
    public class ErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Validation error response structure with detailed field errors
    /// </summary>
    public class ValidationErrorResponse : ErrorResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}
