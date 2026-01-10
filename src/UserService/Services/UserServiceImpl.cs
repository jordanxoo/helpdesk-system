namespace UserService.Services;

using MassTransit;
using Shared.DTOs;
using Shared.Events;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Models;
using UserService.Repositories;
using UserService.Data;

public class UserServiceImpl : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly UserDbContext _context;
    private readonly ILogger<UserServiceImpl> _logger;

    public UserServiceImpl(
        IUserRepository repository, 
        IPublishEndpoint publishEndpoint, 
        UserDbContext context,
        ILogger<UserServiceImpl> logger)
    {
        this._logger = logger;
        this._repository = repository;
        this._publishEndpoint = publishEndpoint;
        this._context = context;
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching user with id: {UserId}", id);

        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }
        return user.ToDto();
    }
    public async Task<UserDto> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Fetching user with email: {email}", email);
        var user = await _repository.GetByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException($"User with email '{email}' was not found.");
        }
        return user.ToDto();
    }

    public async Task<UserListResponse> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Fetching All users - Page {page}, Page size {page size}", page, pageSize);

        var users = await _repository.GetAllAsync(page, pageSize);
        var totalCount = await _repository.GetTotalCountAsync(null, null, null);

        return new UserListResponse(
            Users: users.Select(u => u.ToDto()).ToList(),
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

        return new UserListResponse(users.Select(u => u.ToDto()).ToList(), totalCount, filter.Page, filter.PageSize);
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

        return createdUser.ToDto();
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

        return createdUser.ToDto();
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
            return user.ToDto();
        }

        user.UpdatedAt = DateTime.UtcNow;
        var updatedUser = await _repository.UpdateAsync(user);

        _logger.LogInformation("User updated successfully: {UserId}", id);

        return updatedUser.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting user with id: {UserId}", id);

        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User", id);
        }

        // Delete from UserService database (marks for deletion but doesn't save)
        // Direct removal to avoid double query - we already have the user object
        _context.Users.Remove(user);

        // Publish event for AuthService to consume
        // NOTE: MassTransit with Outbox pattern automatically wraps this in a transaction
        // The event is saved to the outbox table in the same transaction as user deletion
        await _publishEndpoint.Publish(new UserDeletedEvent
        {
            UserId = id,
            Email = user.Email
        });

        // Save both the deletion and the outbox message in one atomic transaction
        // The Outbox pattern ensures both succeed or both fail
        await _context.SaveChangesAsync();

        _logger.LogInformation("User deleted and UserDeletedEvent published: {UserId}", id);
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
        
        return updatedUser.ToDto();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }
}
