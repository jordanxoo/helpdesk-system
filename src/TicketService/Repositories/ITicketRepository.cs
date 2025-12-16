using Shared.Models;

namespace TicketService.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, bool includeComments = true);
    Task<List<Ticket>> GetAllAsync(int page, int pageSize);
    Task<List<Ticket>> SearchAsync(
        string? searchTerm, 
        string? status, 
        string? priority,
        string? category,
        Guid? customerId, 
        Guid? assignedAgentId, 
        int page, 
        int pageSize
    );
    Task<int> GetTotalCountAsync(
        string? searchTerm, 
        string? status, 
        string? priority,
        string? category,
        Guid? customerId, 
        Guid? assignedAgentId
    );
    Task<List<Ticket>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize);
    Task<List<Ticket>> GetByAgentIdAsync(Guid agentId, int page, int pageSize);
    Task<Ticket> CreateAsync(Ticket ticket);
    Task<Ticket> UpdateAsync(Ticket ticket);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    Task<TicketComment> AddCommentAsync(TicketComment comment);
    Task<List<TicketComment>> GetCommentsAsync(Guid ticketId, bool includeInternal = false);
    
    // Statistics methods
    Task<Dictionary<string, int>> GetCountByStatusAsync();
    Task<Dictionary<string, int>> GetCountByPriorityAsync();
    Task<int> GetUnassignedCountAsync();
}