using System.Reflection.Metadata.Ecma335;
using Amazon.S3.Model;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using UserService.Controllers;
using UserService.Services;
namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // TODO: Implementacja endpointów
    // - GET /api/users
    // - GET /api/users/{id}
    // - PUT /api/users/{id}
    // - DELETE /api/users/{id}

    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    // pobierz uzytkownika po id
    // <param name = "id"> ID uzytkownika </param>
    // < returns> Dane uzytkownika </returns>

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/users/{Id}", id);

        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            _logger.LogInformation("User not found {ID}", id);

            return NotFound(new { message = $"User with id {id} not found" });
        }

        return Ok(user);
    }

    /// Pobierz użytkownika po emailu
    /// </summary>
    /// <param name="email">Email użytkownika</param>
    /// <returns>Dane użytkownika</returns>

    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult<UserDto>> GetByEmail(string email)
    {
        _logger.LogInformation("GET /api/users/by-email/{Email}", email);

        var user =  await _userService.GetByEmailAsync(email);

        if (user == null)
        {
            _logger.LogInformation("User not found {email}", email);

            return NotFound(new { message = $"User with email {email} not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Pobierz listę wszystkich użytkowników (z paginacją)
    /// </summary>
    /// <param name="page">Numer strony (domyślnie 1)</param>
    /// <param name="pageSize">Rozmiar strony (domyślnie 10)</param>
    /// <returns>Lista użytkowników</returns>
    /// 
    [HttpGet]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]

    public async Task<ActionResult<UserListResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        _logger.LogInformation("GET /api/users?page={Page}&pageSize={PageSize}", page, pageSize);

        //validate params

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _userService.GetAllAsync(page, pageSize);

        return Ok(result);

    }

    /// <summary>
    /// Wyszukaj użytkowników z filtrowaniem
    /// </summary>
    /// <param name="filter">Parametry filtrowania</param>
    /// <returns>Lista użytkowników</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]

    public async Task<ActionResult<UserListResponse>> Search([FromBody] UserFilterRequest request)
    {
        _logger.LogInformation("POST /api/users/search {@Filter}", request);

        var result = await _userService.SearchAsync(request);
        return Ok(result);
    }
    /// <summary>
    /// Utwórz nowego użytkownika
    /// </summary>
    /// <param name="request">Dane nowego użytkownika</param>
    /// <returns>Utworzony użytkownik</returns>

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "Email already exsists: {email}", request.Email);
            return Conflict(new { message = e.Message });
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Invalid request data");
            return BadRequest(new { message = e.Message });
        }
    }
    /// <summary>
    /// Aktualizuj użytkownika
    /// </summary>
    /// <param name="id">ID użytkownika</param>
    /// <param name="request">Dane do aktualizacji</param>
    /// <returns>Zaktualizowany użytkownik</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult<UserDto>> Update(
        Guid id, [FromBody] UpdateUserRequest request
    )
    {
        _logger.LogInformation("PUT /api/users/{Id} {@Request}", id, request);

        try
        {
            var user = await _userService.UpdateAsync(id, request);
            return Ok(user);
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "User not found: {Id}", id);
            return NotFound(new { message = e.Message });
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Invalid request data");
            return BadRequest(new { message = e.Message });
        }

    }
    /// <summary>
    /// Usuń użytkownika (soft delete)
    /// </summary>
    /// <param name="id">ID użytkownika</param>
    /// <returns>204 No Content</returns>

    [HttpDelete("{id}")]
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
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "User not found: {Id}", id);
            return NotFound(new { message = e.Message });
        }
    }
    /// <summary>
    /// Sprawdź czy użytkownik istnieje
    /// </summary>
    /// <param name="id">ID użytkownika</param>
    /// <returns>True/False</returns>
    [HttpHead("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult> Exists(Guid id)
    {
        _logger.LogInformation("HEAD /api/users/{Id}", id);

        var exists = await _userService.ExistsAsync(id);

        return exists ? Ok() : NotFound();
    }
}
