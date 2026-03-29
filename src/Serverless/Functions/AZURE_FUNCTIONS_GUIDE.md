# HarvestHub Azure Functions - Complete Implementation Guide

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Phase 1: Foundation (✅ COMPLETE)](#phase-1-foundation--complete)
4. [Phase 2: Event-Driven Functions](#phase-2-event-driven-functions)
5. [Phase 3: HTTP-Triggered Functions](#phase-3-http-triggered-functions)
6. [Phase 4: Scheduled Functions](#phase-4-scheduled-functions)
7. [Phase 5: Data Processing Functions](#phase-5-data-processing-functions)
8. [Phase 6: Integration & Deployment](#phase-6-integration--deployment)
9. [Quick Start](#quick-start)

## Overview

This guide walks you through implementing Azure Functions for HarvestHub microservices project. The implementation covers all major trigger types and production patterns:

- ✅ **Event-driven** functions for business event processing
- ✅ **HTTP-triggered** functions for API extensions and webhooks
- ✅ **Timer-triggered** functions for scheduled background jobs
- ✅ **Blob Storage** triggers for file processing
- ✅ **Managed Identities** for secure Azure service authentication
- ✅ **Application Insights** for comprehensive monitoring
- ✅ **Cosmos DB** for analytics and audit trails

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Functions                          │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Event Triggers      HTTP Triggers      Timer Triggers │ │
│  │ ├─ Order Events     ├─ Webhooks        ├─ 6am Report  │ │
│  │ ├─ Product Events   ├─ Admin API       ├─ Sync Jobs  │ │
│  │ ├─ User Events      ├─ Health Check    ├─ Cleanup    │ │
│  │ └─ Payment Events   └─ Analytics       └─ Maintenance│ │
│  └────────────────────────────────────────────────────────┘ │
│           ⬇️              ⬇️              ⬇️              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Shared Services Layer                                 │ │
│  │ ├─ ICosmosDbService (Analytics, Audit)               │ │
│  │ ├─ IBlobStorageService (File Operations)             │ │
│  │ ├─ IServiceBusService (Message Publishing)           │ │
│  │ └─ ILogger (Structured Logging)                      │ │
│  └────────────────────────────────────────────────────────┘ │
│           ⬇️              ⬇️              ⬇️              │
└─────────────────────────────────────────────────────────────┘
        ⬇️              ⬇️              ⬇️              ⬇️
    Service Bus   Blob Storage   Cosmos DB   SQL/Postgres
    (Messaging)   (Files)        (Analytics)  (Existing DBs)
```

## Phase 1: Foundation ✅ COMPLETE

### What Was Set Up

1. ✅ **Azure Infrastructure** via Bicep IaC
   - Storage Account (5 blob containers)
   - Service Bus Namespace (4 topics with subscriptions)
   - Cosmos DB (serverless with 2 containers)
   - Application Insights
   - Function App (host)
   - Managed Identities & RBAC

2. ✅ **Project Structure**
   - Organized into trigger-type folders
   - Shared services for common operations
   - DI configuration for dependency injection
   - Host.json and local.settings.json templates

3. ✅ **Local Development Setup**
   - Scripts to deploy infrastructure
   - Scripts to configure local environment
   - Docker support for emulators

4. ✅ **Documentation**
   - README with troubleshooting
   - Deployment guides
   - Configuration examples

### Files Created

```
√ Functions/
  √ Functions.csproj - Project file with dependencies
  √ Program.cs - Startup configuration
  √ host.json - Runtime settings  
  √ local.settings.json - Dev environment variables
  √ Shared/
    √ GlobalUsings.cs
    √ Models.cs - Domain models
    √ Services.cs - Core services (Cosmos, Blob, ServiceBus)
    √ Events.cs - Event definitions
    √ DependencyInjection.cs - DI setup
  √ EventDriven/ - Placeholder for Phase 2
  √ Http/ - Placeholder for Phase 3
  √ Scheduled/ - Placeholder for Phase 4
  √ DataProcessing/ - Placeholder for Phase 5
  √ README.md - Phase 1 documentation

√ IaC/
  √ main.bicep - Complete infrastructure template
  √ bicepconfig.json - Configuration reference

√ scripts/
  √ Deploy-Infrastructure.ps1 - One-command Azure deployment
  √ Setup-LocalEnvironment.ps1 - Local tools setup
  √ Update-LocalSettings.ps1 - Connection string configuration
```

### Immediate Next Steps (Do This First)

1. **Deploy Azure Infrastructure:**
   ```powershell
   cd scripts
   .\Deploy-Infrastructure.ps1 -SubscriptionId "YOUR_SUB_ID" -Location "eastus"
   ```

2. **Verify Resources Created:**
   ```bash
   az resource group show -n harvesthub-functions-rg
   ```

3. **Setup Local Development:**
   ```powershell
   .\Setup-LocalEnvironment.ps1
   ```

4. **Build and Test Locally:**
   ```powershell
   cd ..\Functions
   dotnet build
   func start  # This will fail until Phase 2 functions are added - that's expected
   ```

---

## Phase 2: Event-Driven Functions

**Goal**: Consume business events from Service Bus and implement distributed processing

### Functions to Implement

#### 1️⃣ Order Processing Function
- **Trigger**: `BasketCheckoutEvent` on `orders` topic
- **Logic**: 
  - Create order record in SQL Server
  - Reserve inventory
  - Write audit entry to Cosmos DB
  - Send order created event
- **Output Bindings**: Email notification

**Code Location**: `Functions/EventDriven/OrderProcessingFunction.cs`

**Implementation Example**:
```csharp
[Function("OrderProcessingFunction")]
public async Task Run(
    [ServiceBusTrigger("orders", "functions-orders")] BasketCheckoutEvent @event,
    ICosmosDbService cosmosDb,
    IServiceBusService serviceBus,
    ILogger<OrderProcessingFunction> logger)
{
    logger.LogInformation("Processing checkout for {Email}", @event.EmailAddress);
    
    // 1. Create order
    var order = new Order 
    { 
        CustomerId = @event.CustomerId,
        TotalPrice = @event.TotalPrice,
        Status = "Pending"
    };
    // ... save to database
    
    // 2. Write audit to Cosmos DB
    var auditEvent = new DomainEvent 
    { 
        EventType = "OrderCreated",
        AggregateId = order.Id.ToString(),
        Data = JObject.FromObject(order)
    };
    await cosmosDb.CreateDocumentAsync(auditEvent, "events");
    
    // 3. Publish event
    var createdEvent = new OrderCreatedEvent 
    { 
        OrderId = order.Id,
        CustomerId = @event.CustomerId
    };
    await serviceBus.PublishEventAsync(createdEvent, "orders");
    
    logger.LogInformation("Order {OrderId} created successfully", order.Id);
}
```

#### 2️⃣ Product Sync Function
- **Trigger**: `ProductUpsertedEvent` on `products` topic
- **Logic**: Invalidate Redis cache, update search index, sync to Cosmos
- **Output Bindings**: Logging only

#### 3️⃣ User Registration Function
- **Trigger**: `UserRegisteredEvent` on `users` topic
- **Logic**: Send welcome email, initialize customer profile
- **Output Bindings**: Email notification

#### 4️⃣ Order Notification Function
- **Trigger**: `OrderCreatedEvent` on `orders` topic
- **Logic**: Send confirmation email, SMS if opted-in
- **Output Bindings**: Email/SMS service

### Implementation Checklist

- [ ] Create `OrderProcessingFunction.cs` with Service Bus trigger
- [ ] Create `ProductSyncFunction.cs` with Service Bus trigger
- [ ] Create `UserRegistrationFunction.cs` with Service Bus trigger
- [ ] Create `OrderNotificationFunction.cs` with Service Bus trigger
- [ ] Add unit tests for each function
- [ ] Test locally with Azure Storage Emulator + Service Bus
- [ ] Deploy to Azure and test with real services
- [ ] Monitor in Application Insights

### Testing Phase 2

**Local Testing**:
```powershell
# Publish test event to local Service Bus
$message = @{
    Username = "testuser"
    CustomerId = "12345"
    EmailAddress = "test@example.com"
    TotalPrice = 99.99
} | ConvertTo-Json

# Use Service Bus Explorer or write test code
```

---

## Phase 3: HTTP-Triggered Functions

**Goal**: Extend API capabilities with new endpoints for webhooks, analytics, and admin operations

### Functions to Implement

#### 1️⃣ Order Status Webhook
- **Endpoint**: `POST /api/webhooks/order-status`
- **Purpose**: Receive status updates from external fulfillment services
- **Security**: API Key authentication + CORS

#### 2️⃣ Notification Status Callback
- **Endpoint**: `POST /api/webhooks/delivery-status`
- **Purpose**: Track email/SMS delivery status
- **Storage**: Update notification records in Cosmos DB

#### 3️⃣ Batch Export Job
- **Endpoint**: `POST /api/reports/generate`
- **Purpose**: Asynchronous CSV export of orders/sales
- **Storage**: Save to Blob Storage with SAS URL for download

#### 4️⃣ Admin Analytics API
- **Endpoint**: `GET /api/admin/analytics/summary`
- **Purpose**: Read-only aggregated metrics for dashboard
- **Query**: Cosmos DB analytics container

#### 5️⃣ Health Check Endpoint
- **Endpoint**: `GET /api/health`
- **Purpose**: Service health diagnostics
- **Checks**: Database connectivity, Service Bus, Storage

### Implementation Example

```csharp
[Function("OrderStatusWebhook")]
public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "webhooks/order-status")] HttpRequestData req,
    ICosmosDbService cosmosDb,
    IServiceBusService serviceBus,
    ILogger<OrderStatusWebhook> logger)
{
    try
    {
        var payload = await req.GetFromJsonAsync<WebhookPayload>();
        if (payload == null) 
            return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);

        logger.LogInformation("Received webhook for order {OrderId}", payload.ResourceId);

        // Validate webhook signature
        var signature = req.Headers.GetValues("X-Webhook-Signature").FirstOrDefault();
        if (!ValidateSignature(signature, payload))
            return req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);

        // Update order status
        await cosmosDb.UpdateDocumentAsync(payload, "events");
        
        // Emit event
        await serviceBus.PublishEventAsync(
            new OrderUpdatedEvent { OrderId = Guid.Parse(payload.ResourceId) },
            "orders");

        var response = req.CreateResponse(System.Net.HttpStatusCode.Accepted);
        await response.WriteAsJsonAsync(new { success = true, webhookId = payload.WebhookId });
        return response;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Webhook processing failed");
        return req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
    }
}
```

---

## Phase 4: Scheduled Functions

**Goal**: Implement background jobs for maintenance, reporting, and reconciliation

### Functions to Implement

#### 1️⃣ Daily Inventory Sync
- **Schedule**: `0 0 * * * UTC` (midnight daily)
- **Logic**: Poll Catalog service, sync to Cosmos DB

#### 2️⃣ Abandoned Cart Cleanup
- **Schedule**: `0 2 * * * UTC` (2 AM daily)
- **Logic**: Find carts inactive > 24h, send reminders, archive

#### 3️⃣ Order Status Reconciliation
- **Schedule**: `0 */6 * * * UTC` (every 6 hours)
- **Logic**: Find stuck orders, escalate to support

#### 4️⃣ Daily Sales Report
- **Schedule**: `0 6 * * * UTC` (6 AM daily)
- **Logic**: Generate PDF, email to admin, archive to Blob Storage

#### 5️⃣ Cosmos DB Maintenance
- **Schedule**: `0 3 * * 0 UTC` (3 AM Sundays)
- **Logic**: Archive old events, cleanup expired documents

### Implementation Example

```csharp
[Function("DailySalesReportFunction")]
public async Task Run(
    [TimerTrigger("0 6 * * * UTC")] TimerInfo myTimer,
    ICosmosDbService cosmosDb,
    IBlobStorageService blobStorage,
    ILogger<DailySalesReportFunction> logger)
{
    logger.LogInformation("Daily sales report generation started");

    // Query analytics from Cosmos DB
    var query = "SELECT * FROM c WHERE c.eventType = 'OrderCreated' AND c.timestamp > @yesterday";
    var analytics = await cosmosDb.QueryAsync<AnalyticsEvent>(query, "analytics");

    // Generate report
    var reportData = analytics
        .GroupBy(a => a.EventType)
        .Select(g => new { Type = g.Key, Count = g.Count(), Total = g.Sum(a => a.TotalAmount) });

    var csvContent = GenerateCsv(reportData);

    // Save to Blob Storage
    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent)))
    {
        var blobName = $"reports/daily-sales-{DateTime.UtcNow:yyyyMMdd}.csv";
        await blobStorage.UploadBlobAsync(stream, blobName, "reports");
        
        // Get SAS URI
        var sasUri = await blobStorage.GetBlobSasUriAsync(blobName, "reports", TimeSpan.FromDays(7));
        
        // Email to admin (next: integrate SendGrid)
        logger.LogInformation("Report generated and saved to: {SasUri}", sasUri);
    }

    logger.LogInformation("Daily sales report completed at {DateTime}", myTimer.ScheduleStatus.Last);
}
```

---

## Phase 5: Data Processing Functions

**Goal**: Handle file operations and batch data processing

### Functions to Implement

#### 1️⃣ Product Image Processing
- **Trigger**: Blob Storage `product-images/` container
- **Logic**: Resize image, generate thumbnails, optimize for web
- **Output**: Save to `product-images-processed/`

#### 2️⃣ Invoice Generation
- **Trigger**: HTTP endpoint or order completion event
- **Logic**: Create PDF invoice from order data
- **Output**: Save to `invoices/` Blob container

#### 3️⃣ Batch CSV Import
- **Trigger**: HTTP endpoint for file upload
- **Logic**: Parse CSV, validate rows, queue individual processing

#### 4️⃣ Audit Log Archival
- **Trigger**: Timer (monthly)
- **Logic**: Export old events from Cosmos DB, compress, move to cold storage

### Implementation Example

```csharp
[Function("ProductImageProcessingFunction")]
public async Task Run(
    [BlobTrigger("product-images/{name}")] Stream image,
    string name,
    IBlobStorageService blobStorage,
    ILogger<ProductImageProcessingFunction> logger)
{
    try
    {
        logger.LogInformation("Processing image: {ImageName}", name);

        // Resize image (thumbnail: 200x200, web: 800x600)
        using (var thumbnail = ResizeImage(image, 200, 200))
        {
            await blobStorage.UploadBlobAsync(
                thumbnail, 
                $"thumbnails/{Path.GetFileNameWithoutExtension(name)}-thumb.jpg",
                "product-images-processed");
        }

        using (var webOptimized = ResizeImage(image, 800, 600))
        {
            await blobStorage.UploadBlobAsync(
                webOptimized,
                $"web/{name}",
                "product-images-processed");
        }

        logger.LogInformation("Image {ImageName} processed successfully", name);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Image processing failed for {ImageName}", name);
        throw;
    }
}

