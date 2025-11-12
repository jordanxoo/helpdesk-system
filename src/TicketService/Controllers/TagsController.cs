using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.Models;
using TicketService.Repositories;

namespace TicketService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{UserRoles.Agent},{UserRoles.Administrator}")]
public class TagsController : ControllerBase
{
    private readonly ITagRepository _repository;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagRepository repository, ILogger<TagsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Pobiera wszystkie tagi (Agent/Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Tag>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Tag>>> GetAll([FromQuery] string? search = null)
    {
        var tags = string.IsNullOrWhiteSpace(search)
            ? await _repository.GetAllAsync()
            : await _repository.SearchAsync(search);

        return Ok(tags);
    }

    /// <summary>
    /// Pobiera tag po ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tag>> GetById(Guid id)
    {
        var tag = await _repository.GetByIdAsync(id);

        if (tag == null)
        {
            return NotFound(new { message = $"Tag {id} not found" });
        }

        return Ok(tag);
    }

    /// <summary>
    /// Tworzy nowy tag (tylko Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Tag>> Create([FromBody] CreateTagRequest request)
    {
        // Sprawdź czy tag o tej nazwie już istnieje
        if (await _repository.ExistsByNameAsync(request.Name))
        {
            return BadRequest(new { message = $"Tag with name '{request.Name}' already exists" });
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Color = request.Color,
            Description = request.Description
        };

        var created = await _repository.CreateAsync(tag);

        _logger.LogInformation("Tag created: {TagId} - {Name}", created.Id, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Aktualizuje tag (tylko Admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Tag>> Update(Guid id, [FromBody] UpdateTagRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing == null)
        {
            return NotFound(new { message = $"Tag {id} not found" });
        }

        // Sprawdź czy inna tag o tej nazwie już istnieje
        var tagWithName = await _repository.GetByNameAsync(request.Name);
        if (tagWithName != null && tagWithName.Id != id)
        {
            return BadRequest(new { message = $"Tag with name '{request.Name}' already exists" });
        }

        existing.Name = request.Name;
        existing.Color = request.Color;
        existing.Description = request.Description;

        var updated = await _repository.UpdateAsync(existing);

        _logger.LogInformation("Tag updated: {TagId}", id);

        return Ok(updated);
    }

    /// <summary>
    /// Usuwa tag (tylko Admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _repository.ExistsAsync(id))
        {
            return NotFound(new { message = $"Tag {id} not found" });
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Tag deleted: {TagId}", id);

        return NoContent();
    }
}

// DTOs
public record CreateTagRequest(
    string Name,
    string? Color,
    string? Description
);

public record UpdateTagRequest(
    string Name,
    string? Color,
    string? Description
);
