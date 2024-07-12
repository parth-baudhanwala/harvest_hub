namespace OrderStream.Infrastructure.Data.Seeds;

internal static class InitialData
{
    public static IEnumerable<Customer> Customers =>
    [
        Customer.Create(CustomerId.Of(new Guid("58c49479-ec65-4de2-86e7-033c546291aa")), "Parth Baudhanwala", "parthb@gmail.com"),
        Customer.Create(CustomerId.Of(new Guid("189dc8dc-990f-48e0-a37b-e6f2b60b9d7d")), "Hailee", "hailee@gmail.com")
    ];

    public static IEnumerable<Product> Products =>
    [
        Product.Create(ProductId.Of(new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61")), "Mango", 950),
        Product.Create(ProductId.Of(new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914")), "Oreo", 30),
        Product.Create(ProductId.Of(new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8")), "Lady Fingers", 150),
        Product.Create(ProductId.Of(new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27")), "Amul Milk", 55)
    ];

    public static IEnumerable<Order> OrdersWithItems
    {
        get
        {
            Address address1 = Address.Of("Parth", "Baudhanwala", "parthb@gmail.com", "Rajhans Apartment", "India", "Gujarat", "31200");
            Address address2 = Address.Of("Hailee", "", "hailee@gmail.com", "Thousand Oaks, Tarzana, Los Angeles", "United States", "California", "90212");

            Payment payment1 = Payment.Of("Parth Baudhanwala", "5555555555554444", "12/28", "355", 1);
            Payment payment2 = Payment.Of("Hailee", "8885555555554444", "06/30", "222", 2);

            Order order1 = Order.Create(
                            OrderId.Of(Guid.NewGuid()),
                            CustomerId.Of(new Guid("58c49479-ec65-4de2-86e7-033c546291aa")),
                            OrderName.Of("ORD_1"),
                            shippingAddress: address1,
                            billingAddress: address1,
                            payment1);

            order1.Add(ProductId.Of(new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61")), 2, 1900);
            order1.Add(ProductId.Of(new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914")), 1, 30);

            Order order2 = Order.Create(
                            OrderId.Of(Guid.NewGuid()),
                            CustomerId.Of(new Guid("189dc8dc-990f-48e0-a37b-e6f2b60b9d7d")),
                            OrderName.Of("ORD_2"),
                            shippingAddress: address2,
                            billingAddress: address2,
                            payment2);

            order2.Add(ProductId.Of(new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8")), 1, 150);
            order2.Add(ProductId.Of(new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27")), 2, 110);

            return [order1, order2];
        }
    }
}
