using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs;
using Shared.Models;
using System.Text.Json;


namespace UserService.Services;

public class CachedUserService : IUserService
{
    private readonly IUserService _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedUserService> _logger;

    private readonly DistributedCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(2)
    };

    public CachedUserService(IUserService userService, IDistributedCache distributedCache, ILogger<CachedUserService> logger)
    {
        _innerService = userService;
        _cache = distributedCache;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        string key = $"user-{id}";

        string? cachedUser = await _cache.GetStringAsync(key);
        if(!string.IsNullOrEmpty(cachedUser))
        {
            _logger.LogInformation("Returning user {id} from Redis Cache",id);
            return JsonSerializer.Deserialize<UserDto>(cachedUser);
        }

        var user = await _innerService.GetByIdAsync(id);

        if(user != null)
        {
         

            await _cache.SetStringAsync(key,JsonSerializer.Serialize(user),_cacheOptions);
        }
        return user;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        string key = $"user-email-{email.ToLower()}";

        string? cachedUser = await _cache.GetStringAsync(key);

        if(!string.IsNullOrEmpty(cachedUser))
        {
            _logger.LogInformation("Cache HIT: Returning user {Email} from redis",email);
            return JsonSerializer.Deserialize<UserDto>(cachedUser);
        }

        var user = await _innerService.GetByEmailAsync(email);

        if(user != null)
        {
            await _cache.SetStringAsync(key,JsonSerializer.Serialize(user),_cacheOptions);
        }
        return user;
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var updatedUser = await _innerService.UpdateAsync(id,request);

        await InvalidateUserCache(id,updatedUser.Email);
    
        return updatedUser;
    }

    public async Task<UserDto> AssignOrganizationAsync(Guid id, Guid organizationId)
    {
        var updatedUser = await _innerService.AssignOrganizationAsync(id,organizationId);

        await InvalidateUserCache(id,updatedUser.Email);

        return updatedUser;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _innerService.GetByIdAsync(id);

        await _innerService.DeleteAsync(id);

        if(user != null)
        {
            await InvalidateUserCache(id,user.Email);
        }
        else
        {
            await _cache.RemoveAsync($"user-{id}");
        }
    }

    public Task<UserListResponse> GetAllAsync(int page, int pageSize)
    {
        return _innerService.GetAllAsync(page,pageSize);
    }

    public Task<UserListResponse> SearchAsync(UserFilterRequest filterRequest)
    {
        return _innerService.SearchAsync(filterRequest);
    }

    public Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        return _innerService.CreateAsync(request);
    }

    public Task<UserDto> CreateAsync(CreateUserRequest request,Guid userId)
    {
        return _innerService.CreateAsync(request,userId);
    }
    public Task<bool> ExistsAsync(Guid id)
    {
        return _innerService.ExistsAsync(id);
    }

    private async Task InvalidateUserCache(Guid id, string email)
    {
        await _cache.RemoveAsync($"user-{id}");

        await _cache.RemoveAsync($"user-email-{email.ToLower()}");

        _logger.LogInformation("Cache invalidated for user {id}",id);
    }

}