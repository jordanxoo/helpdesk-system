using Shared.Models;

namespace TicketService.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id);
    Task<Tag?> GetByNameAsync(string name);
    Task<List<Tag>> GetAllAsync();
    Task<List<Tag>> SearchAsync(string searchTerm);
    Task<Tag> CreateAsync(Tag tag);
    Task<Tag> UpdateAsync(Tag tag);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name);
}
