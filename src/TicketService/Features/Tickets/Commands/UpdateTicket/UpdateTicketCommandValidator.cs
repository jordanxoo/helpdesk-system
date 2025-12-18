using Shared.Models;
using FluentValidation;
using Shared.DTOs;
using System.Data;



namespace TicketService.Features.Tickets.Commands.UpdateTicket;

public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty()
        .WithMessage("TicketId jest wymagany");

        RuleFor(x => x.Title)
        .MinimumLength(5).MaximumLength(200)
        .When(x => !string.IsNullOrEmpty(x.Title))
        .WithMessage("Tytuł musi mieć od 5 do 200 znaków");

        RuleFor(x => x.Description)
        .MinimumLength(20).When(x => !string.IsNullOrEmpty(x.Description))
        .WithMessage("Opis musi mieć conajmniej 20 znaków");

        RuleFor(x => x.Priority).IsEnumName(typeof(TicketPriority), caseSensitive: false)
        .When(x => !string.IsNullOrEmpty(x.Category))
        .WithMessage("Nieprawidłowa kategoria");
    }
}