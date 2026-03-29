# Quick Reference Card - Azure Functions Phase 1

## 🚀 Deploy in 3 Commands

```powershell
# 1. Deploy Azure infrastructure
.\scripts\Deploy-Infrastructure.ps1 -SubscriptionId "YOUR_SUBSCRIPTION_ID"

# 2. Setup local environment
.\scripts\Setup-LocalEnvironment.ps1

# 3. Run locally
cd Functions && func start
```

## 📂 Folder Structure

```
Functions/          Executable functions
├─ EventDriven/     Phase 2: Service Bus triggers (4 functions)
├─ Http/            Phase 3: HTTP endpoints (5 functions)
├─ Scheduled/       Phase 4: Timer triggers (5 functions)
├─ DataProcessing/  Phase 5: Blob triggers (4 functions)
└─ Shared/          Services, models, configuration

IaC/                Infrastructure templates
├─ main.bicep       Bicep template with all Azure resources
└─ bicepconfig.json Configuration reference

scripts/            Deployment & setup scripts
├─ Deploy-Infrastructure.ps1  Creates Azure resources
├─ Setup-LocalEnvironment.ps1 Installs tools
└─ Update-LocalSettings.ps1  Configures local settings
```

## 📋 Documentation

| Document | Purpose | Length |
|----------|---------|--------|
| AZURE_FUNCTIONS_GUIDE.md | Complete implementation guide | 1000+ lines |
| PHASE_1_SUMMARY.md | What was created, next steps | This file |
| Functions/README.md | Configuration & troubleshooting | 400+ lines |
| Functions/EventDriven/_EventDrivenFunctions.md | Phase 2 guide | 100+ lines |
| Functions/Http/_HttpFunctions.md | Phase 3 guide | 100+ lines |
| Functions/Scheduled/_ScheduledFunctions.md | Phase 4 guide | 100+ lines |
| Functions/DataProcessing/_DataProcessingFunctions.md | Phase 5 guide | 100+ lines |

## 🔧 Key Services Available in Functions

```csharp
// In any function, inject these services:
ICosmosDbService      // Read/write analytics and audit logs
IBlobStorageService   // Upload/download files, generate SAS URLs
IServiceBusService    // Publish events to Service Bus
ILogger<T>            // Structured logging
IConfiguration        // Access settings
```

## 📊 What Was Created

**Infrastructure**: 6 Azure resources (Storage, Service Bus, Cosmos DB, AppInsights, Function App, App Service Plan)

**Project Code**: 
- Functions.csproj (with 14 NuGet packages)
- 1 DI configuration file
- 3 service implementations (Cosmos, Blob, Service Bus)
- 8 event definitions
- 8 domain models
- Organized folders for 4 function trigger types

**Automation**: 
- Bicep template for complete infrastructure
- 3 PowerShell scripts for deployment & setup

**Documentation**: 
- 2000+ lines of guides and examples
- Phase-by-phase roadmaps

## 🎯 Next Steps

1. **Run the deployment** (5 min):
   ```powershell
   cd scripts
   .\Deploy-Infrastructure.ps1 -SubscriptionId "YOUR_SUBSCRIPTION_ID"
   ```

2. **View deployment outputs** (1 min):
   ```powershell
   type ..\Functions\azure-deployment.json
   ```

3. **Setup local environment** (2 min):
   ```powershell
   .\Setup-LocalEnvironment.ps1
   ```

4. **Build and test** (2 min):
   ```powershell
   cd ..\Functions
   dotnet build
   func start
   ```

5. **Read Phase 2 guide** (10 min):
   Open `Functions/EventDriven/_EventDrivenFunctions.md`

## 🔗 Azure Portal Links

After deployment, find your resources:
- **Resource Group**: harvesthub-functions-rg
- **Function App**: harvesthub-dev-func
- **Service Bus**: harvesthub-dev-sb
- **Cosmos DB**: harvesthub-dev-cosmos
- **Storage Account**: harvesthubxxxxxxxx
- **Application Insights**: harvesthub-dev-ai

Access at: https://portal.azure.com

## ⚡ Common Commands

```bash
# Local testing
func start                          # Start local runtime
func extensions install             # Install function extensions
dotnet build                        # Build Functions project

# Azure deployment
az login                            # Login to Azure
az resource group list              # List resource groups
az storage account list             # List storage accounts

# Monitoring
az monitor metrics list --resource-id /subscriptionId/...
```

## 🆘 Troubleshooting Quick Links

- **Deployment fails**: Check Azure subscription access (needs Owner/Contributor)
- **Local startup fails**: Run `func extensions install`
- **Connection errors**: Verify local.settings.json connection strings
- **Emulator issues**: Ensure Docker is running (if using containers)

For detailed help, see `Functions/README.md`

## 📈 Phases at a Glance

| Phase | Focus | Functions | Status |
|-------|-------|-----------|--------|
| 1 | Foundation & Infrastructure | - | ✅ COMPLETE |
| 2 | Event-Driven Processing | 4 | 📋 READY |
| 3 | HTTP APIs & Webhooks | 5 | 📋 READY |
| 4 | Scheduled Background Jobs | 5 | 📋 READY |
| 5 | File Processing | 4 | 📋 READY |
| 6 | Integration & Production | - | 📋 READY |

**Total Functions to Implement**: 18

---

**Quick Start**: 10 minutes from now you'll have:
- ✅ Azure infrastructure deployed
- ✅ Local environment ready
- ✅ Functions running locally
- ✅ Ready to implement Phase 2

Let's go! 🚀
