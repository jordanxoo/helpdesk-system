using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models;
using TicketService.Repositories;

namespace TicketService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.Administrator)]
public class SlasController : ControllerBase
{
    private readonly ISlaRepository _repository;
    private readonly ILogger<SlasController> _logger;

    public SlasController(ISlaRepository repository, ILogger<SlasController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Pobiera wszystkie SLA (tylko Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SLA>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SLA>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool activeOnly = false)
    {
        var slas = activeOnly 
            ? await _repository.GetActiveAsync()
            : await _repository.GetAllAsync(page, pageSize);

        var total = await _repository.GetTotalCountAsync(activeOnly);

        return Ok(new
        {
            data = slas,
            page,
            pageSize,
            total,
            totalPages = activeOnly ? 1 : (int)Math.Ceiling(total / (double)pageSize)
        });
    }

    /// <summary>
    /// Pobiera SLA po ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SLA), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SLA>> GetById(Guid id)
    {
        var sla = await _repository.GetByIdAsync(id);

        if (sla == null)
        {
            return NotFound(new { message = $"SLA {id} not found" });
        }

        return Ok(sla);
    }

    /// <summary>
    /// Tworzy nowe SLA
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SLA), StatusCodes.Status201Created)]
    public async Task<ActionResult<SLA>> Create([FromBody] CreateSlaRequest request)
    {
        var sla = new SLA
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ResponseTimeCritical = request.ResponseTimeCritical,
            ResponseTimeHigh = request.ResponseTimeHigh,
            ResponseTimeMedium = request.ResponseTimeMedium,
            ResponseTimeLow = request.ResponseTimeLow,
            ResolutionTimeCritical = request.ResolutionTimeCritical,
            ResolutionTimeHigh = request.ResolutionTimeHigh,
            ResolutionTimeMedium = request.ResolutionTimeMedium,
            ResolutionTimeLow = request.ResolutionTimeLow,
            IsActive = true
        };

        var created = await _repository.CreateAsync(sla);

        _logger.LogInformation("SLA created: {SlaId} - {Name}", created.Id, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Aktualizuje SLA
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SLA), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SLA>> Update(Guid id, [FromBody] UpdateSlaRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing == null)
        {
            return NotFound(new { message = $"SLA {id} not found" });
        }

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.ResponseTimeCritical = request.ResponseTimeCritical;
        existing.ResponseTimeHigh = request.ResponseTimeHigh;
        existing.ResponseTimeMedium = request.ResponseTimeMedium;
        existing.ResponseTimeLow = request.ResponseTimeLow;
        existing.ResolutionTimeCritical = request.ResolutionTimeCritical;
        existing.ResolutionTimeHigh = request.ResolutionTimeHigh;
        existing.ResolutionTimeMedium = request.ResolutionTimeMedium;
        existing.ResolutionTimeLow = request.ResolutionTimeLow;
        existing.IsActive = request.IsActive;

        var updated = await _repository.UpdateAsync(existing);

        _logger.LogInformation("SLA updated: {SlaId}", id);

        return Ok(updated);
    }

    /// <summary>
    /// Usuwa SLA
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return NotFound(new { message = $"SLA {id} not found" });
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("SLA deleted: {SlaId}", id);

        return NoContent();
    }
}

// DTOs
public record CreateSlaRequest(
    string Name,
    string? Description,
    int ResponseTimeCritical,
    int ResponseTimeHigh,
    int ResponseTimeMedium,
    int ResponseTimeLow,
    int ResolutionTimeCritical,
    int ResolutionTimeHigh,
    int ResolutionTimeMedium,
    int ResolutionTimeLow
);

public record UpdateSlaRequest(
    string Name,
    string? Description,
    int ResponseTimeCritical,
    int ResponseTimeHigh,
    int ResponseTimeMedium,
    int ResponseTimeLow,
    int ResolutionTimeCritical,
    int ResolutionTimeHigh,
    int ResolutionTimeMedium,
    int ResolutionTimeLow,
    bool IsActive
);
