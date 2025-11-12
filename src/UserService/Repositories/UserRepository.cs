namespace UserService.Repositories;

using Microsoft.EntityFrameworkCore;
using Shared.Models;
using UserService.Data;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<List<User>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Users
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<User>> SearchAsync(string? searchTerm, string? role, bool? isActive, int page, int pageSize)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Email.Contains(searchTerm) ||
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, out var userRole))
        {
            query = query.Where(u => u.Role == userRole);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm, string? role, bool? isActive)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u =>
                u.Email.Contains(searchTerm) ||
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, out var userRole))
        {
            query = query.Where(u => u.Role == userRole);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        return await query.CountAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        // Jeśli ID nie jest ustawione (Guid.Empty), wygeneruj nowe
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }
        
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if(user != null)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateAsync(user);
        }
    }

}