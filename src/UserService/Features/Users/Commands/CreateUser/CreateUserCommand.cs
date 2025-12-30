using MediatR;
using Shared.DTOs;
using Shared.Models;


namespace UserService.Features.Users.Commands.CreateUser;


public record CreateUserCommand : IRequest<UserDto>
{
    public string Email{get;set;} = string.Empty;
    public string FirstName{get;set;} = string.Empty;
    public string LastName {get;set;} =string.Empty;
    public string PhoneNumber {get;set;} = string.Empty;
    public UserRole Role {get;set;} 
    public Guid? OrganizationId {get;set;}

}