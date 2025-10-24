using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using TicketService.Services;
using System.Security.Claims;

namespace TicketService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    /// <summary>
    /// Pobiera ticket po ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> GetById(Guid id)
    {
        var ticket = await _ticketService.GetByIdAsync(id);
        
        if (ticket == null)
        {
            return NotFound(new { message = $"Ticket {id} not found" });
        }

        return Ok(ticket);
    }

    /// <summary>
    /// Pobiera wszystkie tickety (z paginacją)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(TicketListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketListResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _ticketService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Wyszukuje tickety według różnych kryteriów
    /// </summary>
    [HttpPost("search")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(TicketListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketListResponse>> Search([FromBody] TicketFilterRequest filter)
    {
        var result = await _ticketService.SearchAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Pobiera moje tickety (dla zalogowanego customera)
    /// </summary>
    [HttpGet("my-tickets")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(TicketListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketListResponse>> GetMyTickets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user ID" });
        }

        var result = await _ticketService.GetMyTicketsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Pobiera tickety przypisane do agenta
    /// </summary>
    [HttpGet("assigned")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(TicketListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketListResponse>> GetAssignedTickets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user ID" });
        }

        var result = await _ticketService.GetAssignedTicketsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Tworzy nowy ticket
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDto>> Create([FromBody] CreateTicketRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user ID" });
        }

        try
        {
            var ticket = await _ticketService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Aktualizuje ticket
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Update(Guid id, [FromBody] UpdateTicketRequest request)
    {
        try
        {
            var ticket = await _ticketService.UpdateAsync(id, request);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Przypisuje ticket do agenta
    /// </summary>
    [HttpPost("{id}/assign/{agentId}")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> AssignToAgent(Guid id, Guid agentId)
    {
        try
        {
            var ticket = await _ticketService.AssignToAgentAsync(id, agentId);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Zmienia status ticketa
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Agent,Administrator")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> ChangeStatus(
        Guid id, 
        [FromBody] ChangeStatusRequest request)
    {
        try
        {
            var ticket = await _ticketService.ChangeStatusAsync(id, request.NewStatus);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Usuwa ticket
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _ticketService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Dodaje komentarz do ticketa
    /// </summary>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> AddComment(Guid id, [FromBody] AddCommentRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user ID" });
        }

        try
        {
            var ticket = await _ticketService.AddCommentAsync(id, userId, request);
            return Ok(ticket);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}