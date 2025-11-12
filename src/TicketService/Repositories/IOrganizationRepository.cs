using Shared.Models;

namespace TicketService.Repositories;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(Guid id);
    Task<List<Organization>> GetAllAsync(int page, int pageSize);
    Task<List<Organization>> GetActiveAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync(bool activeOnly = false);
    Task<Organization> CreateAsync(Organization organization);
    Task<Organization> UpdateAsync(Organization organization);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
