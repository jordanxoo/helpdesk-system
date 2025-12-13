using MediatR;
using Shared.DTOs;

namespace AuthService.Features.Auth.Commands.RegisterUser;

public record RegisterUserCommand : IRequest<AuthTokenResponse>
{
    public string Email {get;init;} = string.Empty;
     public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}