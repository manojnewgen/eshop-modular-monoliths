using Basket.Basket.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basket.Data.Repositories
{
    public interface IBasketRepository
    {
        Task<ShoppingCart> GetBasketAsync(string userName, bool asNoTracking = true, CancellationToken token = default);
        
        Task<ShoppingCart?> GetBasketByIdAsync(Guid basketId, bool asNoTracking = true, CancellationToken token = default);
        
        Task<List<ShoppingCart>> GetBasketsByUserAsync(string userName, CancellationToken token = default);
        
        Task<ShoppingCart> CreateBasketAsync(ShoppingCart shoppingCart, CancellationToken token = default);
        
        Task<bool> DeleteBasketAsync(string userName, CancellationToken token = default);
        
        Task<bool> DeleteBasketByIdAsync(Guid basketId, CancellationToken token = default);

        Task<int> SaveChangesAsync(string? userName=null, CancellationToken token = default);

        Task<List<ShoppingCart>> GetBasketsContainingProductAsync(Guid productId, CancellationToken token = default);
        
        // Keep existing methods for backward compatibility
        Task<ShoppingCart> GetBasket(string userName, bool asNoTracking = true, CancellationToken token = default);
        Task<bool> DeleteBasket(string userName, CancellationToken token = default);
    }
}
