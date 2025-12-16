using MassTransit;
using Shared.Events;
using Shared.DTOs;
using Shared.HttpClients;
using NotificationService.Services;

namespace NotificationService.Consumers;

public class TicketCommentAddedConsumer : IConsumer<CommentAddedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TicketCommentAddedConsumer> _logger;
    private readonly IUserServiceClient _userClientService;
    private readonly ISignalRNotificationService _signalRService;

    public TicketCommentAddedConsumer(IEmailService emailService, ILogger<TicketCommentAddedConsumer> logger,
    IUserServiceClient userServiceClient, ISignalRNotificationService signalRService)
    {
        _logger = logger;
        _emailService = emailService;
        _userClientService = userServiceClient;
        _signalRService = signalRService;
    }

    public async Task Consume(ConsumeContext<CommentAddedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing CommentAddedEvent {CommentId} for ticket{ticketId} by user {userId}", message.CommentId,message.TicketId,message.UserId);
    
        try
        {
            var authorTask = _userClientService.GetUserAsync(message.UserId);
            var customerTask = _userClientService.GetUserAsync(message.CustomerId);

            await Task.WhenAll(authorTask,customerTask);

            var customer = customerTask.Result;
            var author = authorTask.Result;

            if(author == null)
            {
                _logger.LogWarning("Comment author {userId} not found for ticket {ticketId}",
                message.UserId,
                message.TicketId);
                return;
            }
            if(message.IsInternal)
            {
                _logger.LogInformation("Comment {commentId} is internal - not notifying customer", message.CommentId);
                //TODO: powiadomienie innych agentow zespolu
                return;
            }

            if(isAgent(author.Role) && customer != null)
            {
                await NotifyCustomerAboutAgentComment(message,author,customer);
            }
            else if(isCustomer(author.Role))
            {
                _logger.LogInformation("Customer {CustomerId} added comment to ticket {ticketId}",
                message.CustomerId,message.TicketId);
            }
        }catch(Exception ex)
        {
            _logger.LogError(ex,
            "Error processing CommentAddedEvent for ticket {ticketId}",
            message.TicketId);
        }
    }

    private async Task NotifyCustomerAboutAgentComment(
        CommentAddedEvent message,
        UserDto agent,
        UserDto customer
    )
    {
     var commentPreview = message.Content.Length > 100
            ? message.Content.Substring(0, 100) + "..."
            : message.Content;

        var notification = new CommentAddedNotification
        {
            TicketId = message.TicketId,
            CommentId = message.CommentId,
            UserId = message.UserId,
            UserName = agent.FullName,
            CommentPreview = commentPreview,
            IsInternal = message.IsInternal,
            Message = $"{agent.FullName} odpowiedział na Twój ticket #{message.TicketId}",
            Timestamp = message.Timestamp,
            Priority = "high", 
            ActionUrl = $"/tickets/{message.TicketId}#comment-{message.CommentId}",
            ShowToast = true,
            Metadata = new Dictionary<string, object>
            {
                { "AgentName", agent.FullName },
                { "CommentSnippet", commentPreview }
            }
        };

        await _signalRService.SendNotificationToUser(
            customer.Id.ToString(),
            notification);   

        if(ShouldSendEmailForComment(message.Content))
        {
            // await _emailService.SendNewCommentEmailAsync(customer.Email);
        }
        _logger.LogInformation(
            "Sent notification to customer {customerId} about new comment from agent {agentId}"
            ,customer.Id,agent.Id
        );
    }

    private static bool ShouldSendEmailForComment(string content) => content.Length < 200;

    private static bool isAgent(string role) =>  role.Equals("Agent",StringComparison.OrdinalIgnoreCase) ||
    role.Equals("Administrator",StringComparison.OrdinalIgnoreCase);


    private static bool isCustomer(string role) => role.Equals("Customer",StringComparison.OrdinalIgnoreCase);
    
    
}
