using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Shared.DTOs;
using Shared.Events;
using Shared.Exceptions;
using Shared.HttpClients;
using Shared.Models;
using TicketService.Configuration;
using TicketService.Data;
using TicketService.Repositories;
using MassTransit;

namespace TicketService.Services;

public class TicketServiceImpl : ITicketService
{
    private readonly ITicketRepository _repository;
    private readonly TicketDbContext _dbContext;
    //private readonly IMessagePublisher _messagePublisher;

    private readonly IPublishEndpoint _publishEndpoint;    
    private readonly IUserServiceClient _userServiceClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TicketServiceImpl> _logger;
    private readonly IFileStorageService _fileStorageService;

    public TicketServiceImpl(
        ITicketRepository repository,
        TicketDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        IUserServiceClient userServiceClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TicketServiceImpl> logger,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _userServiceClient = userServiceClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }


    /// <summary>
    /// dodawanie attachmentu do ticketu z oblsuga transakcji outbox -> massTransit
    /// </summary>
    
    public async Task<Shared.Models.TicketAttachment> AddAttachmentAsync(Guid ticketID, Guid userId, IFormFile file)
    {
        _logger.LogInformation("Adding attachment to ticket {TicketID} by user {userID}", ticketID, userId);
    
        var ticket = await _repository.GetByIdAsync(ticketID, false)
            ?? throw new NotFoundException("Ticket", ticketID);
    
        var fileID = Guid.NewGuid();
        var fileExtension = Path.GetExtension(file.FileName);
        var storagePath = $"tickets/{ticketID}/{fileID}{fileExtension}";

        await _fileStorageService.UploadFileAsync(file, storagePath);

        Shared.Models.TicketAttachment? attachment = null;

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        
        try
        {
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                
                attachment = new Shared.Models.TicketAttachment
                {
                    Id = fileID,
                    TicketId = ticketID,
                    UploadedById = userId,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    StoragePath = storagePath,
                    UploadedAt = DateTime.UtcNow
                };

                _dbContext.TicketAttachments.Add(attachment);

                await AddAuditLogAsync(ticketID, AuditAction.AttachmentAdded, description: $"File added: {file.FileName}");

                // TODO ticketattachment added event
                // await _publishEndpoint.Publish();

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            });

