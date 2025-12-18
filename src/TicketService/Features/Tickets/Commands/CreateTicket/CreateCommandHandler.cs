using MassTransit;
using MediatR;
using Shared.DTOs;
using Shared.Events;
using Shared.HttpClients;
using Shared.Models;
using TicketService.Data;
using TicketService.Repositories;
using TicketService.Services;



namespace TicketService.Features.Tickets.Commands.CreateTicket;


public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand,TicketDto>
{
    private readonly ITicketRepository _repository;
    private readonly TicketDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserServiceClient _userServiceClient;
    private readonly ILogger<CreateTicketCommandHandler> _logger;

    private readonly IFileStorageService _fileStorageService;

    public CreateTicketCommandHandler(ITicketRepository repository, 
    TicketDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    IUserServiceClient userServiceClient,
    ILogger<CreateTicketCommandHandler> logger,
    IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _userServiceClient = userServiceClient;
        _logger = logger;
        _repository = repository;
        _fileStorageService = fileStorageService;
    }


    public async Task<TicketDto> Handle(CreateTicketCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Handling CreateTicketCommand for user {userId}",command.UserId);

        var priority = Enum.Parse<TicketPriority>(command.Priority,ignoreCase: true);
        var category = Enum.Parse<TicketCategory>(command.Category,ignoreCase: true);


        Guid actualCustomerId = command.UserRole == "Customer" ?
        command.UserId : command.CustomerId!.Value;
    
        Guid? organizationId = command.OrganizationId;
        if(!organizationId.HasValue)
        {
            try
            {
                organizationId = await _userServiceClient.GetUserOrganizationAsync(actualCustomerId);
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch ogranization for customer {CustomerId}",actualCustomerId);
            }
        }
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            var ticket = new Ticket
            {
                CustomerId = actualCustomerId,
                Title = command.Title,
                Description = command.Description,
                Priority = priority,
                Category = category,
                Status = TicketStatus.New,
                OrganizationId = organizationId,
                CreatedAt = DateTime.UtcNow
            };
            var createdTicket = await _repository.CreateAsync(ticket);

            _dbContext.TicketAuditLogs.Add(new TicketAuditLog
            {
                Id = Guid.NewGuid(),
                TicketId = createdTicket.Id,
                UserId = command.UserId,
                Action = AuditAction.Created,
                Description = $"Ticket created: {createdTicket.Title}",
                CreatedAt = DateTime.UtcNow
            });

            await _publishEndpoint.Publish(new TicketCreatedEvent
            {
                TicketId = createdTicket.Id,
                CustomerId = createdTicket.CustomerId,
                Timestamp = DateTime.UtcNow
            },ct);

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Ticket created successfully: {TicketId}",createdTicket.Id);

            return MapToDto(createdTicket);
        }catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }


    private TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto(
            Id: ticket.Id,
            Title: ticket.Title,
            Description: ticket.Description,
            Status: ticket.Status.ToString(),
            Priority: ticket.Priority.ToString(),
            Category: ticket.Category.ToString(),
            CustomerId: ticket.CustomerId,
            AssignedAgentId: ticket.AssignedAgentId,
            OrganizationId: ticket.OrganizationId,
            SlaId: ticket.SlaId,
            CreatedAt: ticket.CreatedAt,
            UpdatedAt: ticket.UpdatedAt,
            ResolvedAt: ticket.ResolvedAt,
            Comments: [.. ticket.Comments.Select(c => new CommentDto(
                Id: c.Id,
                UserId: c.UserId,
                Content: c.Content,
                CreatedAt: c.CreatedAt,
                IsInternal: c.IsInternal
            ))],

            Attachment: [.. ticket.Attachments.Select(a =>
            {   
                //generowanie linku wewnatrz sieci docker
                var internalUrl = _fileStorageService.GetPresignedUrl(a.StoragePath);

                // hack dla hosta: zmiana minio na localhost zeby przegladarka mogla pobrac plik
                // remove in production
                var browserUrl = internalUrl.Replace("minio:9000","localhost:9000");

                return new TicketAttachmentDto(
                    id: a.Id,
                    FileName: a.FileName,
                    ContentType: a.ContentType,
                    FileSizeBytes: a.FileSizeBytes,
                    DownloadUrl: browserUrl,
                    UploadedAt: DateTime.UtcNow
                );

            })]
        );
    }
}