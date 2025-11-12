using Shared.Models;

namespace TicketService.Repositories;

public interface ISlaRepository
{
    Task<SLA?> GetByIdAsync(Guid id);
    Task<List<SLA>> GetAllAsync(int page, int pageSize);
    Task<List<SLA>> GetActiveAsync();
    Task<int> GetTotalCountAsync(bool activeOnly = false);
    Task<SLA> CreateAsync(SLA sla);
    Task<SLA> UpdateAsync(SLA sla);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
