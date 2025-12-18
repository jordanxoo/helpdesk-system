using MediatR;
using Shared.DTOs;

namespace TicketService.Features.Tickets.Commands.ChangeStatus;
public record ChangeStatusCommand : IRequest<TicketDto>
{
    public Guid TicketId{get;init;}
    public string NewStatus {get;init;} = string.Empty;
}