private Stream ResizeImage(Stream source, int width, int height)
{
    // Use ImageSharp or similar library
    // Implementation details ...
    return new MemoryStream();
}
```

---

## Phase 6: Integration & Deployment

**Goal**: Production-ready setup with monitoring, testing, and CI/CD

### Tasks

- [ ] Integration with existing microservices
- [ ] Error handling and dead-letter queues  
- [ ] Performance testing and optimization
- [ ] Cost analysis
- [ ] CI/CD pipeline setup
- [ ] Blue-green deployment configuration
- [ ] Scaling policies
- [ ] Security hardening

---

## Quick Start

### 1. Deploy Infrastructure (5 min)

```powershell
# Clone repo and navigate to scripts
cd scripts

# Deploy to Azure
.\Deploy-Infrastructure.ps1 `
    -SubscriptionId "YOUR_SUBSCRIPTION_ID" `
    -Location "eastus" `
    -Environment "dev"
```

### 2. Setup Local Environment (3 min)

```powershell
.\Setup-LocalEnvironment.ps1
```

### 3. Run Locally (1 min)

```powershell
cd ..\Functions
func start
```

### 4. Proceed to Phase 2

Follow Phase 2 implementation guide above.

---

## File Reference Quick Link

| Phase | Files | Status |
|-------|-------|--------|
| 1 | `Functions/` `IaC/` `scripts/` | ✅ Complete |
| 2 | `Functions/EventDriven/` | 📋 Ready to implement |
| 3 | `Functions/Http/` | 📋 Ready to implement |
| 4 | `Functions/Scheduled/` | 📋 Ready to implement |
| 5 | `Functions/DataProcessing/` | 📋 Ready to implement |
| 6 | `Functions/` + integration | 📋 Ready |

---

## Learning Resources

- [Microsoft Learn: Azure Functions](https://learn.microsoft.com/training/paths/create-serverless-applications/)
- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [Bicep Learning Path](https://learn.microsoft.com/training/paths/fundamentals-bicep/)
- [Cosmos DB Best Practices](https://learn.microsoft.com/azure/cosmos-db/best-practices)
- [Application Insights for Functions](https://learn.microsoft.com/azure/azure-functions/functions-monitoring)

---

**Last Updated**: 2026-03-29
**Phase 1 Status**: ✅ COMPLETE - Ready for Phase 2
