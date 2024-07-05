using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Api.Data;

public class BasketCacheRepository(IBasketRepository repository, IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string username, CancellationToken cancellationToken = default)
    {
        string? basketCache = await cache.GetStringAsync(username, cancellationToken);

        if (!string.IsNullOrEmpty(basketCache))
        {
            return JsonSerializer.Deserialize<ShoppingCart>(basketCache)!;
        }

        ShoppingCart cart = await repository.GetBasket(username, cancellationToken);
        await cache.SetStringAsync(username, JsonSerializer.Serialize(cart), cancellationToken);
        return cart;
    }

    public async Task<bool> StoreBasket(ShoppingCart cart, CancellationToken cancellationToken = default)
    {
        bool isSuccess = await repository.StoreBasket(cart, cancellationToken);
        await cache.SetStringAsync(cart.Username, JsonSerializer.Serialize(cart), cancellationToken);
        return isSuccess;
    }

    public async Task<bool> DeleteBasket(string username, CancellationToken cancellationToken = default)
    {
        bool isSuccess = await repository.DeleteBasket(username, cancellationToken);
        await cache.RemoveAsync(username, cancellationToken);
        return isSuccess;
    }
}
