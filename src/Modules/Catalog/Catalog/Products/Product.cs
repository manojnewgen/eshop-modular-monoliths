using Shared.DDD;
using Catalog.Products.Events;
using Catalog.Products.Exceptions;

namespace Catalog.Products
{
    public class Product : Aggregate<Guid>
    {
        private readonly List<string> _categories = new();

        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public IReadOnlyList<string> Categories => _categories.AsReadOnly();
        public decimal Price { get; private set; }
        public string ImageFile { get; private set; } = default!;
        
        // Additional properties for database compatibility
        public int StockQuantity { get; private set; }
        public bool IsAvailable { get; private set; } = true;

        // Soft delete support (audit fields are inherited from Entity<T>)
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }

        // Required for EF Core
        private Product() { }

        // Private constructor for controlled creation
        private Product(Guid id, string name, string description, decimal price, string imageFile) : base(id)
        {
            Name = name;
            Description = description;
            Price = price;
            ImageFile = imageFile;
            StockQuantity = 0;
            IsAvailable = true;
            IsDeleted = false;
            // CreatedAt and CreatedBy will be set automatically by the interceptor
        }

        // Factory method for creating a new product
        public static Product Create(
            Guid id,
            string name,
            string description,
            decimal price,
            string imageFile,
            List<string>? categories = null,
            int stockQuantity = 0)
        {
            ValidateName(name);
            ValidatePrice(price);
            ValidateDescription(description);
            ValidateImageFile(imageFile);

            var product = new Product(
                id,
                name.Trim(),
                description.Trim(),
                price,
                imageFile.Trim())
            {
                StockQuantity = stockQuantity
            };

            if (categories != null && categories.Any())
            {
                product.AddCategories(categories);
            }

            // Raise domain event
            product.AddDomainEvent(new ProductCreatedEvent(
                product.Id, 
                product.Name, 
                product.Price, 
                product._categories.ToList()));

            return product;
        }

        // Business logic methods
        public void UpdatePrice(decimal newPrice, string reason = "Price update")
        {
            ValidatePrice(newPrice);

            if (Price == newPrice)
                return;

            var oldPrice = Price;
            Price = newPrice;

            AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice, reason));
        }

        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative");
            
            StockQuantity = quantity;
            IsAvailable = quantity > 0;
        }

        public void UpdateName(string newName)
        {
            ValidateName(newName);
            Name = newName.Trim();
        }

        public void UpdateDescription(string newDescription)
        {
            ValidateDescription(newDescription);
            Description = newDescription.Trim();
        }

        public void UpdateImageFile(string newImageFile)
        {
            ValidateImageFile(newImageFile);
            ImageFile = newImageFile.Trim();
        }

        public void AddCategory(string category)
        {
            ValidateCategory(category);
            
            if (_categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                return;

            var oldCategories = _categories.ToList();
            _categories.Add(category);

            AddDomainEvent(new ProductCategoriesUpdatedEvent(Id, oldCategories, _categories.ToList()));
        }

        public void AddCategories(IEnumerable<string> categories)
        {
            var categoriesToAdd = categories.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            
            if (!categoriesToAdd.Any())
                return;

            var oldCategories = _categories.ToList();
            
            foreach (var category in categoriesToAdd)
            {
                ValidateCategory(category);
                if (!_categories.Contains(category, StringComparer.OrdinalIgnoreCase))
                {
                    _categories.Add(category);
                }
            }

            if (!oldCategories.SequenceEqual(_categories))
            {
                AddDomainEvent(new ProductCategoriesUpdatedEvent(Id, oldCategories, _categories.ToList()));
            }
        }

        public void RemoveCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return;

            var oldCategories = _categories.ToList();
            var removed = _categories.RemoveAll(c => 
                string.Equals(c, category, StringComparison.OrdinalIgnoreCase));

            if (removed > 0)
            {
                AddDomainEvent(new ProductCategoriesUpdatedEvent(Id, oldCategories, _categories.ToList()));
            }
        }

        public void ClearCategories()
        {
            if (!_categories.Any())
                return;

            var oldCategories = _categories.ToList();
            _categories.Clear();

            AddDomainEvent(new ProductCategoriesUpdatedEvent(Id, oldCategories, new List<string>()));
        }

        public bool HasCategory(string category)
        {
            return _categories.Contains(category, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsInPriceRange(decimal minPrice, decimal maxPrice)
        {
            return Price >= minPrice && Price <= maxPrice;
        }

        public void ApplyDiscount(decimal discountPercentage, string reason = "Discount applied")
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100.");

            var discountAmount = Price * (discountPercentage / 100);
            var newPrice = Price - discountAmount;
            
            UpdatePrice(newPrice, reason);
        }

        // Soft delete method
        public void SoftDelete()
        {
            if (IsDeleted)
                return;

            IsDeleted = true;
            IsAvailable = false;
            
            // DeletedAt and DeletedBy will be set automatically by the interceptor
            AddDomainEvent(new ProductDeletedEvent(Id, Name));
        }

        // Restore from soft delete
        public void Restore()
        {
            if (!IsDeleted)
                return;

            IsDeleted = false;
            IsAvailable = StockQuantity > 0;
            
            AddDomainEvent(new ProductRestoredEvent(Id, Name));
        }

        // Validation methods
        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidProductNameException(name ?? "null");

            if (name.Length > 200)
                throw new InvalidProductNameException(name);
        }

        private static void ValidatePrice(decimal price)
        {
            if (price <= 0)
                throw new InvalidProductPriceException(price);
        }

        private static void ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Product description cannot be null or empty.");

            if (description.Length > 1000)
                throw new ArgumentException("Product description cannot exceed 1000 characters.");
        }

        private static void ValidateImageFile(string imageFile)
        {
            if (string.IsNullOrWhiteSpace(imageFile))
                throw new ArgumentException("Product image file cannot be null or empty.");
        }

        private static void ValidateCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new InvalidProductCategoryException("Category cannot be null or empty.");

            if (category.Length > 100)
                throw new InvalidProductCategoryException("Category name cannot exceed 100 characters.");
        }
    }
}
