using MediatR;
using Shared.DTOs;

namespace TicketService.Features.Tickets.Commands.AssignToAgent;


public record AssignToAgentCommand : IRequest<TicketDto>
{
    public Guid TicketId{get;init;}
    public Guid AgentId{get;init;}
    
}