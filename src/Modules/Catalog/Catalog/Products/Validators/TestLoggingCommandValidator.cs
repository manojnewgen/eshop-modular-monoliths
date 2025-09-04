using FluentValidation;
using Catalog.Products.Features.TestLogging;

namespace Catalog.Products.Validators
{
    /// <summary>
    /// Validator for TestLoggingCommand - demonstrates FluentValidation integration
    /// </summary>
    public class TestLoggingCommandValidator : AbstractValidator<TestLoggingCommand>
    {
        public TestLoggingCommandValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message is required")
                .MaximumLength(500)
                .WithMessage("Message cannot exceed 500 characters")
                .Must(NotContainProfanity)
                .WithMessage("Message contains inappropriate content");

            RuleFor(x => x.DelayMs)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Delay cannot be negative")
                .LessThanOrEqualTo(10000)
                .WithMessage("Delay cannot exceed 10 seconds");
        }

        private bool NotContainProfanity(string message)
        {
            // Simple profanity check for demo purposes
            var bannedWords = new[] { "badword1", "badword2", "spam" };
            return !bannedWords.Any(word => 
                message.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}