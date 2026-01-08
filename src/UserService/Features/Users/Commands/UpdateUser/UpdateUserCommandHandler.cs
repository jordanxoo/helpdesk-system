using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Extensions;
using UserService.Data;


namespace UserService.Features.Users.Commands.UpdateUser;


public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand,UserDto>
{
    private readonly UserDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    

    public UpdateUserCommandHandler(UserDbContext dbContext, IDistributedCache cache, ILogger<UpdateUserCommandHandler> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserDto> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Updating user {userId}", command.UserId);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == command.UserId ,ct);

        if(user == null)
        {
            throw new NotFoundException("User", command.UserId);
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

        // Invalidate cache
        await _cache.RemoveAsync($"user-{user.Id}", ct);
        await _cache.RemoveAsync($"user-email-{user.Email.ToLower()}", ct);

        _logger.LogInformation("User {UserId} updated successfully", user.Id);

        return user.ToDto();
    }
}