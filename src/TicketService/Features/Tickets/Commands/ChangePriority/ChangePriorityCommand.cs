using MediatR;
using Shared.DTOs;

namespace TicketService.Features.Tickets.Commands.ChangePriority;

public record ChangePriorityCommand : IRequest<TicketDto>
{
    public Guid TicketId { get; init; }
    public string NewPriority { get; init; } = string.Empty;
}