            attachment!.DownloadUrl = _fileStorageService.GetPresignedUrl(storagePath);
            return attachment;
        }
        catch
        {
            await _fileStorageService.DeleteFileAsync(storagePath);
            throw;
        }
    }    




    /// <summary>
    /// Gets the current user ID from JWT token
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Adds an audit log entry for ticket changes
    /// </summary>
    private async Task AddAuditLogAsync(
        Guid ticketId,
        AuditAction action,
        string? fieldName = null,
        string? oldValue = null,
        string? newValue = null,
        string? description = null)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            _logger.LogWarning("Cannot create audit log - no user ID in context");
            return;
        }

        var auditLog = new TicketAuditLog
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UserId = userId.Value,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.TicketAuditLogs.Add(auditLog);

        _logger.LogDebug("Audit log created: {Action} on ticket {TicketId} by user {UserId}", 
            action, ticketId, userId.Value);
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching ticket with id: {TicketId}", id);
        var ticket = await _repository.GetByIdAsync(id, includeComments: true);
        return ticket != null ? MapToDto(ticket) : null;
    }

    public async Task<TicketListResponse> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Fetching all tickets - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        var tickets = await _repository.GetAllAsync(page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null, null, null, null);

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<TicketListResponse> SearchAsync(TicketFilterRequest filter)
    {
        _logger.LogInformation("Searching tickets with filter: {@Filter}", filter);

        var tickets = await _repository.SearchAsync(
            filter.SearchTerm,
            filter.Status,
            filter.Priority,
            filter.Category,
            filter.CustomerId,
            filter.AssignedAgentId,
            filter.Page,
            filter.PageSize
        );

        var totalCount = await _repository.GetTotalCountAsync(
            searchTerm: filter.SearchTerm,
            status: filter.Status,
            priority: filter.Priority,
            category: filter.Category,
            customerId: filter.CustomerId,
            assignedAgentId: filter.AssignedAgentId
        );

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: filter.Page,
            PageSize: filter.PageSize
        );
    }

    public async Task<TicketListResponse> GetMyTicketsAsync(Guid customerId, int page, int pageSize)
    {
        _logger.LogInformation("Fetching tickets for customer: {CustomerId}", customerId);

        var tickets = await _repository.GetByCustomerIdAsync(customerId, page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null, null, customerId, null);

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<TicketListResponse> GetAssignedTicketsAsync(Guid agentId, int page, int pageSize)
    {
        _logger.LogInformation("Fetching tickets assigned to agent: {AgentId}", agentId);

        var tickets = await _repository.GetByAgentIdAsync(agentId, page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null, null, null, agentId);

        return new TicketListResponse(
            Tickets: tickets.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<TicketDto> CreateAsync(Guid userId, string userRole, CreateTicketRequest request)
    {
        _logger.LogInformation("Creating new ticket - User: {UserId}, Role: {Role}", userId, userRole);


        var priority = Enum.Parse<TicketPriority>(request.Priority,true);
        var category = Enum.Parse<TicketCategory>(request.Category,true);

        // Determine the actual customer ID based on role
        Guid actualCustomerId;
        
        if (userRole == "Customer")
        {
            // Customer creates ticket for themselves
            actualCustomerId = userId;
            
            if (request.CustomerId.HasValue && request.CustomerId.Value != userId)
            {
                throw new BadRequestException("Customers can only create tickets for themselves");
            }
        }
        else if (userRole == "Agent" || userRole == "Administrator")
        {
            // Agent/Admin must provide CustomerId
            if (!request.CustomerId.HasValue)
            {
                throw new BadRequestException("Agents and Administrators must specify CustomerId when creating tickets");
            }
            
            actualCustomerId = request.CustomerId.Value;
            _logger.LogInformation("Agent/Admin {UserId} creating ticket for customer {CustomerId}", 
                userId, actualCustomerId);
        }
        else
        {
            throw new BadRequestException($"Invalid role: {userRole}");
        }

        // Determine OrganizationId
        Guid? organizationId = null;

        if (request.OrganizationId.HasValue)
        {
            // Manual override (usually by Agent/Admin)
            organizationId = request.OrganizationId.Value;
            _logger.LogInformation("Using manually provided OrganizationId: {OrganizationId}", organizationId);
        }
        else
        {
            // Auto-fetch from UserService based on customer
            _logger.LogDebug("Fetching organization for customer {CustomerId}", actualCustomerId);
            try{
                organizationId = await _userServiceClient.GetUserOrganizationAsync(actualCustomerId);
                if (organizationId.HasValue)
                {
                    _logger.LogDebug("Customer {CustomerId} belongs to organization {OrganizationId}", 
                        actualCustomerId, organizationId.Value);
                }
            }catch(Exception ex)
            {
               _logger.LogError(ex, "Failed to fetch organization for customer {CustomerId}. Proceeding without OrganizationId.", actualCustomerId);
                organizationId = null;
            }
           
        }
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var ticket = new Ticket
            {
                CustomerId = actualCustomerId,
                Title = request.Title,
                Description = request.Description,
                Priority = priority,
                Category = category,
                Status = TicketStatus.New,
                OrganizationId = organizationId
            };

            var createdTicket = await _repository.CreateAsync(ticket);
            _logger.LogInformation("Ticket created successfully: {TicketId} for Customer: {CustomerId} with Organization: {OrganizationId}", 
                createdTicket.Id, actualCustomerId, organizationId?.ToString() ?? "none");

            // Audit log
            await AddAuditLogAsync(
                createdTicket.Id,
                AuditAction.Created,
                description: $"Ticket created: {createdTicket.Title}");

            await _publishEndpoint.Publish(new TicketCreatedEvent
            {
                TicketId = createdTicket.Id,
                CustomerId = createdTicket.CustomerId,
                Timestamp = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(createdTicket);
        }catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TicketDto> UpdateAsync(Guid id, UpdateTicketRequest request)
    {
        _logger.LogInformation("Updating ticket: {TicketId}", id);

        var ticket = await _repository.GetByIdAsync(id, includeComments: false);
        if (ticket == null)
        {
            throw new NotFoundException("Ticket", id);
        }

        // Store old values for audit
        var oldTitle = ticket.Title;
        var oldDescription = ticket.Description;
        var oldStatus = ticket.Status.ToString();
        var oldPriority = ticket.Priority.ToString();
        var oldAgentId = ticket.AssignedAgentId?.ToString();
        
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if(!string.IsNullOrWhiteSpace(request.Title)) ticket.Title = request.Title;
            if(!string.IsNullOrWhiteSpace(request.Description)) ticket.Description = request.Description;

            if(!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<TicketStatus>(request.Status,out var status))
            {
                if(ticket.Status != status)
                {
                    ticket.Status = status;

                    await AddAuditLogAsync(id,AuditAction.StatusChanged,"Status",oldStatus, status.ToString());

                    await _publishEndpoint.Publish(new TicketStatusChangedEvent
                    {
                        TicketId = ticket.Id,
                        OldStatus = oldStatus,
                        NewStatus = status.ToString(),
                        CustomerId = ticket.CustomerId,
                        AgentId = ticket.AssignedAgentId,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            if(!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<TicketPriority>(request.Priority, out var priority))
            {
                if(ticket.Priority != priority)
                {
                    ticket.Priority = priority;

                    await AddAuditLogAsync(id,AuditAction.PriorityChanged,"Priority",oldPriority,priority.ToString());

                }
            }
            if(request.AssignedAgentId.HasValue)
            {
                if(ticket.AssignedAgentId != request.AssignedAgentId)
                {
                    ticket.AssignedAgentId = request.AssignedAgentId;

                    await AddAuditLogAsync(id, AuditAction.Assigned, "AssignedAgentId",oldAgentId,ticket.AssignedAgentId.ToString());
                }
            }

            var updatedTicket = await _repository.UpdateAsync(ticket);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(updatedTicket);
        }catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TicketDto> AssignToAgentAsync(Guid ticketId, Guid agentId)
    {
        _logger.LogInformation("Assigning ticket {TicketId} to agent {AgentId}", ticketId, agentId);

        var ticket = await _repository.GetByIdAsync(ticketId, includeComments: false);
        if (ticket == null)
        {
            throw new NotFoundException("Ticket", ticketId);
        }
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var oldAgentId = ticket.AssignedAgentId?.ToString();
            ticket.AssignedAgentId = agentId;
            ticket.Status = TicketStatus.Open;

            await _repository.UpdateAsync(ticket);

            _logger.LogInformation("Ticket assigned successfully");

            // Audit log
            await AddAuditLogAsync(
                ticketId,
                AuditAction.Assigned,
                fieldName: "AssignedAgentId",
                oldValue: oldAgentId,
                newValue: agentId.ToString());

            await _publishEndpoint.Publish(new TicketAssignedEvent
            {
                TicketId = ticketId,
                AgentId  = agentId,
                CustomerId = ticket.CustomerId,
                Timestamp = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return MapToDto(ticket);
        }catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<TicketDto> ChangeStatusAsync(Guid ticketId, string newStatus)
    {
        _logger.LogInformation("Changing ticket {TicketId} status to {NewStatus}", ticketId, newStatus);

        var ticket = await _repository.GetByIdAsync(ticketId, false);
        if (ticket == null) throw new NotFoundException("Ticket", ticketId);

        if (!Enum.TryParse<TicketStatus>(newStatus, out var status))
        {
            throw new BadRequestException($"Invalid status: {newStatus}");
        }
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var oldStatus = ticket.Status.ToString();
            ticket.Status = status;

            await _repository.UpdateAsync(ticket);
            _logger.LogInformation("Ticket status changed successfully");

            // Audit log
            await AddAuditLogAsync(
                ticketId,
                AuditAction.StatusChanged,
                fieldName: "Status",
                oldValue: oldStatus,
                newValue: status.ToString());

            await _publishEndpoint.Publish(new TicketStatusChangedEvent
            {
                TicketId = ticketId,
                OldStatus = oldStatus,
                NewStatus = status.ToString(),
                CustomerId = ticket.CustomerId,
                AgentId = ticket.AssignedAgentId,
                Timestamp = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return MapToDto(ticket);
        }catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting ticket: {TicketId}", id);

        var ticket = await _repository.GetByIdAsync(id, includeComments: false);
        if (ticket == null)
        {
            throw new NotFoundException("Ticket", id);
        }

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Ticket deleted successfully: {TicketId}", id);
    }

    public async Task<TicketDto> AddCommentAsync(Guid ticketId, Guid userId, AddCommentRequest request)
    {
        _logger.LogInformation("Adding comment to ticket {TicketId} by user {UserId}", ticketId, userId);

        var ticket = await _repository.GetByIdAsync(ticketId, includeComments: false);
        if (ticket == null)
        {
            throw new NotFoundException("Ticket", ticketId);
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Content = request.Content,
                IsInternal = request.IsInternal
            };

            await _repository.AddCommentAsync(comment);
            _logger.LogInformation("Comment added successfully");

            // Audit log
            await AddAuditLogAsync(
                ticketId,
                AuditAction.CommentAdded,
                description: request.IsInternal ? "Internal comment added" : "Comment added");

            await _publishEndpoint.Publish(new CommentAddedEvent
            {
                TicketId =ticketId,
                CommentId = comment.Id,
                UserId = comment.UserId,
                Content = comment.Content,
                CustomerId = ticket.CustomerId,
                IsInternal = comment.IsInternal,
                Timestamp =DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            var updatedTicket = await _repository.GetByIdAsync(ticketId,true);

            return MapToDto(updatedTicket!);
        }catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<TicketAuditLogDto>> GetHistoryAsync(Guid ticketId)
    {
        _logger.LogInformation("Fetching history for ticket: {TicketId}", ticketId);

        var ticket = await _repository.GetByIdAsync(ticketId, includeComments: false);
        if (ticket == null)
        {
            throw new NotFoundException("Ticket", ticketId);
        }

        var auditLogs = await _dbContext.TicketAuditLogs
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new TicketAuditLogDto(
                a.Id,
                a.UserId,
                a.Action.ToString(),
                a.FieldName,
                a.OldValue,
                a.NewValue,
                a.Description,
                a.CreatedAt
            ))
            .ToListAsync();

        return auditLogs;
    }

    // Private helper methods

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

    public async Task<TicketStatisticsDto> GetStatisticsAsync()
    {
        var byStatus = await _repository.GetCountByStatusAsync();
        var byPriority = await _repository.GetCountByPriorityAsync();
        var total = byStatus.Values.Sum();
        var unassigned = await _repository.GetUnassignedCountAsync();

        return new TicketStatisticsDto(
            ByStatus: byStatus,
            ByPriority: byPriority,
            Total: total,
            Unassigned: unassigned
        );
    }
}
    