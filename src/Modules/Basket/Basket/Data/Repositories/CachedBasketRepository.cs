using Basket.Basket.Modules;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Basket.Data.Repositories
{
    public class CachedBasketRepository(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public async Task<ShoppingCart> CreateBasketAsync(ShoppingCart shoppingCart, CancellationToken token = default)
        {
            // Fixed: Actually create the basket, not get it
            var createdBasket = await repository.CreateBasketAsync(shoppingCart, token);
            
            // Cache the newly created basket
            await SetCacheAsync(createdBasket.UserName, createdBasket, token);
            
            return createdBasket;
        }

        public async Task<bool> DeleteBasket(string userName, CancellationToken token = default)
        {
            var result = await repository.DeleteBasket(userName, token);
            if (result)
            {
                await cache.RemoveAsync(userName, token);
            }
            return result;
        }

        public async Task<bool> DeleteBasketAsync(string userName, CancellationToken token = default)
        {
            var result = await repository.DeleteBasketAsync(userName, token);
            if (result)
            {
                await cache.RemoveAsync(userName, token);
            }
            return result;
        }

        public async Task<bool> DeleteBasketByIdAsync(Guid basketId, CancellationToken token = default)
        {
            var result = await repository.DeleteBasketByIdAsync(basketId, token);
            // Note: We can't easily remove from cache here without knowing the userName
            // Consider adding a reverse lookup cache or getting basket first
            return result;
        }

        public async Task<ShoppingCart> GetBasket(string userName, bool asNoTracking = true, CancellationToken token = default)
        {
            if (!asNoTracking)
                return await repository.GetBasket(userName, asNoTracking, token);

            // Try cache first
            var cachedBasket = await GetCacheAsync(userName, token);
            if (cachedBasket != null)
                return cachedBasket;

            // Cache miss - get from repository and cache it
            var basketFromRepo = await repository.GetBasket(userName, asNoTracking, token);
            await SetCacheAsync(userName, basketFromRepo, token);
            
            return basketFromRepo;
        }

        public async Task<ShoppingCart> GetBasketAsync(string userName, bool asNoTracking = true, CancellationToken token = default)
        {
            if (!asNoTracking)
                return await repository.GetBasketAsync(userName, asNoTracking, token);

            // Try cache first
            var cachedBasket = await GetCacheAsync(userName, token);
            if (cachedBasket != null)
                return cachedBasket;

            // Cache miss - get from repository and cache it
            var basketFromRepo = await repository.GetBasketAsync(userName, asNoTracking, token);
            await SetCacheAsync(userName, basketFromRepo, token);
            
            return basketFromRepo;
        }

        public async Task<ShoppingCart?> GetBasketByIdAsync(Guid basketId, bool asNoTracking = true, CancellationToken token = default)
        {
            // Note: Consider caching by basketId as well for better performance
            return await repository.GetBasketByIdAsync(basketId, asNoTracking, token);
        }

        public async Task<List<ShoppingCart>> GetBasketsByUserAsync(string userName, CancellationToken token = default)
        {
            // Note: Consider caching for multiple baskets as well
            return await repository.GetBasketsByUserAsync(userName, token);
        }

        public async Task<List<ShoppingCart>> GetBasketsContainingProductAsync(Guid productId, CancellationToken token = default)
        {
            return await repository.GetBasketsContainingProductAsync(productId, token);
        }

        public async Task<int> SaveChangesAsync(string? userName = null, CancellationToken token = default)
        {
            var result = await repository.SaveChangesAsync(userName, token);

            if (!string.IsNullOrEmpty(userName))
            {
                await cache.RemoveAsync(userName, token);
            }
            
            return result;
        }

        private async Task<ShoppingCart?> GetCacheAsync(string userName, CancellationToken token)
        {
            try
            {
                var cachedData = await cache.GetStringAsync(userName, token);
                if (string.IsNullOrEmpty(cachedData))
                    return null;

                return JsonSerializer.Deserialize<ShoppingCart>(cachedData, JsonOptions);
            }
            catch (JsonException)
            {
                // Remove corrupted cache entry
                await cache.RemoveAsync(userName, token);
                return null;
            }
        }

        private async Task SetCacheAsync(string userName, ShoppingCart basket, CancellationToken token)
        {
            try
            {
                var serializedBasket = JsonSerializer.Serialize(basket, JsonOptions);
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };
                
                await cache.SetStringAsync(userName, serializedBasket, cacheOptions, token);
            }
            catch (JsonException)
            {
                // Log error but don't fail the operation
                // Consider using ILogger here
            }
        }
    }
}
