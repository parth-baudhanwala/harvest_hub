var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(builder.Configuration);

// Add HarvestHub Functions services
builder.Services.AddFunctionServices(builder.Configuration);

// HTTP Client with resilience policies
builder.Services.AddHttpClient("DefaultClient")
    .AddStandardResilienceHandler();

builder.Build().Run();
