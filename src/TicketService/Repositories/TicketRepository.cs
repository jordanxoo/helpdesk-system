using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Shared.Models;
using TicketService.Data;

namespace TicketService.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly TicketDbContext _context;

    public TicketRepository(TicketDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, bool includeComments)
    {
        var query = _context.Tickets.AsQueryable();

        query = query.Include(t => t.Attachments);

        if (includeComments)
        {
            query = query.Include(t => t.Comments.OrderBy(c => c.CreatedAt));
        }
        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Ticket>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Tickets.Include(t => t.Comments).
        OrderByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<List<Ticket>> SearchAsync(string? searchTerm, string? status,
        string? priority, string? category, Guid? customerId, Guid? assignedAgentId,
        int page, int pageSize)
    {
        var query = _context.Tickets.Include(t => t.Comments).AsQueryable();

        //filtorwanie po wyszukiwanym terminie
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Title.Contains(searchTerm) ||
            t.Description.Contains(searchTerm));
        }

        // filtrowanie po statusie
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, out var ticketStatus))
        {
            query = query.Where(t => t.Status == ticketStatus);
        }
        // filtrowanie po priorytecie
        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, out var ticketPriority))
        {
            query = query.Where(t => t.Priority == ticketPriority);
        }
        // po kategorii
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<TicketCategory>(category, out var ticketCategory))
        {
            query = query.Where(t => t.Category == ticketCategory);
        }
        // po kliencie
        if (customerId.HasValue)
        {
            query = query.Where(t => t.CustomerId == customerId.Value);
        }
        // po przypisanym agencie
        if (assignedAgentId.HasValue)
        {
            query = query.Where(t => t.AssignedAgentId == assignedAgentId);
        }
        return await query.OrderByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        string? searchTerm, string? status, string? priority, string? category
        , Guid? customerId, Guid? assignedAgentId)
    {
        var query = _context.Tickets.AsQueryable();
        //taka sama logika jak wyzej
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t => t.Title.Contains(searchTerm) ||
            t.Description.Contains(searchTerm));
        }
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, out var ticketstatus))
        {
            query = query.Where(t => t.Status == ticketstatus);
        }
        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, out var ticketPriority))
        {
            query = query.Where(t => t.Priority == ticketPriority);
        }
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<TicketCategory>(category, out var ticketCategory))
        {
            query = query.Where(t => t.Category == ticketCategory);
        }
        if (customerId.HasValue)
        {
            query = query.Where(t => t.CustomerId == customerId);
        }
        if (assignedAgentId.HasValue)
        {
            query = query.Where(t => t.AssignedAgentId == assignedAgentId);
        }
        return await query.CountAsync();
    }

    public async Task<List<Ticket>> GetByAgentIdAsync(Guid agentId, int page, int pageSize)
    {
        return await _context.Tickets
            .Where(t => t.AssignedAgentId == agentId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    public async Task<List<Ticket>> GetByCustomerIdAsync(Guid customerId, int page, int pageSize)
    {
        return await _context.Tickets
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        ticket.Id = Guid.NewGuid();
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = TicketStatus.New;

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket> UpdateAsync(Ticket ticket)
    {
        ticket.UpdatedAt = DateTime.UtcNow;

        //jesli status s=zostanie zmieniony na resolved, zmieniamy resolved at na teraz
        if (ticket.Status == TicketStatus.Resolved && ticket.ResolvedAt == null)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }

        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();

        return ticket;
    }

    public async Task DeleteAsync(Guid id)
    {
        var ticket = await GetByIdAsync(id, includeComments: false);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Tickets.AnyAsync(t => t.Id == id);
    }

    //operacje na komentarzach

    public async Task<TicketComment> AddCommentAsync(TicketComment comment)
    {
        comment.Id = Guid.NewGuid();
        comment.CreatedAt = DateTime.UtcNow;

        _context.TicketComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }
    
    public async Task<List<TicketComment>> GetCommentsAsync(Guid ticketId, bool includeInternal = false)
    {
        var query = _context.TicketComments.
        Where(c => c.TicketId == ticketId);

        if (!includeInternal)
        {
            query = query.Where(c => !c.IsInternal);
        }

        return await query.OrderBy(c => c.CreatedAt).ToListAsync();
    }
}
