
namespace Shared.Messaging.Events
{
    public record ProductPriceChangedIntegrationEvent : IntegrationEvent
    {
        public Guid ProductId { get; init; }= default!;
        public string Name { get; init; } =  default!;

        public List<string> Category { get; init; } = default!;

        public string Description { get; init; } = default!;

        public string ImageFile { get; init; } = default!;

        public decimal Price { get; init; } = default!;
    }
}
