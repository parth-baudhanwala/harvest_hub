using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization;

namespace Functions.Shared;

/// <summary>
/// Cosmos DB service for analytics and event audit trail
/// </summary>
public interface ICosmosDbService
{
    Task<T?> GetDocumentAsync<T>(string id, string containerName);
    Task<T> CreateDocumentAsync<T>(T document, string containerName);
    Task<T> UpdateDocumentAsync<T>(T document, string containerName);
    Task DeleteDocumentAsync(string id, string containerName);
    Task<IEnumerable<T>> QueryAsync<T>(string query, string containerName);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _client;
    private readonly string _databaseId;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(CosmosClient client, IConfiguration config, ILogger<CosmosDbService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _databaseId = config["CosmosDbSettings:DatabaseName"] ?? "HarvestHubAnalytics";
        _logger = logger;
    }

    public async Task<T?> GetDocumentAsync<T>(string id, string containerName)
    {
        try
        {
            var container = _client.GetContainer(_databaseId, containerName);
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading document {Id} from {Container}", id, containerName);
            throw;
        }
    }

    public async Task<T> CreateDocumentAsync<T>(T document, string containerName)
    {
        try
        {
            var container = _client.GetContainer(_databaseId, containerName);
            var response = await container.CreateItemAsync(document);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document in {Container}", containerName);
            throw;
        }
    }

    public async Task<T> UpdateDocumentAsync<T>(T document, string containerName)
    {
        try
        {
            var container = _client.GetContainer(_databaseId, containerName);
            var response = await container.UpsertItemAsync(document);
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document in {Container}", containerName);
            throw;
        }
    }

    public async Task DeleteDocumentAsync(string id, string containerName)
    {
        try
        {
            var container = _client.GetContainer(_databaseId, containerName);
            await container.DeleteItemAsync<dynamic>(id, new PartitionKey(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id} from {Container}", id, containerName);
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string query, string containerName)
    {
        try
        {
            var container = _client.GetContainer(_databaseId, containerName);
            var iterator = container.GetItemQueryIterator<T>(query);
            var results = new List<T>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying documents from {Container}", containerName);
            throw;
        }
    }
}

/// <summary>
/// Blob Storage service for file operations
/// </summary>
public interface IBlobStorageService
{
    Task<string> UploadBlobAsync(Stream content, string blobName, string containerName);
    Task<Stream> DownloadBlobAsync(string blobName, string containerName);
    Task<bool> DeleteBlobAsync(string blobName, string containerName);
    Task<IEnumerable<string>> ListBlobsAsync(string containerName, string? prefix = null, CancellationToken cancellationToken = default);
    Task<string> GetBlobSasUriAsync(string blobName, string containerName, TimeSpan expiresIn);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        _logger = logger;
    }

    public async Task<string> UploadBlobAsync(Stream content, string blobName, string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, overwrite: true);
            
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob {BlobName} to {Container}", blobName, containerName);
            throw;
        }
    }

    public async Task<Stream> DownloadBlobAsync(string blobName, string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            
            var download = await blobClient.DownloadAsync();
            var stream = new MemoryStream();
            await download.Value.Content.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading blob {BlobName} from {Container}", blobName, containerName);
            throw;
        }
    }

    public async Task<bool> DeleteBlobAsync(string blobName, string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            
            var result = await blobClient.DeleteIfExistsAsync();
            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blob {BlobName} from {Container}", blobName, containerName);
            throw;
        }
    }

    public async Task<IEnumerable<string>> ListBlobsAsync(string containerName, string? prefix = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.All, BlobStates.All, prefix, cancellationToken))
            {
                blobs.Add(blobItem.Name);
            }

            return blobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs in {Container}", containerName);
            throw;
        }
    }

    public async Task<string> GetBlobSasUriAsync(string blobName, string containerName, TimeSpan expiresIn)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            
            var sasUri = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiresIn));
            return sasUri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SAS URI for blob {BlobName}", blobName);
            throw;
        }
    }
}

/// <summary>
/// Service Bus service for message operations
/// </summary>
public interface IServiceBusService
{
    Task PublishEventAsync<T>(T @event, string topicName);
    Task SendCommandAsync<T>(T command, string queueName);
}

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusService> _logger;

    public ServiceBusService(ServiceBusClient client, ILogger<ServiceBusService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger;
    }

    public async Task PublishEventAsync<T>(T @event, string topicName)
    {
        try
        {
            var sender = _client.CreateSender(topicName);
            var message = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(@event))
            {
                Subject = typeof(T).Name,
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Published event {EventType} to {Topic}", typeof(T).Name, topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to {Topic}", topicName);
            throw;
        }
    }

    public async Task SendCommandAsync<T>(T command, string queueName)
    {
        try
        {
            var sender = _client.CreateSender(queueName);
            var message = new ServiceBusMessage(System.Text.Json.JsonSerializer.Serialize(command))
            {
                Subject = typeof(T).Name,
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Sent command {CommandType} to {Queue}", typeof(T).Name, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command to {Queue}", queueName);
            throw;
        }
    }
}
