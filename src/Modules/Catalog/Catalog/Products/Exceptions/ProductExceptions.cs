using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;

namespace Catalog.Products.Exceptions
{
    public class InvalidProductNameException : Exception
    {
        public InvalidProductNameException(string name) 
            : base($"Product name '{name}' is invalid. Name cannot be null, empty, or exceed 200 characters.")
        {
        }
    }

    public class InvalidProductPriceException : Exception
    {
        public InvalidProductPriceException(decimal price) 
            : base($"Product price '{price}' is invalid. Price must be greater than zero.")
        {
        }
    }

    public class InvalidProductCategoryException : Exception
    {
        public InvalidProductCategoryException(string message) 
            : base(message)
        {
        }
    }

    public class ProductNotFoundException: NotFoundException
    {
        public ProductNotFoundException(Guid productId) 
            : base($"Product with ID '{productId}' was not found.")
        {
        }

    }
}