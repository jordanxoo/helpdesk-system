using Microsoft.EntityFrameworkCore;
using Shared.Models;
using TicketService.Data;

namespace TicketService.Repositories;

public class TagRepository : ITagRepository
{
    private readonly TicketDbContext _context;

    public TagRepository(TicketDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Tag>> SearchAsync(string searchTerm)
    {
        return await _context.Tags
            .Where(t => t.Name.Contains(searchTerm) || 
                       (t.Description != null && t.Description.Contains(searchTerm)))
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag> CreateAsync(Tag tag)
    {
        tag.CreatedAt = DateTime.UtcNow;
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteAsync(Guid id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Tags.AnyAsync(t => t.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower());
    }
}
