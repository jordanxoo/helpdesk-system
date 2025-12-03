namespace Shared.Constants;

/// <summary>
/// Centralized routing keys for RabbitMQ events.
/// Used by both publishers (TicketService) and consumers (NotificationService).
/// </summary>
public static class RoutingKeys
{
    /// <summary>
    /// Event published when a new ticket is created.
    /// Publisher: TicketService
    /// Consumers: NotificationService
    /// </summary>
    public const string TicketCreated = "ticket-created";

    /// <summary>
    /// Event published when a ticket is assigned to an agent.
    /// Publisher: TicketService
    /// Consumers: NotificationService
    /// </summary>
    public const string TicketAssigned = "ticket-assigned";

    /// <summary>
    /// Event published when a ticket status changes.
    /// Publisher: TicketService
    /// Consumers: NotificationService
    /// </summary>
    public const string TicketStatusChanged = "ticket-status-changed";

    /// <summary>
    /// Event published when a comment is added to a ticket.
    /// Publisher: TicketService
    /// Consumers: NotificationService
    /// </summary>
    public const string CommentAdded = "comment-added";
    
    /// <summary>
    /// Event published when a new user registers.
    /// Publisher: AuthService
    /// Consumers: UserService
    /// Contains full profile data - UserService creates user record.
    /// </summary>
    public const string UserRegistered = "user.registered";

    public const string UserLoggedIn = "user.loggedin";
}
