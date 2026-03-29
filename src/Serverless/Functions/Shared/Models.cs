using System.ComponentModel.DataAnnotations;

namespace Functions.Shared;

/// <summary>
/// Base domain event for auditing and event sourcing
/// </summary>
public class DomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = default!;
    public string AggregateId { get; set; } = default!;
    public string AggregateType { get; set; } = default!;
    public Dictionary<string, object> Data { get; set; } = new();
    public string Source { get; set; } = "AzureFunctions";
    public int SequenceNumber { get; set; }
}

/// <summary>
/// Analytics event for Cosmos DB time-series tracking
/// </summary>
public class AnalyticsEvent
{
    [System.ComponentModel.DataAnnotations.Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = default!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = default!;
    public string OrderId { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Order notification tracking
/// </summary>
public class NotificationRecord
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = default!;
    public string RecipientEmail { get; set; } = default!;
    public string NotificationType { get; set; } = default!; // OrderCreated, PaymentConfirmed, Shipped, etc.
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Bounced
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// File processing metadata
/// </summary>
public class ProcessedFile
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OriginalBlobName { get; set; } = default!;
    public string ProcessedBlobName { get; set; } = default!;
    public string FileType { get; set; } = default!; // Image, Invoice, Report, etc.
    public string Status { get; set; } = "Processing"; // Processing, Completed, Failed
    public long OriginalSize { get; set; }
    public long ProcessedSize { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string ProcessingFunction { get; set; } = default!;
}

/// <summary>
/// Health check result
/// </summary>
public class HealthCheckResult
{
    public string Status { get; set; } = "Healthy"; // Healthy, Degraded, Unhealthy
    public Dictionary<string, ServiceHealth> Services { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

public class ServiceHealth
{
    public string Name { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? Message { get; set; }
    public long ResponseTimeMs { get; set; }
}

/// <summary>
/// Webhook payload for external service callbacks
/// </summary>
public class WebhookPayload
{
    public string WebhookId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = default!;
    public string ResourceId { get; set; } = default!;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = default!;
}

/// <summary>
/// Batch operation job
/// </summary>
public class BatchJob
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobType { get; set; } = default!; // Export, Import, ProcessImages, etc.
    public string Status { get; set; } = "Queued"; // Queued, Running, Completed, Failed
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public string? OutputBlobUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorLog { get; set; }
}
