namespace Shared.Events;


public class TicketSlaBreachedEvent
{
    public Guid TicketId {get;set;}
    public string Title{get;set;} = string.Empty;
    public Guid CustomerId {get;set;}
    public Guid? AgentId {get;set;}
    public DateTime CreatedAt{get;set;}
    public int HoursOverdue{get;set;}
    public DateTime Timestamp{get;set;}
}