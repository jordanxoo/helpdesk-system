using MediatR;
using Shared.DTOs;
namespace TicketService.Features.Tickets.Commands.UpdateTicket;

public record UpdateTicketCommand : IRequest<TicketDto>
{
    public Guid TicketId {get;init;}
    public string? Title {get;init;}
    public string? Description {get;init;}
    public string? Priority {get;init;}
    public string? Category{get;init;}
    public Guid? SlaID {get;init;}
}

