using Shared.Exceptions;

namespace Catalog.Products.Exceptions
{
    /// <summary>
    /// Product-specific exception classes
    /// </summary>
    public static class ProductExceptions
    {
        public static NotFoundException ProductNotFound(Guid productId)
        {
            return new NotFoundException($"Product with ID {productId} was not found");
        }

        public static BadRequestException InvalidProductPrice(decimal price)
        {
            return new BadRequestException(
                "Invalid product price",
                $"Price {price:C} is not valid. Price must be greater than 0.");
        }

        public static BadRequestException InvalidStockQuantity(int quantity)
        {
            return new BadRequestException(
                "Invalid stock quantity",
                $"Stock quantity {quantity} is not valid. Quantity cannot be negative.");
        }

        public static BadRequestException ProductNameTooLong(string name)
        {
            return new BadRequestException(
                "Product name too long",
                $"Product name '{name}' exceeds the maximum length of 200 characters.");
        }

        public static BadRequestException DuplicateProduct(string name)
        {
            return new BadRequestException(
                "Duplicate product",
                $"A product with name '{name}' already exists.");
        }

        public static InternalServerException ProductCreationFailed(string reason)
        {
            return new InternalServerException(
                "Product creation failed",
                $"Failed to create product: {reason}");
        }
    }
}