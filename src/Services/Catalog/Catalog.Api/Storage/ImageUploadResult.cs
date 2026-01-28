namespace Catalog.Api.Storage;

public record ImageUploadResult(
    string ObjectKey,
    string Url,
    string FileName,
    string ContentType,
    long SizeBytes);
