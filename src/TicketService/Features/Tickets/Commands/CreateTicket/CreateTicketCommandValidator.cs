using System.Data;
using FluentValidation;
using Shared.Models;


namespace TicketService.Features.Tickets.Commands.CreateTicket;


public class CreateTicketCommandValidatior : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidatior()
    {
        RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Tytuł jest wymagany")
        .MinimumLength(5).MaximumLength(200);

        RuleFor(x => x.Description)
        .NotEmpty().WithMessage("Opis jest wymagany")
        .MinimumLength(20);


        RuleFor(x => x.Priority)
        .NotEmpty().IsEnumName(typeof(TicketPriority), caseSensitive: false)
        .WithMessage("Nieprawidłowy priorytet");

        RuleFor(x => x.Category)
        .IsEnumName(typeof(TicketCategory), caseSensitive: false)
        .WithMessage("Nieprawidłowa kategoria");

        RuleFor(x => x)
            .Must(cmd => !(cmd.UserRole != "Customer" && cmd.CustomerId == null))
            .WithMessage("Agenci i Administratorzy muszą podać CustomerId");
    }
}
