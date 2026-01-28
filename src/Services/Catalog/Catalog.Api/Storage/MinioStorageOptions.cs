namespace Catalog.Api.Storage;

public class MinioStorageOptions
{
    public string Endpoint { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string BucketName { get; set; } = default!;
    public bool UseSsl { get; set; }
    public string PublicBaseUrl { get; set; } = default!;
}
