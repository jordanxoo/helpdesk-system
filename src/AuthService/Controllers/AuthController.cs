using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;
using Shared.Events;
using AuthService.Data;
using AuthService.Services;
using UAParser;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using MediatR;
using AuthService.Features.Auth.Commands.RegisterUser;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthController> _logger;
    private readonly ISessionService _sessionService;
    private readonly IMediator _mediator;
    public AuthController(
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService, 
        IPublishEndpoint publishEndpoint,
        ISessionService sessionService, 
        IMediator mediator,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _publishEndpoint = publishEndpoint;
        _sessionService = sessionService;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register new user - creates authentication credentials only.
    /// Profile data is managed by UserService (synced via UserRegisteredEvent).
    /// After registration, call GET /api/users/me to get full user profile.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthTokenResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role
        };

        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(Register),response);
    }
    /// <summary>
    /// Login user - verifies credentials and returns JWT token.
    /// To get user profile, call GET /api/users/me with the token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {email}", request.Email);
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed - invalid password for: {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var deviceInfo = GetDeviceInfo(Request.Headers["User-Agent"].ToString());

        var ipAddress = GetClientIpAddress();

        var expiresAt = DateTime.UtcNow.AddDays(7);

        var sessionId = await _sessionService.CreateSessionAsync(
            userId: Guid.Parse(user.Id),
            deviceInfo: deviceInfo,
            ipAddress: ipAddress,
            expiresAt: expiresAt
        );

        var accessToken = await _tokenService.GenerateTokenWithSessionAsync(user, sessionId);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        _logger.LogInformation("User {userId} logged in successfully from {ipAddress} {DeviceInfo}", user.Id, ipAddress, deviceInfo);

        var response = new AuthTokenResponse(
            Token: accessToken,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(60)
        );

        return Ok(response);
    }
    
    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var sessionId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if(string.IsNullOrEmpty(sessionId))
        {
            _logger.LogWarning("Logout failed - no session ID in token");
            return BadRequest(new {message = "Invalid token - no session ID"});
        }    

        await _sessionService.RevokeSessionAsync(sessionId);

        _logger.LogInformation("User logged out successfully. SessionId: {sessionID}",sessionId);

        return Ok(new {message = "Logged out successfully"});
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500,new {message = "An error occured during logout"});
        }
    }


    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Logout All failed - no user ID in token");
                return BadRequest(new {message = "Invalid token - no user ID"});
            }

            await _sessionService.RevokeAllUserSessionsAsync(Guid.Parse(userId));
            _logger.LogInformation("User {userId} logged out from all devices",userId);
            return Ok(new {message = "logged out from all devices successfully"});
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error during logout-all");
            return StatusCode(500,new {message = "An error occured during logout"});
        }
    }

    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<UserSession>>> GetActiveSessions()
    {
        try 
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest(new {message = "Invalid token - no user ID"});
            }
            var sessions = await _sessionService.GetActiveSessionsAsync(Guid.Parse(userId));

            return Ok(sessions);
        }catch(Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions");
            return StatusCode(500,new {message = "An error occured retrieving sessions"});
        }
    }


    /// <summary>
    /// Pobiera wymogi haseł dla walidacji po stronie frontendu
    /// </summary>
    [HttpGet("password-requirements")]
    [ProducesResponseType(typeof(PasswordRequirements), StatusCodes.Status200OK)]
    public ActionResult<PasswordRequirements> GetPasswordRequirements()
    {
        var passwordOptions = _userManager.Options.Password;
        
        var requirements = new PasswordRequirements
        {
            MinimumLength = passwordOptions.RequiredLength,
            RequireDigit = passwordOptions.RequireDigit,
            RequireLowercase = passwordOptions.RequireLowercase,
            RequireUppercase = passwordOptions.RequireUppercase,
            RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric
        };

        return Ok(requirements);
    }


    private string GetDeviceInfo(string userAgent)
    {
        if(string.IsNullOrEmpty(userAgent))
        {
            return "Unknown Device";
        }
        try
        {
            var parser = Parser.GetDefault();
            var clientInfo = parser.Parse(userAgent);

            var browser = clientInfo.UA.Family ?? "Unknown Browser";
            var browserVersion = !string.IsNullOrEmpty(clientInfo.UA.Major) ? 
            $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}" : "";

            var os = clientInfo.OS.Family ?? "Unknown OS";
            var osVersion = !string.IsNullOrEmpty(clientInfo.OS.Major)
            ? $"{clientInfo.OS.Major}" : "";

            var device = clientInfo.Device.Family ?? "";

            var result = $"{browser}";

            if(!string.IsNullOrEmpty(browserVersion))
            {
                result += $" {browserVersion}";
            }
            result += $" on {os}";

            if(!string.IsNullOrEmpty(osVersion))
            {
                result += $" {osVersion}";
            }

            if(!string.IsNullOrEmpty(device) && device != "Other")
            {
                result += $" ({device})";
            }
            return result;
        }catch(Exception ex)
        {
            _logger.LogWarning(ex,"Failed to parse User-Agent: {userAgent}",userAgent);
            return "Unknown Device";
        }
    }


    private string GetClientIpAddress()
    {
        var cloudflareIp = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if(!string.IsNullOrEmpty(cloudflareIp))
        {
            _logger.LogDebug("IP from CloudFlare header: {cloudflareIP}",cloudflareIp);
            return cloudflareIp.Trim();
        }

        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if(!string.IsNullOrEmpty(forwardedFor))
        {
            var clientIp = forwardedFor.Split(",")[0].Trim();
            _logger.LogDebug("IP from X-Forwarded-For header: {IP}",clientIp);
            return clientIp;
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if(!string.IsNullOrEmpty(realIp))
        {
            _logger.LogDebug("IP from X-Real-IP header: {IP}",realIp);
            return realIp;
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        if(!string.IsNullOrEmpty(remoteIp))
        {
            _logger.LogDebug("Ip address from RemoteIpAddress: {IP}",remoteIp);
            return remoteIp;
        }

        _logger.LogWarning("Could not determine client IP address");
        return "Unknown IP";
    }
}
