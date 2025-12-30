using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Events;
using Shared.Models;
using UserService.Data;


namespace UserService.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand,UserDto>
{
    private readonly UserDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(UserDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<CreateUserCommandHandler> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Creating user with email: {email}",command.Email);

        var exsistingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email,ct);
    
        if(exsistingUser != null)
        {
            throw new InvalidOperationException($"User with email: {command.Email} already exsists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PhoneNumber = command.PhoneNumber,
            Role = command.Role,
            OrganizationId = command.OrganizationId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("User Created Successfully: {userId}",user.Id);
        await _publishEndpoint.Publish(new UserCreatedEvent
        {
            UserId = user.Id,
            Email = user.Email,
            Timestamp = DateTime.UtcNow
        });
        
    
    return MapToDto(user);
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            FullName: $"{user.FirstName} + {user.LastName}",
            PhoneNumber: user.PhoneNumber,
            Role: user.Role.ToString(),
            OrganizationId: user.OrganizationId,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            IsActive: user.IsActive
        );
    }
}