namespace Shared.DTOs;


public record NotificationDto
{
    public string Type {get;init;} = string.Empty;
    public string Message {get;init;} = string.Empty;
    public DateTime Timestamp {get;init;}
    public string? Icon {get;init;}
    public string Priority {get;init;} = "normal";
    public string? ActionUrl {get;init;}
    public bool ShowToast{get;init;} = true;
    public Dictionary<string,object>? Metadata {get;init;}
}

public record TicketCreatedNotification: NotificationDto
{
    public Guid TicketId {get;init;}
    public Guid CustomerId {get;init;}
    public string? CustomerName {get;init;}
    public string? Title {get;init;}
    
    public TicketCreatedNotification()
    {
        Type = "TicketCreated";
        Icon = "Ticket";
    }
}

public record TicketStatusChangedNotification : NotificationDto
{
    public Guid TicketId {get;init;}
    public string OldStatus{get;init;} = string.Empty;
    public string NewStatus{get;init;} = string.Empty;
    public Guid? AgentId {get;init;}
    public string? AgentName{get;init;}

    public TicketStatusChangedNotification()
    {
        Type = "TicketStatusChanged";
        Icon = "status-change";
    }
}

public record TicketAssignedNotification : NotificationDto
{
    public Guid TicketId{get;init;}
    public Guid AgentId{get;init;}
    public string? AgentName{get;init;}
    public Guid CustomerId{get;init;} 

    public TicketAssignedNotification()
    {
        Type = "TicketAssigned";
        Icon = "user-check";
    }
}

public record CommentAddedNotification : NotificationDto
{
    public Guid TicketId{get;init;}
    public Guid CommentId{get;init;}
    public Guid UserId{get;init;}
    public string? UserName{get;init;}
    public string? CommentPreview {get;init;}
    public bool IsInternal{get;init;}

    public CommentAddedNotification()
    {
        Type = "commentAdded";
        Icon = "message";
    }
}