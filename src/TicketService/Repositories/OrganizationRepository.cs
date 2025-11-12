using Microsoft.EntityFrameworkCore;
using Shared.Models;
using TicketService.Data;

namespace TicketService.Repositories;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly TicketDbContext _context;

    public OrganizationRepository(TicketDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(Guid id)
    {
        return await _context.Organizations
            .Include(o => o.Sla)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Organization>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Organizations
            .Include(o => o.Sla)
            .OrderBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Organization>> GetActiveAsync(int page, int pageSize)
    {
        return await _context.Organizations
            .Include(o => o.Sla)
            .Where(o => o.IsActive)
            .OrderBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(bool activeOnly = false)
    {
        var query = _context.Organizations.AsQueryable();
        
        if (activeOnly)
        {
            query = query.Where(o => o.IsActive);
        }
        
        return await query.CountAsync();
    }

    public async Task<Organization> CreateAsync(Organization organization)
    {
        organization.CreatedAt = DateTime.UtcNow;
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();
        return organization;
    }

    public async Task<Organization> UpdateAsync(Organization organization)
    {
        organization.UpdatedAt = DateTime.UtcNow;
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync();
        return organization;
    }

    public async Task DeleteAsync(Guid id)
    {
        var organization = await _context.Organizations.FindAsync(id);
        if (organization != null)
        {
            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Organizations.AnyAsync(o => o.Id == id);
    }
}
