namespace UserService.Services;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualBasic;
using Shared.DTOs;
using Shared.Models;
using UserService.Repositories;

public class UserServiceImpl : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserServiceImpl> _logger;

    public UserServiceImpl(IUserRepository repository, ILogger<UserServiceImpl> logger)
    {
        this._logger = logger;
        this._repository = repository;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching user with id: {UserId}", id);

        var user = await _repository.GetByIdAsync(id);
        return user != null ? MapToDto(user) : null;
    }
    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Fetching user with email: {email}", email);
        var user = await _repository.GetByEmailAsync(email);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserListResponse> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Fetching All users - Page {page}, Page size {page size}", page, pageSize);

        var users = await _repository.GetAllAsync(page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null);

        return new UserListResponse(
            Users: users.Select(MapToDto).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize
        );
    }

    public async Task<UserListResponse> SearchAsync(UserFilterRequest filter)
    {
        _logger.LogInformation("Searching users with filter: {@filter}", filter);

        var users = await _repository.SearchAsync(
            filter.SearchTerm, filter.Role, filter.IsActive, filter.Page, filter.PageSize
        );
        var totalCount = await _repository.GetTotalCountAsync(filter.SearchTerm, filter.Role, filter.IsActive);

        return new UserListResponse(users.Select(MapToDto).ToList(), totalCount, filter.Page, filter.PageSize);
    }
    
    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user with email: {email}", request.Email);

        if (await _repository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists");
        }
        if (!Enum.TryParse<UserRole>(request.Role, out var role))
        {
            throw new ArgumentException($"Invalid role: {request.Role}");
        }

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = role,
            IsActive = true
        };

        var createdUser = await _repository.CreateAsync(user);

        _logger.LogInformation("User created successfully with id: {UserId}", createdUser.Id);

        return MapToDto(createdUser);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, Guid userId)
    {
        _logger.LogInformation("Creating user with specific ID: {UserId}, email: {Email}", userId, request.Email);

        if (await _repository.GetByIdAsync(userId) != null)
        {
            throw new InvalidOperationException($"User with id {userId} already exists");
        }
        
        if (await _repository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists");
        }
        
        if (!Enum.TryParse<UserRole>(request.Role, out var role))
        {
            throw new ArgumentException($"Invalid role: {request.Role}");
        }

        var user = new User
        {
            Id = userId, // Używamy ID z AuthService
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = role,
            IsActive = true
        };

        var createdUser = await _repository.CreateAsync(user);

        _logger.LogInformation("User created successfully with id: {UserId}", createdUser.Id);

        return MapToDto(createdUser);
    }


    /// <summary>
    /// Updates UserService-specific fields only (OrganizationId, IsActive).
    /// Profile data (firstName, lastName, phoneNumber, role) is managed by AuthService.
    /// Changes from AuthService are synced via ProfileUpdatedEvent.
    /// </summary>
    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        _logger.LogInformation("Updating UserService-specific fields for user: {UserId}", id);
        
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {id} not found");
        }

        var hasChanges = false;

        if (request.OrganizationId.HasValue && user.OrganizationId != request.OrganizationId.Value)
        {
            user.OrganizationId = request.OrganizationId.Value;
            hasChanges = true;
        }
        
        if (request.IsActive.HasValue && user.IsActive != request.IsActive.Value)
        {
            user.IsActive = request.IsActive.Value;
            hasChanges = true;
        }

        if (!hasChanges)
        {
            _logger.LogInformation("No changes detected for user: {UserId}", id);
            return MapToDto(user);
        }

        user.UpdatedAt = DateTime.UtcNow;
        var updatedUser = await _repository.UpdateAsync(user);
        
        _logger.LogInformation("User updated successfully: {UserId}", id);
        
        return MapToDto(updatedUser);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting user with id: {UserId}", id);
        
        if (!await _repository.ExistsAsync(id))
        {
            throw new KeyNotFoundException($"User with id {id} not found");
        }

        await _repository.DeleteAsync(id);
        
        _logger.LogInformation("User deleted successfully: {UserId}", id);
    }

    public async Task<UserDto> AssignOrganizationAsync(Guid userId, Guid organizationId)
    {
        _logger.LogInformation("Assigning organization {OrganizationId} to user {UserId}", organizationId, userId);
        
        var user = await _repository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {userId} not found");
        }

        user.OrganizationId = organizationId;
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _repository.UpdateAsync(user);
        
        _logger.LogInformation("Organization assigned successfully: User {UserId} -> Organization {OrganizationId}", userId, organizationId);
        
        return MapToDto(updatedUser);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }
    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            FullName: user.FullName,
            PhoneNumber: user.PhoneNumber,
            Role: user.Role.ToString(),
            OrganizationId: user.OrganizationId,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            IsActive: user.IsActive);
    }
}
