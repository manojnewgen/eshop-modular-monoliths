using Catalog.Products.Dtos;
using Catalog.Data;
using Catalog.Products;
using Microsoft.Extensions.Logging;
using Shared.CQRS;
using Shared.Mapping;
using FluentValidation;

namespace Catalog.Products.Features.CreateProduct
{
    /// <summary>
    /// Command to create a new product using ProductDto
    /// </summary>
    public record CreateProductCommand(ProductDto Product) : ICommand<CreateProductResult>;

    /// <summary>
    /// Result of creating a product
    /// </summary>
    public record CreateProductResult(Guid ProductId);

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Product.Name).NotEmpty()
            .WithMessage("Name is required");

            RuleFor(x => x.Product.Categories).NotEmpty()
           .WithMessage("Category is required");
            RuleFor(x => x.Product.Price).GreaterThan(0).WithMessage("Product price should be greater than 0");
            RuleFor(x => x.Product.ImageFile).NotEmpty()
           .WithMessage("Image is required");

            RuleFor(x => x.Product.Name).NotEmpty()
          .WithMessage("Name is required");

        }
    }

    /// <summary>
    /// Handler for CreateProductCommand - creates a new product using domain model and Mapster
    /// </summary>
    public class CreateProductHandler(CatalogDbContext _dbContext,
            ILogger<CreateProductHandler> _logger,
            IMappingService _mappingService)
        : ICommandHandler<CreateProductCommand, CreateProductResult>
    {

        public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
        {
           
            _logger.LogInformation("Creating product: {ProductName}", command.Product.Name);


            try
            {
                // Use Mapster to map ProductDto to Product domain entity
                var product = _mappingService.Map<ProductDto, Product>(command.Product);

                // Add to database context
                _dbContext.Products.Add(product);

                // Save changes - this will trigger your SaveChanges interceptor automatically!
                // ✅ Audit fields set automatically (CreatedAt, CreatedBy)
                // ✅ Domain events dispatched automatically (ProductCreatedEvent)
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Product created successfully with ID: {ProductId} using Mapster mapping", product.Id);

                return new CreateProductResult(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create product: {ProductName}", command.Product.Name);
                throw;
            }
        }
    }
}
