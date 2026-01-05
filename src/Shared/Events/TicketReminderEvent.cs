namespace Shared.Events;

public class TicketReminderEvent
{
    public Guid TicketId{get;set;}
    public string Title{get;set;} = string.Empty;
    public Guid CustomerId{get;set;}
    public Guid? AgentId {get;set;}
    public int HoursSinceCreated{get;set;}
    public DateTime Timestamp{get;set;}

    
}