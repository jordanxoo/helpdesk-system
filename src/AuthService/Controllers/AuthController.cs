using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Models;
using AuthService.Data;
using AuthService.Services;
using Amazon.SQS.Model;


namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// rejestracja nowego uzytkownika
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed - user already exsists!: {Email}", request.Email);
            return BadRequest(new { message = "User with this email already exists" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Registration failed for email {Email}: {Errors}", request.Email, string.Join(",", result.Errors.Select(e => e.Description)));

            return BadRequest(new { message = "Registration failed", errors = result.Errors.Select(e => e.Description) });
        }

        // domyslna rola - customer, chb git???????/
        await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());

        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        //automatyczne logowanie
        // po zalozeniu konta
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        var response = new LoginResponse(
            Token: accessToken,

            RefreshToken: refreshToken,

            ExpiresAt: DateTime.UtcNow.AddMinutes(60),

            User: new UserDto(
                Id: Guid.Parse(user.Id),
                Email: user.Email!,
                FirstName: user.FirstName,
                LastName: user.LastName,
                FullName: $"{user.FirstName} {user.LastName}",
                PhoneNumber: user.PhoneNumber ?? string.Empty,
                Role: roles.FirstOrDefault() ?? UserRole.Customer.ToString(),
                CreatedAt: user.CreatedAt,
                UpdatedAt: null,
                IsActive: true
            )
        );

        return CreatedAtAction(nameof(Register), response);
    }

    //logowanie uzytkownika

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]

    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
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

        _logger.LogInformation("user logged in successfully: {Email}", request.Email);

        //generujemy tokeny
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

        var response = new LoginResponse(
              Token: accessToken,
              RefreshToken: refreshToken,
              ExpiresAt: DateTime.UtcNow.AddMinutes(60),
              User: new UserDto(
                  Id: Guid.Parse(user.Id),
                  Email: user.Email!,
                  FirstName: user.FirstName,
                  LastName: user.LastName,
                  FullName: $"{user.FirstName} {user.LastName}",
                  PhoneNumber: user.PhoneNumber ?? string.Empty,
                  Role: roles.FirstOrDefault() ?? UserRole.Customer.ToString(),
                  CreatedAt: user.CreatedAt,
                  UpdatedAt: null,
                  IsActive: true
              ));
        return Ok(response);
    }

    //odswiezanie tokenu 

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User logout");

        await _signInManager.SignOutAsync();

        return Ok(new { message = "Logged out successfully" });
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
}
