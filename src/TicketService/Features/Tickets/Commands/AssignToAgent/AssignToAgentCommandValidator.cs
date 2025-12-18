using FluentValidation;

namespace TicketService.Features.Tickets.Commands.AssignToAgent;


public class AssignToAgentCommandValidator : AbstractValidator<AssignToAgentCommand>
{
    public AssignToAgentCommandValidator()
    {
        RuleFor(x => x.TicketId)
        .NotEmpty().WithMessage("TicketId jest wymagany");

        RuleFor(x => x.AgentId)
        .NotEmpty().WithMessage("AgentId jest wymagany");
    }
}