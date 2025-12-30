using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using UserService.Data;


namespace UserService.Features.Users.Commands.UpdateUser;


public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand,UserDto>
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    

    public UpdateUserCommandHandler(UserDbContext dbContext, ILogger<UpdateUserCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Updating user {userId}", command.UserId);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId ,ct);

        if(user == null)
        {
            throw new KeyNotFoundException($"User {command.UserId} not found");
        }


   if (!string.IsNullOrEmpty(command.FirstName))
            user.FirstName = command.FirstName;

        if (!string.IsNullOrEmpty(command.LastName))
            user.LastName = command.LastName;

        if (!string.IsNullOrEmpty(command.PhoneNumber))
            user.PhoneNumber = command.PhoneNumber;

        if (!string.IsNullOrEmpty(command.Role))
            user.Role = Enum.Parse<Shared.Models.UserRole>(command.Role);

        if (command.OrganizationId.HasValue)
            user.OrganizationId = command.OrganizationId.Value;

        if (command.IsActive.HasValue)
            user.IsActive = command.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} updated successfully", user.Id);

        return MapToDto(user);
    }

    private static UserDto MapToDto(Shared.Models.User user)
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
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            UpdatedAt: DateTime.UtcNow
            
        );
    }
}