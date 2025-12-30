using MediatR;
using Shared.DTOs;


namespace UserService.Features.Users.Commands.UpdateUser;
public record UpdateUserCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Role { get; init; }
    public Guid? OrganizationId { get; init; }
    public bool? IsActive { get; init; }
}