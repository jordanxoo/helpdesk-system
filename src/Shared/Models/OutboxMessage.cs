namespace Shared.Models;

public class OutboxMessage
{
    public Guid Id {get;set;}

    // id encji ktora wygnererowala event, np ticketId userId
    public Guid AggregateId{get;set;}

    public string Type {get;set;} = string.Empty;

    // serialized json payload eventu - wszystkie properties jako json
    public string Payload{get;set;} = string.Empty;

    public DateTime CreatedAt {get;set;}

    public DateTime? ProcessedAt {get;set;}
    public int RetryCount{get;set;}

    public string? Error{get;set;}
}