# Azure Functions - Phase 1: Foundation & Infrastructure

## Overview

This directory contains the Azure Functions implementation for HarvestHub. The structure follows a modular approach organized by trigger type:

- **EventDriven/** - Service Bus triggered functions (Phase 2)
- **Http/** - HTTP-triggered REST endpoints (Phase 3)
- **Scheduled/** - Timer-triggered background jobs (Phase 4)
- **DataProcessing/** - Blob Storage and file processing (Phase 5)
- **Shared/** - Common utilities, models, and services
- **Tests/** - Unit and integration tests

## Phase 1: Foundation Setup (Current)

### Infrastructure Deployment

All Azure resources are defined as Infrastructure-as-Code using Bicep templates in the `/IaC` folder.

#### Resources Created:
1. **Storage Account** - Blob containers for files, queues for processing
2. **Service Bus Namespace** - Topics and subscriptions for event messaging
3. **Cosmos DB Account** - Serverless database for analytics and audit trails
4. **Application Insights** - Monitoring, logging, and performance tracking
5. **Function App** - Compute runtime for Azure Functions
6. **Managed Identities** - Secure authentication without connection strings

#### Deployment Steps:

##### Step 1: Prerequisites
```powershell
# Install required tools
choco install azure-cli
choco install azure-functions-core-tools-4

# Login to Azure
az login

# List available subscriptions
az account list -o table
```

##### Step 2: Deploy Infrastructure
```powershell
# Navigate to scripts folder
cd scripts

# Run deployment script
.\Deploy-Infrastructure.ps1 `
    -SubscriptionId "YOUR_SUBSCRIPTION_ID" `
    -ResourceGroupName "harvesthub-functions-rg" `
    -Location "eastus" `
    -Environment "dev"
```

**Expected Output:**
```
[INFO] Starting HarvestHub Infrastructure Deployment...
[INFO] Step 1: Authenticating to Azure...
[INFO] Step 2: Creating/verifying Resource Group...
[INFO] Step 3: Validating Bicep template...
[INFO] Step 4: Deploying infrastructure using Bicep...
[INFO] Deployment succeeded!
[INFO] Step 5: Deployment outputs:
  storageAccountName: harvesthubxxxxxxxxxxxxx
  serviceBusNamespaceName: harvesthub-dev-sb
  cosmosAccountName: harvesthub-dev-cosmos
  functionAppName: harvesthub-dev-func
  appInsightsInstrumentationKey: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

**Saved to:** `Functions/azure-deployment.json`

##### Step 3: Setup Local Development Environment
```powershell
# Run setup script
.\Setup-LocalEnvironment.ps1

# OR with existing tools
.\Setup-LocalEnvironment.ps1 -SkipToolsInstall
```

##### Step 4: Configure Local Settings
```powershell
# Update connection strings from deployment
.\Update-LocalSettings.ps1 -DeploymentOutputJson ../Functions/azure-deployment.json

# OR use local emulators
.\Update-LocalSettings.ps1 -UseEmulator
```

### Project Structure

```
Functions/
├── Functions.csproj                 # Project file with NuGet dependencies
├── Program.cs                       # Startup and dependency injection
├── host.json                        # Functions runtime configuration
├── local.settings.json              # Local development settings
├── .funcignore                      # Deployment exclusions
├── GlobalUsings.cs                  # Global namespace imports
│
├── Shared/
│   ├── GlobalUsings.cs             # Common using statements
│   ├── Models.cs                   # Domain and data models
│   ├── Services.cs                 # Core services (Cosmos DB, Blob, Service Bus)
│   ├── Events.cs                   # Integration event definitions
│   └── DependencyInjection.cs       # DI configuration and settings
│
├── EventDriven/                     # Phase 2: Service Bus triggers
│   ├── OrderProcessingFunction.cs
│   ├── ProductSyncFunction.cs
│   ├── UserRegistrationFunction.cs
│   └── OrderNotificationFunction.cs
│
├── Http/                           # Phase 3: HTTP triggers
│   ├── OrderStatusWebhookFunction.cs
│   ├── NotificationStatusFunction.cs
│   ├── BatchExportFunction.cs
│   ├── AdminAnalyticsFunction.cs
│   └── HealthCheckFunction.cs
│
├── Scheduled/                      # Phase 4: Timer triggers
│   ├── InventorySyncFunction.cs
│   ├── AbandonedCartCleanupFunction.cs
│   ├── OrderStatusReconciliationFunction.cs
│   ├── DailySalesReportFunction.cs
│   └── CosmosDbMaintenanceFunction.cs
│
├── DataProcessing/                 # Phase 5: Blob triggers
│   ├── ProductImageProcessingFunction.cs
│   ├── InvoiceGenerationFunction.cs
│   ├── BatchCsvImportFunction.cs
│   └── AuditLogArchivalFunction.cs
│
└── Tests/                          # Unit and integration tests
    └── Functions.Tests.csproj
```

### Configuration Files

#### host.json
- **functionTimeout**: 5 minutes (adjustable per function via attribute)
- **functionLanguageWorker**: Dotnet isolated worker (in-process evolution)
- **healthMonitor**: Enabled to detect runtime issues
- **applicationInsights**: Sampling configured (100% during dev, tunable in prod)

#### local.settings.json
Stores local development configuration and connection strings:
- `AzureWebJobsStorage`: Points to local emulator or development storage account
- `FUNCTIONS_WORKER_RUNTIME`: Dotnet-isolated (better isolation than in-process)
- `ServiceBusConnection`: Service Bus connection string
- `CosmosDbConnection`: Cosmos DB connection string  
- Database connections: Order, Catalog, Basket databases

**⚠️ Important:** Do NOT commit local.settings.json to source control (it's in .gitignore)

### NuGet Dependencies

Core packages installed:
- `Microsoft.Azure.WebJobs.Extensions.ServiceBus` - Service Bus triggers/bindings
- `Microsoft.Azure.WebJobs.Extensions.Storage` - Storage triggers/bindings
- `Microsoft.Azure.Cosmos` - Cosmos DB SDK
- `Azure.Storage.Blobs` - Blob Storage SDK
- `Azure.Messaging.ServiceBus` - Service Bus operations
- `Microsoft.ApplicationInsights` - Monitoring
- `Microsoft.Extensions.Azure` - Azure client integration
- `Azure.Identity` - Managed Identity support
- `Polly` - Resilience and retry policies
- `Microsoft.EntityFrameworkCore.SqlServer` & `Npgsql` - Database access

### Dependency Injection Architecture

Services are registered in `Shared/DependencyInjection.cs`:

```csharp
// Available services in all functions:
ICosmosDbService       // Cosmos DB operations
IBlobStorageService    // Blob Storage operations
IServiceBusService     // Service Bus messaging
ILogger<T>             // Structured logging
IConfiguration         // Configuration values
IHttpClientFactory     // Resilient HTTP client
```

#### Example: Function with DI
```csharp
public class MyFunction
{
    private readonly ICosmosDbService _cosmosDb;
    private readonly ILogger<MyFunction> _logger;

    public MyFunction(ICosmosDbService cosmosDb, ILogger<MyFunction> logger)
    {
        _cosmosDb = cosmosDb;
        _logger = logger;
    }

    [Function("MyFunction")]
    public async Task Run([ServiceBusTrigger("my-topic", "my-subscription")] ServiceBusReceivedMessage message)
    {
        // Function logic
    }
}
```

### Managed Identity & RBAC

The Function App uses Managed Identity for secure access to:
- **Service Bus**: `Azure Service Bus Data Owner` role
- **Blob Storage**: `Storage Blob Data Contributor` role
- **Cosmos DB**: `Cosmos DB Built-in Data Contributor` role

**Benefit**: No connection strings stored in Function AppSettings; authentication automatically handled by Azure.

### Bicep Infrastructure Template

Located in `/IaC/main.bicep`:

**Resources:**
- Storage Account with 5 Blob containers
- Service Bus Namespace with 4 Topics and Subscriptions
- Cosmos DB Account with "HarvestHubAnalytics" database (serverless)
- Application Insights for centralizedmonitoring
- App Service Plan (B2 tier, configurable)
- Function App with System-Assigned Managed Identity
- 3 Role Assignments for security

**Customization:**
```bicep
param appServicePlanTier string = 'B2'  // Change to B1, B3, P1V2 per your needs
param environment string = 'dev'         // dev, staging, prod
param location string = resourceGroup().location
```

**Deploy Custom Bicep:**
```powershell
New-AzResourceGroupDeployment `
    -ResourceGroupName "my-rg" `
    -TemplateFile "./IaC/main.bicep" `
    -appServicePlanTier "P1V2" `
    -environment "prod"
```

### Local Development with Emulators

#### Option 1: Azure Storage Emulator (Azurite)
```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

#### Option 2: Cosmos DB Emulator
```bash
docker run -p 8081:8081 -e AZURE_COSMOS_EMULATOR_PARTITION_COUNT=3 mcr.microsoft.com/cosmosdb/linux:latest
```

#### Option 3: Service Bus Mock (RabbitMQ - not true Service Bus)
For true local Service Bus testing, use Azure Service Bus Emulator or connect to real Service Bus:
```powershell
# Configure in local.settings.json
"ServiceBusConnection": "Endpoint=sb://your-namespace.servicebus.windows.net/;..."
```

### Running Functions Locally

1. **Install extensions:**
   ```powershell
   cd Functions
   func extensions install
   ```

2. **Start the runtime:**
   ```powershell
   func start
   ```

3. **Expected output:**
   ```
   Azure Functions Core Tools
   Worker runtime: dotnet-isolated
   Starting JobHost...
   
   EventDriven/OrderProcessingFunction
   EventDriven/ProductSyncFunction
   ...
   
   Listening on http://localhost:7071
   ```

4. **Test HTTP functions:**
   ```bash
   curl http://localhost:7071/api/health
   ```

### Monitoring & Diagnostics

#### Application Insights
Access real-time metrics, logs, and traces:
```bash
# Open in browser
az webapp log tail --resource-group harvesthub-functions-rg --name harvesthub-dev-func
```

#### Query Examples:
```kusto
// Find all failed invocations
functionResults
| where success == false
| project timestamp, name, resultCode, message
| order by timestamp desc

// Function execution duration
customMetrics
| where name == "FunctionDuration"
| summarize Avg = avg(value) by bin(timestamp, 5m)
```

#### Local Logs:
Add to code:
```csharp
_logger.LogInformation("Processing order {OrderId}", orderId);
_logger.LogError(ex, "Failed to process order {OrderId}", orderId);
```

### Next Steps

✅ **Phase 1 Complete:** Infrastructure deployed, local environment ready

📖 **Phase 2:** Implement Event-Driven Functions
- OrderProcessingFunction (Service Bus trigger)
- ProductSyncFunction (Service Bus trigger)
- UserRegistrationFunction (Service Bus trigger)
- OrderNotificationFunction (Service Bus trigger)

See: `EventDriven/_EventDrivenFunctions.md`

### Troubleshooting

**Issue: "AzureWebJobsStorage not found"**
```
Solution: Update local.settings.json with valid storage connection string
```

**Issue: "Service Bus authentication failed"**
```
Solution: Verify ServiceBusConnection is correct (including SharedAccessKey)
```

**Issue: "Cosmos DB connection timeout"**
```
Solution: Check CosmosDbConnection and firewall rules if using Azure-hosted
```

**Issue: "Functions not starting on localhost"**
```
PowerShell: dotnet tool update -g Azure.Functions.Cli
Python: pip install azure-functions
```

### References

- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Bicep Language Reference](https://docs.microsoft.com/azure/azure-resource-manager/bicep/file)
- [Service Bus Triggers](https://docs.microsoft.com/azure/azure-functions/functions-bindings-service-bus)
- [Cosmos DB Bindings](https://docs.microsoft.com/azure/azure-functions/functions-bindings-cosmosdb-v2)
- [Blob Storage Triggers](https://docs.microsoft.com/azure/azure-functions/functions-bindings-storage-blob)
- [Application Insights for Functions](https://docs.microsoft.com/azure/azure-functions/functions-monitoring)

---

**Phase 1 Created:** 2026-03-29
**Status:** ✅ Ready for Phase 2 - Event-Driven Functions
