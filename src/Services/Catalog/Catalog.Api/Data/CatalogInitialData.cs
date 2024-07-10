using Marten.Schema;

namespace Catalog.Api.Data;

public class CatalogInitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();

        if (await session.Query<Product>().AnyAsync(cancellation)) return;

        // Marten UPSERT will cater for existing records
        session.Store(GetPreconfiguredProducts());
        await session.SaveChangesAsync(cancellation);
    }

    private static IEnumerable<Product> GetPreconfiguredProducts() =>
    [
        new Product()
        {
            Id = new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61"),
            Name = "Mango",
            Description = "1 box kesar mangoes from Ratnagiri.",
            ImageFile = "product-1.png",
            Price = 950.00M,
            Categories = ["Fruits"]
        },
        new Product()
        {
            Id = new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914"),
            Name = "Oreo",
            Description = "Cadburry Oreo.",
            ImageFile = "product-2.png",
            Price = 30.00M,
            Categories = ["Biscuit"]
        },
        new Product()
        {
            Id = new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8"),
            Name = "Lady Fingers",
            Description = "1 Kelogram lady fingers.",
            ImageFile = "product-3.png",
            Price = 150.00M,
            Categories = ["Vegetables"]
        },
        new Product()
        {
            Id = new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27"),
            Name = "Grapes",
            Description = "1 Kelogram black grapes from Nasik.",
            ImageFile = "product-4.png",
            Price = 120.00M,
            Categories = ["Fruits"]
        },
        new Product()
        {
            Id = new Guid("b786103d-c621-4f5a-b498-23452610f88c"),
            Name = "Balaji Waffers",
            Description = "Balaji masala waffers.",
            ImageFile = "product-5.png",
            Price = 30.00M,
            Categories = ["Snacks"]
        },
        new Product()
        {
            Id = new Guid("c4bbc4a2-4555-45d8-97cc-2a99b2167bff"),
            Name = "Kurkure Solid Masti",
            Description = "Kurkure solid masti.",
            ImageFile = "product-6.png",
            Price = 25.00M,
            Categories = ["Snacks"]
        },
        new Product()
        {
            Id = new Guid("93170c85-7795-489c-8e8f-7dcf3b4f4188"),
            Name = "Amul Milk",
            Description = "1 liter Amul milk.",
            ImageFile = "product-7.png",
            Price = 55.00M,
            Categories = ["Dairy"]
        }
    ];
}
