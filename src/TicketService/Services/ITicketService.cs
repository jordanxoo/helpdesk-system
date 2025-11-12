using Shared.DTOs;

namespace TicketService.Services;

public interface ITicketService
{
    // implementacja logiki biznesowej z publikowaniem eventow

    Task<TicketDto?> GetByIdAsync(Guid id);
    Task<TicketListResponse> GetAllAsync(int page, int pageSize);
    Task<TicketListResponse> SearchAsync(TicketFilterRequest filter);
    Task<TicketListResponse> GetMyTicketsAsync(Guid customerId, int page, int pageSize);
    Task<TicketListResponse> GetAssignedTicketsAsync(Guid agentId, int page, int pageSize);
    Task<TicketDto> CreateAsync(Guid userId, string userRole, CreateTicketRequest request);
    Task<TicketDto> UpdateAsync(Guid id, UpdateTicketRequest request);
    Task<TicketDto> AssignToAgentAsync(Guid ticketId, Guid agentId);
    Task<TicketDto> ChangeStatusAsync(Guid ticketId, string newStatus);
    Task DeleteAsync(Guid id);

    //operacje na komentarzach
    Task<TicketDto> AddCommentAsync(Guid ticketId, Guid userId, AddCommentRequest request);
    
}
