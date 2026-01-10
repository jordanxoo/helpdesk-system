using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Caching;
using Shared.Events;
using TicketService.Data;

namespace TicketService.Consumers;

/// <summary>
/// Handles UserDeletedEvent to invalidate cache for all tickets related to the deleted user
/// </summary>
public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
{
    private readonly TicketDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserDeletedConsumer> _logger;

    public UserDeletedConsumer(
        TicketDbContext context,
        IDistributedCache cache,
        ILogger<UserDeletedConsumer> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received UserDeletedEvent for UserId: {UserId}, invalidating related ticket caches",
            message.UserId);

        try
        {
            // Find all tickets where the user is either customer or assigned agent
            var relatedTicketIds = await _context.Tickets
                .Where(t => t.CustomerId == message.UserId || t.AssignedAgentId == message.UserId)
                .Select(t => t.Id)
                .ToListAsync();

            if (relatedTicketIds.Count == 0)
            {
                _logger.LogInformation("No tickets found for deleted user {UserId}", message.UserId);
                return;
            }

            _logger.LogInformation("Found {Count} tickets related to deleted user {UserId}, invalidating cache",
                relatedTicketIds.Count, message.UserId);

            // Invalidate cache for each ticket
            foreach (var ticketId in relatedTicketIds)
            {
                try
                {
                    await _cache.RemoveAsync(CacheKeys.Ticket(ticketId));
                    await _cache.RemoveAsync($"ticket:{ticketId}:history");

                    _logger.LogDebug("Invalidated cache for ticket {TicketId} (user {UserId} deleted)",
                        ticketId, message.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to invalidate cache for ticket {TicketId}", ticketId);
                    // Continue processing other tickets even if one fails
                }
            }

            _logger.LogInformation("Successfully invalidated cache for {Count} tickets related to user {UserId}",
                relatedTicketIds.Count, message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserDeletedEvent for user {UserId}", message.UserId);
            throw; // Let MassTransit handle retry
        }
    }
}
