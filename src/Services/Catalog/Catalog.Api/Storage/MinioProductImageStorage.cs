using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;

namespace Catalog.Api.Storage;

public class MinioProductImageStorage(
    IMinioClient minioClient,
    IOptions<MinioStorageOptions> options,
    ILogger<MinioProductImageStorage> logger) : IProductImageStorage
{
    private readonly MinioStorageOptions _options = options.Value;

    public async Task<ImageUploadResult> UploadAsync(Guid productId,
                                                     string fileName,
                                                     byte[] bytes,
                                                     string? contentType,
                                                     CancellationToken cancellationToken)
    {
        if (bytes is null || bytes.Length == 0)
        {
            throw new ArgumentException("Image bytes are required.", nameof(bytes));
        }

        string safeFileName = SanitizeFileName(fileName);
        ValidateImageExtension(safeFileName);
        string objectKey = $"products/{productId}/{safeFileName}";
        string resolvedContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType;

        await EnsureBucketAsync(cancellationToken);

        await using var stream = new MemoryStream(bytes);
        await minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(resolvedContentType),
            cancellationToken);

        string publicUrl = BuildPublicUrl(objectKey);

        logger.LogInformation("Uploaded product image {ObjectKey} to bucket {Bucket}.", objectKey, _options.BucketName);

        return new ImageUploadResult(objectKey, publicUrl, safeFileName, resolvedContentType, bytes.LongLength);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        bool exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.BucketName),
            cancellationToken);

        if (exists)
        {
            return;
        }

        await minioClient.MakeBucketAsync(
            new MakeBucketArgs().WithBucket(_options.BucketName),
            cancellationToken);

        string policyJson = $$"""
        {
          "Version": "2012-10-17",
          "Statement": [
            {
              "Effect": "Allow",
              "Principal": { "AWS": ["*"] },
              "Action": ["s3:GetObject"],
              "Resource": ["arn:aws:s3:::{{_options.BucketName}}/*"]
            }
          ]
        }
        """;

        try
        {
            await minioClient.SetPolicyAsync(
                new SetPolicyArgs()
                    .WithBucket(_options.BucketName)
                    .WithPolicy(policyJson),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to set bucket policy for {Bucket}. Configure public read manually if needed.", _options.BucketName);
        }
    }

    private string BuildPublicUrl(string objectKey)
    {
        string baseUrl = _options.PublicBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{_options.BucketName}/{objectKey}";
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "image";
        }

        return Path.GetFileName(fileName);
    }

    private static void ValidateImageExtension(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Image file extension is required.", nameof(fileName));
        }

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp",
            ".svg"
        };

        if (!allowed.Contains(extension))
        {
            throw new ArgumentException($"Unsupported image extension: {extension}", nameof(fileName));
        }
    }
}
