namespace Shared.DTOs;

// DTO do tworzenia użytkownika
public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Role  // Use UserRoles constants: Customer, Agent, Administrator
);

/// <summary>
/// Request for updating user profile data in AuthService.
/// AuthService is the source of truth for: FirstName, LastName, PhoneNumber, Role.
/// </summary>
public record UpdateUserProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Role
);

/// <summary>
/// Request for updating user-specific data in UserService.
/// Only OrganizationId and IsActive can be modified here.
/// </summary>
public record UpdateUserRequest(
    Guid? OrganizationId,
    bool? IsActive
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string PhoneNumber,
    string Role,
    Guid? OrganizationId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsActive
);


public record UserListResponse(
    List<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);

public record UserFilterRequest(
    string? SearchTerm,
    string? Role,
    bool? IsActive,
    int Page = 1,
    int PageSize = 10
);

public record AssignOrganizationRequest(
    Guid OrganizationId
);