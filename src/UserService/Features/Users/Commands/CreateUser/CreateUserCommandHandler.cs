using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs;
using Shared.Events;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Models;
using UserService.Data;


namespace UserService.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand,UserDto>
{
    private readonly UserDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(UserDbContext dbContext, IPublishEndpoint publishEndpoint, IDistributedCache cache, ILogger<CreateUserCommandHandler> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Creating user with email: {email}",command.Email);

        var exsistingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == command.Email,ct);
    
        if(exsistingUser != null)
        {
            throw new ConflictException("User", "email", command.Email);
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
        
    
    return user.ToDto();
    }
}