using Shared.DTOs;

namespace UserService.Services;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> GetByEmailAsync(string email);
    Task<UserListResponse> GetAllAsync(int page, int pageSize);
    Task<UserListResponse> SearchAsync(UserFilterRequest filter);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto> CreateAsync(CreateUserRequest request, Guid userId); // Overload z konkretnym ID
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<UserDto> AssignOrganizationAsync(Guid userId, Guid organizationId);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
