using MediatR;
using Shared.DTOs;


namespace TicketService.Features.Tickets.Commands.CreateTicket;


public record CreateTicketCommand : IRequest<TicketDto>
{
    public string Title {get;init;} = string.Empty;
    public string Description{get;init;} = string.Empty;
    public string Priority{get;init;} = string.Empty;
    public string Category{get;init;} = string.Empty;

    public Guid? CustomerId {get;init;}
    public Guid? OrganizationId{get;init;}

    public Guid UserId{get;init;}
    public string UserRole {get;init;} = string.Empty;
}