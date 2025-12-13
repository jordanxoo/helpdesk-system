using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using AuthService.Data;
using AuthService.Services;
using Shared.DTOs;
using Shared.Events;





namespace AuthService.Features.Auth.Commands.RegisterUser;

/// <summary>
/// HANDLER = "JAK zarejestrować użytkownika"
/// Implementuje IRequestHandler<TRequest, TResponse>
/// - TRequest = RegisterUserCommand 
/// /// - TResponse = AuthTokenResponse 
/// MediatR automatycznie:
/// 1. Znajdzie ten handler gdy wywołasz mediator.Send(RegisterUserCommand)
/// 2. Wstrzyknie zależności przez konstruktor 
/// 3. Wywoła metodę Handle
/// </summary>

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand,AuthTokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint; //mass transit do rabbitmq
    private readonly ILogger<RegisterUserCommand> _logger;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager
    ,ITokenService tokenService,
    IPublishEndpoint publishEndpoint,
    ILogger<RegisterUserCommand> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<AuthTokenResponse> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        
        _logger.LogInformation("Processing RegisterUserCommand for email: {email}", command.Email);

        var exsistingUser = await _userManager.FindByEmailAsync(command.Email);

        if(exsistingUser != null)
        {
            throw new InvalidOperationException("User with this email already exsists");
        }

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user,command.Password);
        if(!result.Succeeded)
        {
            var errors = string.Join(",", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user,command.Role);

        _logger.LogInformation("User registered successfully: {email}",command.Email);

        var userRegisteredEvent = new UserRegisteredEvent
        {
            UserId = Guid.Parse(user.Id),
            Email = user.Email!,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PhoneNumber = command.PhoneNumber,
            Role = command.Role
        };

        try
        {
            await _publishEndpoint.Publish(userRegisteredEvent,ct);
            _logger.LogInformation("UserRegisteredEvent published for user: {Email}",command.Email);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,"Failed to publish userRegisteredEvent, rollback of user creation");
            await _userManager.DeleteAsync(user);
            throw;
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user,roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id,refreshToken);

        return new AuthTokenResponse(
            Token: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(60)
        );
    }



}