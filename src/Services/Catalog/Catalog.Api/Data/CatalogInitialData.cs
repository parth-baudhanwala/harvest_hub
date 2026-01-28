using Catalog.Api.Storage;
using Marten.Schema;

namespace Catalog.Api.Data;

public class CatalogInitialData(
    IHttpClientFactory httpClientFactory,
    IProductImageStorage imageStorage,
    ILogger<CatalogInitialData> logger) : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();

        if (await session.Query<Product>().AnyAsync(cancellation)) return;

        var products = GetPreconfiguredProducts();

        foreach (var product in products)
        {
            if (cancellation.IsCancellationRequested)
            {
                break;
            }

            string? sourceUrl = GetSeedImageUrl(product.Id);
            if (string.IsNullOrWhiteSpace(sourceUrl))
            {
                continue;
            }

            try
            {
                using var httpClient = httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(sourceUrl, cancellation);
                response.EnsureSuccessStatusCode();

                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellation);
                string? contentType = response.Content.Headers.ContentType?.MediaType;
                string fileName = ResolveFileName(sourceUrl, contentType);

                ImageUploadResult upload = await imageStorage.UploadAsync(
                    product.Id,
                    fileName,
                    bytes,
                    contentType,
                    cancellation);

                product.ImageFile = upload.Url;
                product.ImageObjectKey = upload.ObjectKey;
                product.ImageFileName = upload.FileName;
                product.ImageContentType = upload.ContentType;
                product.ImageSizeBytes = upload.SizeBytes;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to seed image for product {ProductId} from {SourceUrl}.", product.Id, sourceUrl);
            }
        }

        // Marten UPSERT will cater for existing records
        session.Store(products);
        await session.SaveChangesAsync(cancellation);
    }

    private static IEnumerable<Product> GetPreconfiguredProducts() =>
    [
        new Product()
        {
            Id = new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61"),
            Name = "Mango",
            Description = "1 box kesar mangoes from Ratnagiri.",
            ImageFile = "https://aamrai.com/wp-content/uploads/2023/12/KESAR-scaled.jpg",
            Price = 950.00M,
            Categories = ["Fruits"]
        },
        new Product()
        {
            Id = new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914"),
            Name = "Oreo",
            Description = "Cadburry Oreo.",
            ImageFile = "https://ecom-su-static-prod.wtrecom.com/images/products/4/LN_448126_BP_4.jpg",
            Price = 30.00M,
            Categories = ["Biscuit"]
        },
        new Product()
        {
            Id = new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8"),
            Name = "Lady Fingers",
            Description = "1 Kelogram lady fingers.",
            ImageFile = "https://www.trustbasket.com/cdn/shop/articles/Lady_Finger.webp?v=1681366481",
            Price = 150.00M,
            Categories = ["Vegetables"]
        },
        new Product()
        {
            Id = new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27"),
            Name = "Grapes",
            Description = "1 Kelogram black grapes from Nasik.",
            ImageFile = "https://www.foodrepublic.com/img/gallery/15-types-of-grapes-to-know-eat-and-drink/thomcord-1743188190.jpg",
            Price = 120.00M,
            Categories = ["Fruits"]
        },
        new Product()
        {
            Id = new Guid("b786103d-c621-4f5a-b498-23452610f88c"),
            Name = "Balaji Waffers",
            Description = "Balaji masala waffers.",
            ImageFile = "https://www.balajiwafers.com/cdn/shop/files/Crunchem__Chaat_Chaska_Wafers_Carousal_01_0097a0df-79db-4f3c-a168-3c0a54fa4f2e.jpg?v=1746169312",
            Price = 30.00M,
            Categories = ["Snacks"]
        },
        new Product()
        {
            Id = new Guid("c4bbc4a2-4555-45d8-97cc-2a99b2167bff"),
            Name = "Kurkure Solid Masti",
            Description = "Kurkure solid masti.",
            ImageFile = "https://www.bbassets.com/media/uploads/p/l/40070746_7-kurkure-namkeen-solid-masti-twisteez.jpg",
            Price = 25.00M,
            Categories = ["Snacks"]
        },
        new Product()
        {
            Id = new Guid("93170c85-7795-489c-8e8f-7dcf3b4f4188"),
            Name = "Amul Milk",
            Description = "1 liter Amul milk.",
            ImageFile = "https://www.bbassets.com/media/uploads/p/l/40090893_8-amul-amul-gold.jpg",
            Price = 55.00M,
            Categories = ["Dairy"]
        }
    ];

    private static string? GetSeedImageUrl(Guid productId)
    {
        return productId switch
        {
            var id when id == new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61") =>
                "https://aamrai.com/wp-content/uploads/2023/12/KESAR-scaled.jpg",
            var id when id == new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914") =>
                "https://ecom-su-static-prod.wtrecom.com/images/products/4/LN_448126_BP_4.jpg",
            var id when id == new Guid("4f136e9f-ff8c-4c1f-9a33-d12f689bdab8") =>
                "https://www.trustbasket.com/cdn/shop/articles/Lady_Finger.webp?v=1681366481",
            var id when id == new Guid("6ec1297b-ec0a-4aa1-be25-6726e3b51a27") =>
                "https://www.foodrepublic.com/img/gallery/15-types-of-grapes-to-know-eat-and-drink/thomcord-1743188190.jpg",
            var id when id == new Guid("b786103d-c621-4f5a-b498-23452610f88c") =>
                "https://www.balajiwafers.com/cdn/shop/files/Crunchem__Chaat_Chaska_Wafers_Carousal_01_0097a0df-79db-4f3c-a168-3c0a54fa4f2e.jpg?v=1746169312",
            var id when id == new Guid("c4bbc4a2-4555-45d8-97cc-2a99b2167bff") =>
                "https://www.bbassets.com/media/uploads/p/l/40070746_7-kurkure-namkeen-solid-masti-twisteez.jpg",
            var id when id == new Guid("93170c85-7795-489c-8e8f-7dcf3b4f4188") =>
                "https://www.bbassets.com/media/uploads/p/l/40090893_8-amul-amul-gold.jpg",
            _ => null
        };
    }

    private static string ResolveFileName(string imageUrl, string? contentType)
    {
        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            string candidate = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        string extension = contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            _ => ".img"
        };

        return $"image{extension}";
    }
}
