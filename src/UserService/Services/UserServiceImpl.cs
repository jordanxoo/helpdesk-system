namespace UserService.Services;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualBasic;
using Shared.DTOs;
using Shared.Exceptions;
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

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching user with id: {UserId}", id);

        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }
        return MapToDto(user);
    }
    public async Task<UserDto> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Fetching user with email: {email}", email);
        var user = await _repository.GetByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException($"User with email '{email}' was not found.");
        }
        return MapToDto(user);
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
            throw new ConflictException("User", "email", request.Email);
        }
        if (!Enum.TryParse<UserRole>(request.Role, out var role))
        {
            throw new BadRequestException($"Invalid role: {request.Role}");
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
            throw new ConflictException("User", "id", userId);
        }
        
        if (await _repository.EmailExistsAsync(request.Email))
        {
            throw new ConflictException("User", "email", request.Email);
        }
        
        if (!Enum.TryParse<UserRole>(request.Role, out var role))
        {
            throw new BadRequestException($"Invalid role: {request.Role}");
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
    /// Updates user data - UserService is the OWNER of all profile and business data.
    /// All fields are optional - only provided fields will be updated.
    /// </summary>
    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user data for user: {UserId}", id);

        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        var hasChanges = false;

        // Update profile fields (UserService is now the owner)
        if (!string.IsNullOrWhiteSpace(request.FirstName) && user.FirstName != request.FirstName)
        {
            user.FirstName = request.FirstName;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName) && user.LastName != request.LastName)
        {
            user.LastName = request.LastName;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber)
        {
            user.PhoneNumber = request.PhoneNumber;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (Enum.TryParse<UserRole>(request.Role, out var newRole) && user.Role != newRole)
            {
                user.Role = newRole;
                hasChanges = true;
            }
        }

        // Update business fields
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
            throw new NotFoundException("User", id);
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
            throw new NotFoundException("User", userId);
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
