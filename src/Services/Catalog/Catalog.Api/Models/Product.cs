namespace Catalog.Api.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<string> Categories { get; set; } = [];
    public string ImageFile { get; set; } = default!;
    public string? ImageObjectKey { get; set; }
    public string? ImageFileName { get; set; }
    public string? ImageContentType { get; set; }
    public long? ImageSizeBytes { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = default!;
}
