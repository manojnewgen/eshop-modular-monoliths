using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts.CQRS;

namespace Shared.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>>? _logger;

        public ValidationBehavior(
            IEnumerable<IValidator<TRequest>> validators, 
            ILogger<ValidationBehavior<TRequest, TResponse>>? logger = null)
        {
            _validators = validators ?? Enumerable.Empty<IValidator<TRequest>>();
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Only validate commands, not queries
            if (request is not ICommand<TResponse> && request is not ICommand)
            {
                return await next();
            }

            // If no validators are registered, continue with the request
            if (!_validators.Any())
            {
                _logger?.LogDebug("No validators found for {RequestType}", typeof(TRequest).Name);
                return await next();
            }

            _logger?.LogDebug("Validating {RequestType} with {ValidatorCount} validator(s)", 
                typeof(TRequest).Name, _validators.Count());

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count > 0)
            {
                _logger?.LogWarning("Validation failed for {RequestType} with {ErrorCount} error(s)", 
                    typeof(TRequest).Name, failures.Count);
                
                throw new ValidationException(failures);
            }

            _logger?.LogDebug("Validation passed for {RequestType}", typeof(TRequest).Name);
            return await next();
        }
    }
}