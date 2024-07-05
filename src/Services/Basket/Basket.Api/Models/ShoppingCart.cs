namespace Basket.Api.Models;

public class ShoppingCart
{
    public ShoppingCart() { }

    public ShoppingCart(string username)
    {
        Username = username;
    }

    public string Username { get; set; } = default!;

    public List<ShoppingCartItem> Items { get; set; } = [];

    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
}
