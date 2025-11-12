using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserListResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GET /api/users - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;
        
        var result = await _userService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPost("search")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserListResponse>> Search([FromBody] UserFilterRequest filter)
    {
        _logger.LogInformation("POST /api/users/search - Filter: {@Filter}", filter);
        
        var result = await _userService.SearchAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // TODO: Security - Consider API Key or service-to-service auth for internal calls
                      // Currently allows unauthenticated access for TicketService communication
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/users/{Id}", id);

        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            _logger.LogWarning("User not found: {Id}", id);
            return NotFound(new { message = $"User with id {id} not found" });
        }

        return Ok(user);
    }

    [HttpGet("by-email/{email}")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetByEmail(string email)
    {
        _logger.LogInformation("GET /api/users/by-email/{Email}", email);

        var user = await _userService.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogWarning("User not found by email: {Email}", email);
            return NotFound(new { message = $"User with email {email} not found" });
        }

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("POST /api/users - Email: {Email}", request.Email);

        try
        {
            var user = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Email already exists: {Email}", request.Email);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update user-specific data (OrganizationId, IsActive).
    /// For profile data (firstName, lastName, phoneNumber, role), use AuthService PUT /api/auth/users/{id}.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("PUT /api/users/{Id} - updating UserService-specific fields only", id);

        try
        {
            var user = await _userService.UpdateAsync(id, request);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request data");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("DELETE /api/users/{Id}", id);

        try
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user ID in token" });
        }

        _logger.LogInformation("GET /api/users/me - UserId: {UserId}", userId);

        var user = await _userService.GetByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    [HttpPut("{id}/organization")]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> AssignOrganization(Guid id, [FromBody] AssignOrganizationRequest request)
    {
        _logger.LogInformation("PUT /api/users/{Id}/organization - OrganizationId: {OrganizationId}", id, request.OrganizationId);

        try
        {
            var updatedUser = await _userService.AssignOrganizationAsync(id, request.OrganizationId);
            return Ok(updatedUser);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpHead("{id}")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Exists(Guid id)
    {
        _logger.LogInformation("HEAD /api/users/{Id}", id);
        
        var exists = await _userService.ExistsAsync(id);
        return exists ? Ok() : NotFound();
    }
}
