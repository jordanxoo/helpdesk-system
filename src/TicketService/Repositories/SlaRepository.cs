using Microsoft.EntityFrameworkCore;
using Shared.Models;
using TicketService.Data;

namespace TicketService.Repositories;

public class SlaRepository : ISlaRepository
{
    private readonly TicketDbContext _context;

    public SlaRepository(TicketDbContext context)
    {
        _context = context;
    }

    public async Task<SLA?> GetByIdAsync(Guid id)
    {
        return await _context.Slas.FindAsync(id);
    }

    public async Task<List<SLA>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Slas
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SLA>> GetActiveAsync()
    {
        return await _context.Slas
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(bool activeOnly = false)
    {
        var query = _context.Slas.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }
        
        return await query.CountAsync();
    }

    public async Task<SLA> CreateAsync(SLA sla)
    {
        sla.CreatedAt = DateTime.UtcNow;
        _context.Slas.Add(sla);
        await _context.SaveChangesAsync();
        return sla;
    }

    public async Task<SLA> UpdateAsync(SLA sla)
    {
        sla.UpdatedAt = DateTime.UtcNow;
        _context.Slas.Update(sla);
        await _context.SaveChangesAsync();
        return sla;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Slas.AnyAsync(s => s.Id == id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var sla = await _context.Slas.FindAsync(id);
        if (sla != null)
        {
            _context.Slas.Remove(sla);
            await _context.SaveChangesAsync();
        }
    }
}
