using FluentValidation;
using Shared.Models;
using TicketService.Features.Tickets.Commands.ChangeStatus;

namespace TicketService.Features.Tickets.Commands.ChangeStatus;
public class ChangeStatusCommandValidator : AbstractValidator<ChangeStatusCommand>
{
    public ChangeStatusCommandValidator()
    {
        RuleFor(x => x.TicketId)
        .NotEmpty().WithMessage("TicketId jest wymagany");

        RuleFor(x => x.NewStatus)
        .NotEmpty().WithMessage("NewStatus jest wymagany")
        .IsEnumName(typeof(TicketStatus), caseSensitive: false)
        .WithMessage("Nieprawidłowy status: Dozwolone: New, Assigned, InProgress, Resolved, Closed, OnHold");
    }
}