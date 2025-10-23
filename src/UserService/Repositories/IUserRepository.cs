using Shared.Models;

namespace UserService.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllAsync(int page, int pageSize);
    Task<List<User>> SearchAsync(string? searchTerm, string? role, bool? isActive, int page, int pageSize);
    Task<int> GetTotalCountAsync(string? searchTerm, string? role, bool? isActive);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}
