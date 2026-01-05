using System.Text.Json;
using Hangfire.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using TicketService.Data;


namespace TicketService.Services;


public interface IOutboxProcessorService
{
    Task ProcessPendingMessageAsync();
}


public class OutboxProcessorService : IOutboxProcessorService
{
    private readonly TicketDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<OutboxProcessorService> _logger;
    private const int MaxRetryCount = 5;
    private const int BatchSize = 100;
    public OutboxProcessorService(TicketDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<OutboxProcessorService> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task ProcessPendingMessageAsync()
    {
        var pendingMessages = await _dbContext.OutboxMessages.Where(t => t.ProcessedAt == null && t.RetryCount < MaxRetryCount)
        .OrderBy(t => t.CreatedAt)
        .Take(BatchSize)
        .ToListAsync();

        if(!pendingMessages.Any())
        {
            _logger.LogDebug("No pending outbox messages to process");
            return;
        }   
        _logger.LogInformation("Processing {Count} pending outbox messages", pendingMessages.Count);

        foreach(var message in pendingMessages)
        {
            try
            { 
               var eventObject = DeserializeEvent(message.Type,message.Payload);

                if(eventObject == null)
                {
                    _logger.LogWarning("Failed to deserialize event type {EventType} for outbox message {MessageId}",message.Type,message.Id);
                    
                    message.Error = $"Unknown event type: {message.Type}";
                    message.RetryCount++;
                    continue; 
                }
                await _publishEndpoint.Publish(eventObject);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
                
                _logger.LogInformation("Sucessfully processed outbox message {MessageId} of type {messageType}",message.Id, message.Type);
            }catch(Exception ex)
            {
                message.RetryCount ++;
                message.Error = ex.Message;
                _logger.LogWarning(ex,"Failed to process outbox message {messageId} of type {EventType}. Retry Count {RetryCount}",
                message.Id,message.Type,message.RetryCount);
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Finished processing outbox batch.Processed: {Processed}, Failed: {Failed}",
        pendingMessages.Count(p => p.ProcessedAt !=null),
        pendingMessages.Count(p =>p.ProcessedAt == null));
    }

    private object? DeserializeEvent(string eventType,string payload)
    {
        var type = eventType switch
        {
            nameof(TicketCreatedEvent) => typeof(TicketCreatedEvent),
            nameof(TicketAssignedEvent) => typeof(TicketAssignedEvent),
            nameof(TicketStatusChangedEvent) => typeof(TicketStatusChangedEvent),
            nameof(TicketClosedEvent) => typeof(TicketClosedEvent),
            nameof(CommentAddedEvent) => typeof(CommentAddedEvent),
            nameof(TicketSlaBreachedEvent) => typeof(TicketSlaBreachedEvent),
            nameof(TicketReminderEvent) => typeof(TicketReminderEvent),
            _ => null // Unknown event type
        };

        if (type == null)
            return null;

        return JsonSerializer.Deserialize(payload,type,new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    
}