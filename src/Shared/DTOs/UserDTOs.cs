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
/// DEPRECATED: Use UpdateUserRequest in UserService instead.
/// AuthService no longer manages profile data - only authentication credentials.
/// </summary>
[Obsolete("Use UpdateUserRequest in UserService instead")]
public record UpdateUserProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Role
);

/// <summary>
/// Request for updating user data in UserService.
/// UserService is the OWNER of all profile and business data.
/// All fields are optional - only provided fields will be updated.
/// </summary>
public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Role,
    Guid? OrganizationId,
    bool? IsActive
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? FullName,
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