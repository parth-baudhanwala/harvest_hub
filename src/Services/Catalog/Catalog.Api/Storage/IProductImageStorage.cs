namespace Catalog.Api.Storage;

public interface IProductImageStorage
{
    Task<ImageUploadResult> UploadAsync(Guid productId,
                                        string fileName,
                                        byte[] bytes,
                                        string? contentType,
                                        CancellationToken cancellationToken);
}
