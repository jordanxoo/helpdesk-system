using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Models;
using TicketService.Data;



namespace TicketService.Services;


public class OutboxService : IOutboxService
{
    private readonly TicketDbContext _dbContext;
    private readonly ILogger<OutboxService> _logger;

    public OutboxService(TicketDbContext dbContext, ILogger<OutboxService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    //zapisujemy event do outbox_message table
    //nie wywoluje savechanges async, robi to caller w transakcji
    public Task SaveEventAsync<TEvent>(TEvent @event, Guid aggregateId) where TEvent : class
    {
        var eventType = @event.GetType().Name;

        var payload = JsonSerializer.Serialize(@event,new JsonSerializerOptions
        {
            // pascal case CreatedAt -> camel case createdAt (JSON)
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
    
    
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateId =  aggregateId,
            Type = eventType,
            Payload = payload,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null,
            RetryCount = 0,
            Error = null
        };
        _dbContext.OutboxMessages.Add(outboxMessage);

        _logger.LogInformation("Saved event {EventType} for {AggregateId} to outbox with ID {OutboxId}",eventType,aggregateId,outboxMessage.Id);
        
        return Task.CompletedTask;
    }
}