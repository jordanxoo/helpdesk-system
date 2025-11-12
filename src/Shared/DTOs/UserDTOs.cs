namespace Shared.DTOs;

// DTO do tworzenia użytkownika
public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Role  // Use UserRoles constants: Customer, Agent, Administrator
);


public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Role,
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