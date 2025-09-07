using Basket.Basket.Modules;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Data.Repositories
{
    public class BasketRepository(BasketDbContext dbContext) : IBasketRepository
    {
        public async Task<ShoppingCart> CreateBasketAsync(ShoppingCart shoppingCart, CancellationToken token = default)
        {
            dbContext.ShoppingCarts.Add(shoppingCart);
            await dbContext.SaveChangesAsync(token);
            return shoppingCart;
        }

        public async Task<ShoppingCart> GetBasketAsync(string userName, bool asNoTracking = true, CancellationToken token = default)
        {
            var query = dbContext.ShoppingCarts.Include(x => x.Items).Where(u => u.UserName == userName);
            if (asNoTracking)
                query = query.AsNoTracking();

            var basket = await query.SingleOrDefaultAsync(token);
            return basket ?? throw new KeyNotFoundException($"Basket with UserName '{userName}' not found.");
        }

        public async Task<ShoppingCart?> GetBasketByIdAsync(Guid basketId, bool asNoTracking = true, CancellationToken token = default)
        {
            var query = dbContext.ShoppingCarts.Include(x => x.Items).Where(b => b.Id == basketId);
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(token);
        }

        public async Task<List<ShoppingCart>> GetBasketsByUserAsync(string userName, CancellationToken token = default)
        {
            return await dbContext.ShoppingCarts
                .Include(x => x.Items)
                .Where(b => b.UserName == userName)
                .ToListAsync(token);
        }

        public async Task<bool> DeleteBasketAsync(string userName, CancellationToken token = default)
        {
            var basket = await dbContext.ShoppingCarts
                .FirstOrDefaultAsync(x => x.UserName == userName, token);
            if (basket == null) return false;

            dbContext.ShoppingCarts.Remove(basket);
            await dbContext.SaveChangesAsync(token);
            return true;
        }

        public async Task<bool> DeleteBasketByIdAsync(Guid basketId, CancellationToken token = default)
        {
            var basket = await dbContext.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == basketId, token);
            if (basket == null) return false;

            dbContext.ShoppingCarts.Remove(basket);
            await dbContext.SaveChangesAsync(token);
            return true;
        }

        public async Task<List<ShoppingCart>> GetBasketsContainingProductAsync(Guid productId, CancellationToken token = default)
        {
            return await dbContext.ShoppingCarts
                .Include(x => x.Items)
                .Where(b => b.Items.Any(i => i.ProductId == productId))
                .ToListAsync(token);
        }

        public async Task<int> SaveChangesAsync(string? userName=null, CancellationToken token = default)
        {
            return await dbContext.SaveChangesAsync(token);
        }

        // Legacy methods for backward compatibility
        public async Task<ShoppingCart> CreateBasket(ShoppingCart shoppingCart, CancellationToken token = default)
        {
            return await CreateBasketAsync(shoppingCart, token);
        }

        public Task<ShoppingCart> GetBasket(string userName, bool asNoTracking = true, CancellationToken token = default)
        {
            return GetBasketAsync(userName, asNoTracking, token);
        }

        public async Task<bool> DeleteBasket(string userName, CancellationToken token = default)
        {
            return await DeleteBasketAsync(userName, token);
        }
    }
}
