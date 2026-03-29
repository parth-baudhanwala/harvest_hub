using Microsoft.ApplicationInsights;

namespace Functions.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddFunctionServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Get connection strings
        var cosmosConnectionString = configuration.GetConnectionString("CosmosDbConnection")
            ?? throw new InvalidOperationException("CosmosDbConnection not configured");
        
        var serviceBusConnectionString = configuration.GetConnectionString("ServiceBusConnection")
            ?? throw new InvalidOperationException("ServiceBusConnection not configured");

        var storageConnectionString = configuration.GetConnectionString("AzureWebJobsStorage")
            ?? throw new InvalidOperationException("AzureWebJobsStorage not configured");

        // Register Azure Clients directly
        services.AddSingleton(_ => new BlobServiceClient(storageConnectionString));
        services.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
        services.AddSingleton(_ => new CosmosClient(cosmosConnectionString));

        // Application Services
        services.AddScoped<ICosmosDbService, CosmosDbService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IServiceBusService, ServiceBusService>();
        services.AddSingleton<TelemetryClient>();

        // Configuration
        services.Configure<CosmosDbSettings>(configuration.GetSection("CosmosDbSettings"));
        services.Configure<ServiceBusSettings>(configuration.GetSection("ServiceBusSettings"));
        services.Configure<StorageSettings>(configuration.GetSection("StorageSettings"));
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        return services;
    }
}

public class CosmosDbSettings
{
    public string DatabaseName { get; set; } = default!;
    public string ContainerName { get; set; } = default!;
}

public class ServiceBusSettings
{
    public string Namespace { get; set; } = default!;
    public string SubscriptionName { get; set; } = default!;
}

public class StorageSettings
{
    public string BlobConnectionString { get; set; } = default!;
    public string ProductImageContainer { get; set; } = default!;
    public string ProcessedImagesContainer { get; set; } = default!;
    public string InvoicesContainer { get; set; } = default!;
    public string ReportsContainer { get; set; } = default!;
    public string AuditLogsContainer { get; set; } = default!;
}

public class EmailSettings
{
    public string SenderEmail { get; set; } = default!;
    public string SenderName { get; set; } = default!;
}
