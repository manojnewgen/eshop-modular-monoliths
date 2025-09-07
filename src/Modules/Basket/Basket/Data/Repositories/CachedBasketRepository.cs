using Basket.Basket.Modules;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basket.Data.Repositories
{
    public class CachedBasketRepository(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
    {
        public async Task<ShoppingCart> CreateBasketAsync(ShoppingCart shoppingCart, CancellationToken token = default)
        {
            return await repository.GetBasket(shoppingCart.UserName, true, token);
        }

        public async Task<bool> DeleteBasket(string userName, CancellationToken token = default)
        {
            return await repository.DeleteBasket(userName, token);
        }

        public async Task<bool> DeleteBasketAsync(string userName, CancellationToken token = default)
        {
            return await repository.DeleteBasketAsync(userName, token);
        }

        public async Task<bool> DeleteBasketByIdAsync(Guid basketId, CancellationToken token = default)
        {
            return await repository.DeleteBasketByIdAsync(basketId, token);
        }

        public async Task<ShoppingCart> GetBasket(string userName, bool asNoTracking = true, CancellationToken token = default)
        {
            if (asNoTracking == false)
                return await repository.GetBasket(userName, asNoTracking, token);

            var cacheBasket = await cache.GetStringAsync(userName, token);
            if (cacheBasket != null)
            {
                var cachedCart = System.Text.Json.JsonSerializer.Deserialize<ShoppingCart>(cacheBasket);
                if (cachedCart != null)
                    return cachedCart;
            }

            // If cache miss or deserialization fails, fall back to fetching from repository
            var basketFromRepo = await repository.GetBasket(userName, asNoTracking, token);
            var serializedBasket = System.Text.Json.JsonSerializer.Serialize(basketFromRepo);
            await cache.SetStringAsync(userName, serializedBasket, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Cache for 30 minutes
            }, token);
            return basketFromRepo;
        }

        public async Task<ShoppingCart> GetBasketAsync(string userName, bool asNoTracking = true, CancellationToken token = default)
        {
            return await repository.GetBasketAsync(userName, asNoTracking, token);
        }

        public async Task<ShoppingCart?> GetBasketByIdAsync(Guid basketId, bool asNoTracking = true, CancellationToken token = default)
        {
            return await repository.GetBasketByIdAsync(basketId, asNoTracking, token);
        }

        public async Task<List<ShoppingCart>> GetBasketsByUserAsync(string userName, CancellationToken token = default)
        {
            return await repository.GetBasketsByUserAsync(userName, token);
        }

        public async Task<List<ShoppingCart>> GetBasketsContainingProductAsync(Guid productId, CancellationToken token = default)
        {
            return await repository.GetBasketsContainingProductAsync(productId, token);
        }

        public async Task<int> SaveChangesAsync(string? userName = null, CancellationToken token = default)
        {
            var result = await repository.SaveChangesAsync(userName, token);

            if(userName is not null)
            {
               await cache.RemoveAsync(userName, token);    
            }
            return result;
        }
    }
}
