namespace Shared.Models;

public class Ticket
{
    // TODO: Implementacja modelu ticketu
}

public class TicketComment
{
    // TODO: Implementacja komentarza do ticketu
}

public enum TicketStatus
{
    New,
    Open,
    InProgress,
    Pending,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketCategory
{
    Hardware,
    Software,
    Network,
    Security,
    Account,
    Other
}
