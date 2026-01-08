using Microsoft.Extensions.Caching.Distributed;
using Shared.Caching;
using Shared.DTOs;
using Shared.Models;


namespace TicketService.Services;


public class CachedTicketService : ITicketService
{
    private readonly ITicketService _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedTicketService> _logger;

    private static readonly TimeSpan TicketCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan HistoryCacheExpiration = TimeSpan.FromMinutes(2);


    public CachedTicketService(
        ITicketService inner,
        IDistributedCache cache,
        ILogger<CachedTicketService> logger
    )
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TicketDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = CacheKeys.Ticket(id);

        var cached = await _cache.GetAsync<TicketDto>(cacheKey);

        if(cached is not null)
        {
            _logger.LogInformation("Cache HIT for ticket {TicketID}",id);
            return cached;
        }

        _logger.LogDebug("Cache MISS for ticket {TickedId}",id);
        var ticket = await _inner.GetByIdAsync(id);

        if(ticket is not null)
        {
            await _cache.SetAsync(cacheKey,ticket,TicketCacheExpiration);
        }

        return ticket;
    }

    public async Task<TicketListResponse> GetAllAsync(int page,int pageSize)
    {
        return await _inner.GetAllAsync(page,pageSize);
    }

    public async Task<TicketListResponse> SearchAsync(TicketFilterRequest request)
    {
        return await _inner.SearchAsync(request);
    }

    public async Task<TicketListResponse> GetMyTicketsAsync(Guid customerID, int page,int pageSize)
    {
        return await _inner.GetMyTicketsAsync(customerID,page,pageSize);
    }

    public async Task<TicketDto> CreateAsync(Guid userId, string userRole, CreateTicketRequest request)
    {
        return await _inner.CreateAsync(userId,userRole,request);
    }

    public async Task<TicketDto> UpdateAsync(Guid id, UpdateTicketRequest request)
    {
        var result = await _inner.UpdateAsync(id,request);

        await InvalidateTicketCacheAsync(id);
        return result;
    }

    public async Task<TicketDto> AssignToAgentAsync(Guid ticketId, Guid agentId)
    {
        var result = await _inner.AssignToAgentAsync(ticketId,agentId);
        await InvalidateTicketCacheAsync(ticketId);
        return result;
    }

    public async Task<TicketDto> UnassignAgentAsync(Guid ticketId)
    {
        var result = await _inner.UnassignAgentAsync(ticketId);
        await InvalidateTicketCacheAsync(ticketId);
        return result;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _inner.DeleteAsync(id);
        await InvalidateTicketCacheAsync(id);
    }
    public async Task<TicketDto> AddCommentAsync(Guid ticketID, Guid userID, AddCommentRequest request)
    {
        var result = await _inner.AddCommentAsync(ticketID,userID,request);
        await InvalidateTicketCacheAsync(ticketID);
        return result;
    }
    public async Task<List<TicketAuditLogDto>> GetHistoryAsync(Guid ticketID)
    {
        var cachedKey = $"ticket:{ticketID}:history";

        return await _cache.GetOrSetAsync(cachedKey,
        async () => await _inner.GetHistoryAsync(ticketID),
        HistoryCacheExpiration);
    }
     public async Task<TicketListResponse> GetAssignedTicketsAsync(Guid agentId, int page, int pageSize)
    {
        return await _inner.GetAssignedTicketsAsync(agentId, page, pageSize);
    }

     public async Task<TicketDto> ChangeStatusAsync(Guid ticketId, string newStatus)
    {
        var result = await _inner.ChangeStatusAsync(ticketId, newStatus);
        await InvalidateTicketCacheAsync(ticketId);
        return result;
    }

    public async Task<TicketDto> ChangePriorityAsync(Guid ticketId, string newPriority)
    {
        var result = await _inner.ChangePriorityAsync(ticketId, newPriority);
        await InvalidateTicketCacheAsync(ticketId);
        return result;
    }

    public async Task<TicketAttachment> AddAttachmentAsync(Guid ticketId, Guid userId, IFormFile file)
    {
        var result = await _inner.AddAttachmentAsync(ticketId,userId,file);
        await InvalidateTicketCacheAsync(ticketId);
        return result;
    }

    private async Task InvalidateTicketCacheAsync(Guid id)
    {
        var cacheKey = CacheKeys.Ticket(id);

        await _cache.RemoveAsync(cacheKey);
    
        await _cache.RemoveAsync($"ticket:{id}:history");

        _logger.LogDebug("Cache invalidated for ticket {TicketID}",id);

    }

    public Task<TicketStatisticsDto> GetStatisticsAsync()
    {
        return _inner.GetStatisticsAsync();
    }

}