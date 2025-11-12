using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models;
using TicketService.Repositories;

namespace TicketService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Administrator)]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationRepository _repository;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(
        IOrganizationRepository repository,
        ILogger<OrganizationsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Pobiera wszystkie organizacje (tylko Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Organization>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Organization>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool activeOnly = false)
    {
        var organizations = activeOnly 
            ? await _repository.GetActiveAsync(page, pageSize)
            : await _repository.GetAllAsync(page, pageSize);
            
        var total = await _repository.GetTotalCountAsync(activeOnly);

        return Ok(new
        {
            data = organizations,
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    /// <summary>
    /// Pobiera organizację po ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Organization>> GetById(Guid id)
    {
        var organization = await _repository.GetByIdAsync(id);

        if (organization == null)
        {
            return NotFound(new { message = $"Organization {id} not found" });
        }

        return Ok(organization);
    }

    /// <summary>
    /// Tworzy nową organizację
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Organization>> Create([FromBody] CreateOrganizationRequest request)
    {
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            SlaId = request.SlaId,
            IsActive = true
        };

        var created = await _repository.CreateAsync(organization);

        _logger.LogInformation("Organization created: {OrganizationId} - {Name}", created.Id, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Aktualizuje organizację
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Organization), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Organization>> Update(Guid id, [FromBody] UpdateOrganizationRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing == null)
        {
            return NotFound(new { message = $"Organization {id} not found" });
        }

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.ContactEmail = request.ContactEmail;
        existing.ContactPhone = request.ContactPhone;
        existing.SlaId = request.SlaId;
        existing.IsActive = request.IsActive;

        var updated = await _repository.UpdateAsync(existing);

        _logger.LogInformation("Organization updated: {OrganizationId}", id);

        return Ok(updated);
    }

    /// <summary>
    /// Usuwa organizację
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return NotFound(new { message = $"Organization {id} not found" });
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Organization deleted: {OrganizationId}", id);

        return NoContent();
    }
}

// DTOs dla API
public record CreateOrganizationRequest(
    string Name,
    string? Description,
    string ContactEmail,
    string? ContactPhone,
    Guid? SlaId
);

public record UpdateOrganizationRequest(
    string Name,
    string? Description,
    string ContactEmail,
    string? ContactPhone,
    Guid? SlaId,
    bool IsActive
);
